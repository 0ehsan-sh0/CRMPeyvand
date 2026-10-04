using System;
using System.Linq;
using BE;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    [Collection(DataSourceCollection.Name)]
    public class DashboardDALTests
    {
        private static void SeedDebt(DB db, string phone, decimal unitPrice, decimal paid)
        {
            var customer = new Customer { Name = phone, Phone = phone, RegDate = new DateTime(2026, 10, 1) };
            var invoice = new Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m, Customer = customer };
            invoice.Lines.Add(new InvoiceLine
            {
                Quantity = 1,
                UnitPrice = unitPrice,
                CatalogItem = new CatalogItem { Name = "کالا", Kind = ItemKind.Good, SalePrice = unitPrice, Stock = 10 },
            });
            invoice.Payments.Add(new Payment { Amount = paid, RegDate = new DateTime(2026, 10, 2) });
            db.Customers.Add(customer);
            db.Invoices.Add(invoice);
            db.SaveChanges();
        }

        [Fact]
        public void The_debtor_count_ignores_customers_who_have_paid_in_full()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedDebt(db, "09120000001", 10000m, 4000m);
                SeedDebt(db, "09120000002", 5000m, 5000m);

                Assert.Equal("1", new DashboardDAL(db).DebtorCustomerCount());
            });
        }

        [Fact]
        public void The_debtor_count_ignores_voided_payments()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedDebt(db, "09120000001", 10000m, 10000m);
                Assert.Equal("0", new DashboardDAL(db).DebtorCustomerCount());

                db.Payments.Single().DeleteStatus = true;
                db.SaveChanges();

                // The money is gone, so the customer owes the invoice again.
                Assert.Equal("1", new DashboardDAL(db).DebtorCustomerCount());
            });
        }

        [Fact]
        public void The_debtor_count_ignores_deleted_invoices()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedDebt(db, "09120000001", 10000m, 0m);
                db.Invoices.Single().DeleteStatus = true;
                db.SaveChanges();

                Assert.Equal("0", new DashboardDAL(db).DebtorCustomerCount());
            });
        }

        [Fact]
        public void The_debtor_count_counts_a_customer_once_however_many_invoices_they_owe_on()
        {
            SqliteTestDb.WithDb(db =>
            {
                var busy = new BE.Customer { Name = "بدهکار", Phone = "09120000001", RegDate = new DateTime(2026, 10, 1) };
                db.Customers.Add(busy);
                db.SaveChanges();
                SeedInvoiceFor(db, busy, 10000m);
                SeedInvoiceFor(db, busy, 7000m);

                var other = new BE.Customer { Name = "بدهکار دیگر", Phone = "09120000002", RegDate = new DateTime(2026, 10, 1) };
                db.Customers.Add(other);
                db.SaveChanges();
                SeedInvoiceFor(db, other, 3000m);

                Assert.Equal("2", new DashboardDAL(db).DebtorCustomerCount());
            });
        }

        private static void SeedInvoiceFor(DB db, BE.Customer customer, decimal unitPrice)
        {
            var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m, Customer = customer };
            invoice.Lines.Add(new BE.InvoiceLine
            {
                Quantity = 1,
                UnitPrice = unitPrice,
                CatalogItem = new BE.CatalogItem { Name = "کالا", Kind = BE.ItemKind.Good, SalePrice = unitPrice, Stock = 10 },
            });
            db.Invoices.Add(invoice);
            db.SaveChanges();
        }

        [Fact]
        public void The_debtor_count_is_zero_for_an_empty_book()
        {
            SqliteTestDb.WithDb(db =>
            {
                Assert.Equal("0", new DashboardDAL(db).DebtorCustomerCount());
            });
        }
    }
}