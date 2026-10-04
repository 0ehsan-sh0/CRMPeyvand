using System;
using System.Data;
using System.Linq;
using BE;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    [Collection(DataSourceCollection.Name)]
    public class PaymentDALTests
    {
        private static readonly Func<decimal, decimal, DateTime, DateTime, InvalidOperationException> Allow =
            BLL.SettlementPolicy.ValidateRecording;

        private static readonly DateTime PayDay = new DateTime(2026, 10, 4);

        private static Customer SeedCustomer(DB db, string phone = "09120000000", string name = "مشتری آزمایشی")
        {
            var customer = new Customer { Name = name, Phone = phone, RegDate = new DateTime(2026, 10, 1) };
            db.Customers.Add(customer);
            db.SaveChanges();
            return customer;
        }

        private static Invoice SeedInvoice(DB db, Customer customer, decimal unitPrice, decimal discount = 0m)
        {
            var invoice = new Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = discount, Customer = customer };
            invoice.Lines.Add(new InvoiceLine
            {
                Quantity = 1,
                UnitPrice = unitPrice,
                CatalogItem = new CatalogItem { Name = "کالا", Kind = ItemKind.Good, SalePrice = unitPrice, Stock = 10 },
            });
            db.Invoices.Add(invoice);
            db.SaveChanges();
            return invoice;
        }

        [Fact]
        public void Recording_a_payment_reduces_the_balance_and_leaves_the_invoice_unsettled()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);

                var result = new PaymentDAL(db).Create(
                    new Payment { Amount = 4000m, RegDate = PayDay, Instrument = PaymentInstrument.Cash },
                    invoice.id, Allow);

                Assert.Equal("ثبت وصولی با موفقیت انجام شد", result);

                var reloaded = db.Invoices.Include("Payments").Single();
                Assert.Equal(4000m, reloaded.Paid);
                Assert.Equal(6000m, reloaded.Balance);
                Assert.False(reloaded.IsCheckedout);
            });
        }

        [Fact]
        public void Settling_the_remainder_flips_the_checkout_flag_and_stamps_the_date()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);

                new PaymentDAL(db).Create(new Payment { Amount = 10000m, RegDate = PayDay }, invoice.id, Allow);

                var reloaded = db.Invoices.Single();
                Assert.True(reloaded.IsCheckedout);
                Assert.NotNull(reloaded.CheckoutDate);
            });
        }

        [Fact]
        public void The_balance_is_measured_after_the_discount_not_the_subtotal()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m, discount: 3000m);

                new PaymentDAL(db).Create(new Payment { Amount = 7000m, RegDate = PayDay }, invoice.id, Allow);

                Assert.True(db.Invoices.AsNoTracking().Single().IsCheckedout);
            });
        }

        [Fact]
        public void Two_payments_accumulate()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                dal.Create(new Payment { Amount = 3000m, RegDate = new DateTime(2026, 10, 2) }, invoice.id, Allow);
                dal.Create(new Payment { Amount = 2000m, RegDate = new DateTime(2026, 10, 3) }, invoice.id, Allow);

                Assert.Equal(5000m, db.Invoices.Include("Payments").Single().Paid);
            });
        }

        [Fact]
        public void Voiding_a_payment_reopens_the_invoice()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                dal.Create(new Payment { Amount = 10000m, RegDate = PayDay }, invoice.id, Allow);
                Assert.True(db.Invoices.AsNoTracking().Single().IsCheckedout);

                Assert.Equal("ابطال وصولی با موفقیت انجام شد", dal.Void(db.Payments.Single().Id));

                var reloaded = db.Invoices.AsNoTracking().Include("Payments").Single();
                Assert.False(reloaded.IsCheckedout);
                Assert.Null(reloaded.CheckoutDate);
                Assert.Equal(0m, reloaded.Paid);
            });
        }

        [Fact]
        public void Voiding_one_of_two_payments_reopens_the_invoice()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                dal.Create(new Payment { Amount = 4000m, RegDate = new DateTime(2026, 10, 2) }, invoice.id, Allow);
                dal.Create(new Payment { Amount = 6000m, RegDate = new DateTime(2026, 10, 3) }, invoice.id, Allow);
                Assert.True(db.Invoices.AsNoTracking().Single().IsCheckedout);

                dal.Void(db.Payments.OrderBy(p => p.Id).First().Id);

                // 4000 of the 10000 is gone, so the invoice is open again.
                var reloaded = db.Invoices.AsNoTracking().Single();
                Assert.False(reloaded.IsCheckedout);
                Assert.Null(reloaded.CheckoutDate);
                Assert.Equal(6000m, db.Payments.Where(p => !p.DeleteStatus).Sum(p => p.Amount));
            });
        }

        [Fact]
        public void Voiding_something_that_does_not_exist_is_reported_not_thrown()
        {
            SqliteTestDb.WithDb(db =>
            {
                Assert.Equal("وصولی مورد نظر یافت نشد", new PaymentDAL(db).Void(999));
            });
        }

        [Fact]
        public void An_unknown_invoice_is_reported_not_thrown()
        {
            SqliteTestDb.WithDb(db =>
            {
                var message = new PaymentDAL(db).Create(new Payment { Amount = 1000m }, 999, Allow);

                Assert.Equal("فاکتور مورد نظر یافت نشد", message);
                Assert.Empty(db.Payments);
            });
        }

        [Fact]
        public void A_deleted_invoice_cannot_take_a_payment()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                invoice.DeleteStatus = true;
                db.SaveChanges();

                var message = new PaymentDAL(db).Create(new Payment { Amount = 1000m }, invoice.id, Allow);

                Assert.Equal("فاکتور حذف شده و قابل وصولی نیست", message);
                Assert.Empty(db.Payments);
            });
        }

        [Fact]
        public void An_overpayment_is_refused_and_stores_nothing()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                Assert.Throws<InvalidOperationException>(
                    () => dal.Create(new Payment { Amount = 10001m, RegDate = PayDay }, invoice.id, Allow));

                Assert.Empty(db.Payments);
                Assert.False(db.Invoices.Single().IsCheckedout);
            });
        }

        [Fact]
        public void A_payment_dated_in_the_future_is_refused_and_stores_nothing()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                Assert.Throws<InvalidOperationException>(
                    () => dal.Create(new Payment { Amount = 1000m, RegDate = new DateTime(2030, 1, 1) }, invoice.id, Allow));

                Assert.Empty(db.Payments);
            });
        }

        [Fact]
        public void The_payment_keeps_its_instrument_and_reference()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);

                new PaymentDAL(db).Create(new Payment
                {
                    Amount = 1000m,
                    RegDate = PayDay,
                    Instrument = PaymentInstrument.Cheque,
                    Reference = "123456",
                }, invoice.id, Allow);

                var stored = db.Payments.Single();
                Assert.Equal(PaymentInstrument.Cheque, stored.Instrument);
                Assert.Equal("123456", stored.Reference);
            });
        }

        [Fact]
        public void The_payment_is_attributed_to_the_operator_who_took_it()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var operatorUser = new User { Name = "احسان", UserName = "ehsan", RegDate = new DateTime(2026, 1, 1) };
                db.Users.Add(operatorUser);
                db.SaveChanges();

                new PaymentDAL(db).Create(
                    new Payment { Amount = 1000m, RegDate = PayDay, User = operatorUser },
                    invoice.id, Allow);

                Assert.Equal("ehsan", db.Payments.Include("User").Single().User.UserName);
            });
        }

        [Fact]
        public void The_grid_carries_the_receipt_number_the_invoice_the_customer_and_the_amount()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);

                new PaymentDAL(db).Create(new Payment
                {
                    Amount = 2500m,
                    RegDate = PayDay,
                    Instrument = PaymentInstrument.Card,
                    Reference = "7788",
                }, invoice.id, Allow);

                var table = new PaymentDAL(db).Read();

                Assert.Equal(
                    new[]
                    {
                        "شماره وصولی", "شماره فاکتور", "نام مشتری", "تاریخ وصول",
                        "مبلغ", "روش پرداخت", "شماره پیگیری", "ثبت توسط",
                    },
                    table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

                Assert.Equal(invoice.id, Convert.ToInt32(table.Rows[0]["شماره فاکتور"]));
                Assert.Equal("مشتری آزمایشی", table.Rows[0]["نام مشتری"]);
                Assert.Equal(2500m, Convert.ToDecimal(table.Rows[0]["مبلغ"]));
                Assert.Equal("کارت", table.Rows[0]["روش پرداخت"]);
                Assert.Equal("7788", table.Rows[0]["شماره پیگیری"]);
            });
        }

        [Fact]
        public void The_grid_shows_the_most_recent_receipt_first()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);
                dal.Create(new Payment { Amount = 1000m, RegDate = new DateTime(2026, 10, 2) }, invoice.id, Allow);
                dal.Create(new Payment { Amount = 2000m, RegDate = new DateTime(2026, 10, 3) }, invoice.id, Allow);

                var table = dal.Read();

                Assert.Equal(2000m, Convert.ToDecimal(table.Rows[0]["مبلغ"]));
                Assert.Equal(1000m, Convert.ToDecimal(table.Rows[1]["مبلغ"]));
            });
        }

        [Fact]
        public void A_voided_payment_leaves_the_grid()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);
                dal.Create(new Payment { Amount = 1000m, RegDate = PayDay }, invoice.id, Allow);

                dal.Void(db.Payments.Single().Id);

                Assert.Empty(dal.Read().Rows);
            });
        }

        [Fact]
        public void Searching_the_grid_matches_the_invoice_number_and_the_customer_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                var first = SeedCustomer(db, "09120000001", "علی رضایی");
                var second = SeedCustomer(db, "09120000002", "سمیرا محمدی");

                var dal = new PaymentDAL(db);
                var firstInvoice = SeedInvoice(db, first, 5000m);
                var secondInvoice = SeedInvoice(db, second, 5000m);
                dal.Create(new Payment { Amount = 1000m, RegDate = PayDay }, firstInvoice.id, Allow);
                dal.Create(new Payment { Amount = 1000m, RegDate = PayDay }, secondInvoice.id, Allow);

                Assert.Single(dal.Search("رضایی").Rows);
                Assert.Single(dal.Search(secondInvoice.id.ToString()).Rows);
                Assert.Empty(dal.Search("هیچ‌کس").Rows);
            });
        }

        [Fact]
        public void The_customer_balance_list_keeps_only_those_who_owe_something()
        {
            SqliteTestDb.WithDb(db =>
            {
                var debtor = SeedCustomer(db, "09120000001");
                var settled = SeedCustomer(db, "09120000002");
                SeedInvoice(db, debtor, 10000m);
                var paidInFull = SeedInvoice(db, settled, 5000m);
                new PaymentDAL(db).Create(new Payment { Amount = 5000m, RegDate = PayDay }, paidInFull.id, Allow);

                var balances = new PaymentDAL(db).ReadCustomerBalances();

                Assert.Single(balances);
                Assert.Equal("09120000001", balances[0].Customer.Phone);
                Assert.Equal(10000m, balances[0].Balance);
                Assert.Equal(1, balances[0].InvoiceCount);
            });
        }

        [Fact]
        public void The_customer_balance_list_ignores_voided_payments()
        {
            SqliteTestDb.WithDb(db =>
            {
                var customer = SeedCustomer(db);
                var invoice = SeedInvoice(db, customer, 10000m);
                new PaymentDAL(db).Create(new Payment { Amount = 10000m, RegDate = PayDay }, invoice.id, Allow);

                Assert.Empty(new PaymentDAL(db).ReadCustomerBalances());
            });
        }

        [Fact]
        public void The_customer_balance_list_sums_several_invoices_and_skips_deleted_ones()
        {
            SqliteTestDb.WithDb(db =>
            {
                var customer = SeedCustomer(db);

                var first = SeedInvoice(db, customer, 10000m);
                SeedInvoice(db, customer, 6000m);
                var discarded = SeedInvoice(db, customer, 100000m);
                discarded.DeleteStatus = true;
                db.SaveChanges();

                new PaymentDAL(db).Create(new Payment { Amount = 4000m, RegDate = PayDay }, first.id, Allow);

                var balances = new PaymentDAL(db).ReadCustomerBalances();

                Assert.Single(balances);
                Assert.Equal(2, balances[0].InvoiceCount);
                Assert.Equal(16000m, balances[0].PayableTotal);
                Assert.Equal(4000m, balances[0].PaidTotal);
                Assert.Equal(12000m, balances[0].Balance);
            });
        }

        [Fact]
        public void Voiding_the_only_payment_puts_the_customer_back_on_the_debtor_list()
        {
            SqliteTestDb.WithDb(db =>
            {
                var customer = SeedCustomer(db);
                var invoice = SeedInvoice(db, customer, 10000m);
                var dal = new PaymentDAL(db);
                dal.Create(new Payment { Amount = 10000m, RegDate = PayDay }, invoice.id, Allow);
                Assert.Empty(dal.ReadCustomerBalances());

                dal.Void(db.Payments.Single().Id);

                var balances = dal.ReadCustomerBalances();
                Assert.Single(balances);
                Assert.Equal(10000m, balances[0].Balance);
            });
        }
    }
}