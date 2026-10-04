using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace DAL
{
    public class PaymentDAL
    {
        DB db;

        public PaymentDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public PaymentDAL(DB db)
        {
            this.db = db;
        }

        /// <summary>
        /// Money already received against one invoice, optionally excluding one
        /// payment.
        ///
        /// A SQL aggregate rather than a sum over invoice.Payments, because on the
        /// write path the row being written is not in the collection and on the
        /// void path the row being voided still is. Asking the database settles
        /// both cases the same way, and it is the only figure that cannot be out
        /// of step with what is actually stored.
        ///
        /// The cast to decimal? is not cosmetic. SUM over zero rows returns NULL,
        /// and materialising NULL into a non-nullable decimal throws
        /// "The cast to value type 'System.Decimal' failed because the
        /// materialized value is null" rather than yielding zero. Summing the
        /// nullable and coalescing in C# is the portable spelling of the
        /// ISNULL(SUM(...), 0) the old stored procedure used.
        /// </summary>
        private static decimal PaidSoFar(DB context, int invoiceId, int excludingPaymentId)
        {
            return context.Payments
                .Where(p => p.Invoice.id == invoiceId
                         && p.DeleteStatus == false
                         && (excludingPaymentId == 0 || p.Id != excludingPaymentId))
                .Sum(p => (decimal?)p.Amount) ?? 0m;
        }

        /// <summary>
        /// Records a payment and re-derives the invoice's checkout flag in the
        /// same transaction, so the flag and the payment list can never be seen
        /// disagreeing.
        ///
        /// validate is injected from BLL (SettlementPolicy.ValidateRecording)
        /// because the DAL assembly cannot reference BLL (BLL references DAL). It
        /// is rethrown rather than returned: the form catches
        /// InvalidOperationException and shows the message as a warning, which is
        /// the pattern InvoiceDAL.Create established.
        ///
        /// This runs on the context it was given rather than opening one per call,
        /// which is what makes it testable against a throwaway database file: a
        /// per-call `new DB()` resolves to the process-wide DataSource, so a test
        /// would silently write into the developer's real database. The cost is
        /// that a rollback can leave work pending on a shared context, which is
        /// what DiscardPendingChanges is for.
        /// </summary>
        public string Create(Payment payment, int invoiceId,
            Func<decimal, decimal, DateTime, DateTime, InvalidOperationException> validate)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var invoice = db.Invoices
                        .Include("Lines")
                        .FirstOrDefault(i => i.id == invoiceId);

                    if (invoice == null)
                    {
                        transaction.Rollback();
                        return "فاکتور مورد نظر یافت نشد";
                    }

                    if (invoice.DeleteStatus)
                    {
                        transaction.Rollback();
                        return "فاکتور حذف شده و قابل وصولی نیست";
                    }

                    decimal balance = Math.Max(0m, invoice.Payable - PaidSoFar(db, invoiceId, 0));
                    var rejection = validate(payment.Amount, balance, payment.RegDate, DateTime.Now);
                    if (rejection != null) throw rejection;

                    payment.Invoice = invoice;
                    if (payment.User != null)
                    {
                        payment.User = db.Users.Find(payment.User.id);
                    }

                    db.Payments.Add(payment);
                    SettlementFlag.Apply(invoice, PaidSoFar(db, invoiceId, 0) + payment.Amount);

                    db.SaveChanges();
                    transaction.Commit();
                    return "ثبت وصولی با موفقیت انجام شد";
                }
                catch (InvalidOperationException)
                {
                    transaction.Rollback();
                    DiscardPendingChanges();
                    throw;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    DiscardPendingChanges();
                    return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
                }
            }
        }

        /// <summary>
        /// Soft-deletes a payment and re-derives the flag, in one transaction.
        /// There is deliberately no edit: a payment may already have a printed
        /// receipt in the customer's hand, so a correction is a void plus a fresh
        /// payment rather than an amendment.
        /// </summary>
        public string Void(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var payment = db.Payments
                        .Include("Invoice.Lines")
                        .FirstOrDefault(p => p.Id == id);

                    if (payment == null)
                    {
                        transaction.Rollback();
                        return "وصولی مورد نظر یافت نشد";
                    }

                    var invoice = payment.Invoice;
                    payment.DeleteStatus = true;

                    SettlementFlag.Apply(invoice, PaidSoFar(db, invoice.id, payment.Id));

                    db.SaveChanges();
                    transaction.Commit();
                    return "ابطال وصولی با موفقیت انجام شد";
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    DiscardPendingChanges();
                    return "ابطال وصولی با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
                }
            }
        }

        /// <summary>
        /// A rolled-back SaveChanges leaves the work pending on the context, and
        /// the next successful SaveChanges on the same context would flush it
        /// along with the real thing - so a rejected payment would surface later as
        /// an unrequested second payment. Nothing this class adds or changes is
        /// ever wanted after a failure, so every entity left in Added or Modified
        /// state is detached. Deleted is left alone: nothing here removes rows.
        /// </summary>
        private void DiscardPendingChanges()
        {
            foreach (var entry in db.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList())
            {
                entry.State = EntityState.Detached;
            }
        }

        // Copied from the projection below, in the same order: the grid binds with
        // AutoGenerateColumns, so these strings are the visible headers.
        private static readonly string[] ReadColumns =
        {
            "شماره وصولی", "شماره فاکتور", "نام مشتری", "تاریخ وصول",
            "مبلغ", "روش پرداخت", "شماره پیگیری", "ثبت توسط",
        };

        /// <summary>
        /// The rows behind the grid, newest receipt first.
        ///
        /// Include("Invoice.Customer") is not optional: EF6 does not lazy load, so
        /// without it the customer column sees a null navigation. It is a reference
        /// include, so it is a plain join - a collection under this collection is
        /// what SQLite cannot serve.
        /// </summary>
        private List<object[]> PaymentRows()
        {
            return db.Payments
                .Include("Invoice.Customer")
                .Include("User")
                .Where(p => p.DeleteStatus == false)
                .OrderByDescending(p => p.Id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(p => new object[]
                {
                    p.Id,
                    p.Invoice?.id,
                    p.Invoice?.Customer?.Name,
                    p.RegDate,
                    p.Amount,
                    PaymentInstrumentTitles.Of(p.Instrument),
                    p.Reference,
                    // Name-then-UserName, the fallback every other report uses.
                    p.User?.Name ?? p.User?.UserName,
                })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, PaymentRows());
        }

        public DataTable Search(string Filter)
        {
            // Filtered after materialisation because string.Contains in a
            // LINQ-to-Entities Where needs CHARINDEX, which SQLite lacks. The
            // invoice number and the customer name are the two things an employee
            // knows when hunting for a receipt.
            var rows = PaymentRows()
                .Where(r => GridTable.Matches(
                    Filter,
                    Convert.ToString(r[1]),
                    Convert.ToString(r[2])))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }

        public string Count()
        {
            return db.Payments.Where(p => p.DeleteStatus == false).Count().ToString();
        }

        /// <summary>Full payment for the receipt: the payment, its invoice and its customer.</summary>
        public Payment ReadById(int id)
        {
            // Every include here hangs off the Payment's reference to its Invoice,
            // so each is a plain join. Nothing may be hung off Invoice as a
            // collection of collections.
            return db.Payments
                .Include("Invoice.Customer")
                .Include("Invoice.Lines")
                .Include("Invoice.Payments")
                .Include("User")
                .FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Customers who still owe something, most indebted first.
        ///
        /// The arithmetic lives in BalanceQuery because it is needed in three
        /// places - here, the customer grid and the dashboard - and the shape it
        /// has to take is a provider constraint rather than a preference.
        /// </summary>
        public List<CustomerBalance> ReadCustomerBalances()
        {
            return BalanceQuery.LoadDebtors(db);
        }
    }
}