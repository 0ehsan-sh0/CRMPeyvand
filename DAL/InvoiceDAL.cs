using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class InvoiceDAL
    {
        DB db;

        public InvoiceDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public InvoiceDAL(DB db)
        {
            this.db = db;
        }

        // Creates invoice + lines + optional settling payment atomically, validates/
        // decrements Goods stock, returns saved invoice with id.
        // validateStock / computeDiscount are injected from BLL (StockPolicy.Validate /
        // Pricing.ComputeDiscount) because the DAL assembly cannot reference BLL
        // (BLL references DAL). settledBy is the «وضعیت پرداخت» checkbox: when the
        // employee ticks it while composing the invoice, the full payable arrived at
        // the counter, so a real Payment is written here inside the transaction that
        // creates the invoice. It used to be a second call to Done() in a second
        // context afterwards, which was never atomic and recorded no amount.
        public Invoice Create(Invoice invoice, int customerId, IReadOnlyList<InvoiceLine> lines,
            Func<CatalogItem, int, InvalidOperationException> validateStock,
            Func<OffCode, decimal, decimal> computeDiscount,
            Payment settledBy)
        {
            // Per-call context: a rollback must not leave modified entities pending in a
            // shared long-lived tracker (phantom decrements would flush on the next SaveChanges).
            using (var db = new DB())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        invoice.Customer = db.Customers.Find(customerId);
                        invoice.User = db.Users.Find(invoice.User.id);
                        foreach (var line in lines)
                        {
                            var item = db.CatalogItems.Find(line.CatalogItemId);
                            var rejection = validateStock(item, line.Quantity);
                            if (rejection != null) throw rejection;
                            if (item.Kind == ItemKind.Good) item.Stock -= line.Quantity;
                            line.UnitPrice = item.SalePrice;
                            line.CatalogItem = item;
                            invoice.Lines.Add(line);
                        }
                        invoice.DiscountAmount = computeDiscount(
                            string.IsNullOrEmpty(invoice.OffCode)
                                ? null
                                : db.OffCodes.FirstOrDefault(o => o.Code == invoice.OffCode),
                            invoice.SubTotal);
                        db.Invoices.Add(invoice);
                        if (settledBy != null)
                        {
                            // The amount can only be known now: it is the payable,
                            // and the payable depends on the discount the line
                            // above just computed.
                            settledBy.Amount = invoice.Payable;
                            settledBy.RegDate = invoice.RegDate;
                            settledBy.User = invoice.User;
                            settledBy.Invoice = invoice;
                            db.Payments.Add(settledBy);
                            invoice.Payments.Add(settledBy);
                            SettlementFlag.Apply(invoice, invoice.Payable);
                        }
                        db.SaveChanges();
                        transaction.Commit();
                        return invoice;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public int ReadInvoiceLastID()
        {
            var q = db.Invoices.OrderByDescending(i => i.id).FirstOrDefault();
            return q.id;
        }


        public string CountInvoices()
        {
            return db.Invoices.Where(i => i.DeleteStatus == false).Count().ToString();
        }


        // Copied from the AS aliases of the SQL this replaces, in its order: the
        // two computed columns come last, not in the middle. There is no "قیمت کل"
        // here - that alias lived only in the stored procedure dropped in the
        // first task - and no customer or user column either, because the live
        // query had no joins.
        private static readonly string[] ReadColumns =
        {
            "شماره فاکتور", "وضعیت پرداخت", "تاریخ پرداخت", "کد تخفیف", "تاریخ ثبت",
            "تعداد کالاهای فاکتور", "مبلغ کل فاکتور", "مبلغ پرداخت شده", "مانده حساب",
        };

        /// <summary>
        /// The rows behind the grid, newest first.
        ///
        /// Include("Lines") replaces the pair of correlated subqueries the old
        /// SQL ran per invoice over InvoiceLines, and Include("Payments") is what
        /// makes «مبلغ پرداخت شده» and «مانده حساب» real numbers rather than
        /// zeros. Neither is optional: EF6 does not lazy load, so without them the
        /// sums see empty collections.
        ///
        /// The projection itself is deliberately in memory rather than in the
        /// SQL, because EF6 cannot project a collection aggregate - and neither
        /// provider can do the grouping LINQ would need to express it. Sums over
        /// the one materialised collection are a single pass.
        /// </summary>
        private List<object[]> InvoiceRows()
        {
            return db.Invoices
                .Include("Lines")
                .Include("Payments")
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[]
                {
                    i.id,
                    // Persian rather than the bool. The column carried a bool in an
                    // object[] and rendered the literal text "True"/"False".
                    i.IsCheckedout ? "پرداخت شده" : "پرداخت نشده",
                    i.CheckoutDate,
                    i.OffCode,
                    i.RegDate,
                    // ISNULL(SUM(l.Quantity), 0) becomes Sum, which returns 0
                    // for an empty sequence rather than null.
                    i.Lines.Sum(l => l.Quantity),
                    // Payable rather than the raw expression this used to carry:
                    // it is clamped, and clamping matters now that a payment is
                    // measured against it.
                    i.Payable,
                    i.Paid,
                    i.Balance,
                })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, InvoiceRows());
        }

        public string Delete(int id)
        {
            try
            {
                var q = db.Invoices.Find(id);
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "فاکتور مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }


        public Invoice ReadById(int id)
        {
            return db.Invoices.Find(id);
        }

        // Full invoice for the details view: navigation properties are not lazy loaded, so they must be pulled in explicitly.
        // Payments is what backs «مبلغ پرداخت شده» and «مانده حساب» on the screen.
        public Invoice ReadDetails(int id)
        {
            return db.Invoices
                .Include("Customer")
                .Include("User")
                .Include("Lines.CatalogItem")
                .Include("Payments")
                .FirstOrDefault(i => i.id == id);
        }
        public DataTable Search(string Filter)
        {
            // The old clause was CONVERT(nvarchar(max), i.id) LIKE
            // N'%' + @search + N'%' - the invoice number and nothing else, not
            // the discount code or the payment status. The same substring match
            // is applied to the first column after materialisation, because
            // Contains in a LINQ-to-Entities Where clause needs CHARINDEX, which
            // SQLite does not have.
            var rows = InvoiceRows()
                .Where(r => GridTable.Matches(Filter, Convert.ToString(r[0])))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
    }
}
