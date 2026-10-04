using System;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class SqliteSchemaTests
    {
        [Fact]
        public void Ensure_creates_every_table_the_context_maps()
        {
            string[] expected =
            {
                "AccessGrants", "UserGroups", "Users", "Activities", "ActivityCategories",
                "Customers", "Invoices", "InvoiceLines", "CatalogItems", "Reminders",
                "MessagePanels", "Messages", "OffCodes", "RememberMes", "Payments",
            };

            var found = new System.Collections.Generic.List<string>();
            SqliteTestDb.WithRaw(connection =>
            {
                DAL.SqliteSchema.Ensure(connection);
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            found.Add(reader.GetString(0));
                }
            });

            foreach (var table in expected)
                Assert.Contains(table, found);
        }

        [Fact]
        public void Ensure_is_idempotent()
        {
            SqliteTestDb.WithRaw(connection =>
            {
                DAL.SqliteSchema.Ensure(connection);
                DAL.SqliteSchema.Ensure(connection);   // must not throw
            });
        }

        [Fact]
        public void Ef_can_insert_and_read_a_customer()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "مشتری آزمایشی",
                    Phone = "09120000000",
                    RegDate = new DateTime(2026, 9, 29),
                });
                db.SaveChanges();

                var found = db.Customers.Single();
                Assert.Equal("مشتری آزمایشی", found.Name);
            });
        }

        [Fact]
        public void Ef_generates_incrementing_identity_keys()
        {
            SqliteTestDb.WithDb(db =>
            {
                for (var i = 0; i < 3; i++)
                    db.Customers.Add(new BE.Customer { Name = "c" + i, Phone = "0912000000" + i, RegDate = DateTime.Now });
                db.SaveChanges();

                var ids = db.Customers.OrderBy(c => c.id).Select(c => c.id).ToList();
                Assert.Equal(new[] { 1, 2, 3 }, ids);
            });
        }

        [Fact]
        public void A_payment_round_trips_through_the_context_with_its_relations()
        {
            SqliteTestDb.WithDb(db =>
            {
                var customer = new BE.Customer { Name = "مشتری", Phone = "09120000001", RegDate = new DateTime(2026, 10, 4) };
                db.Customers.Add(customer);
                db.SaveChanges();

                var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 4), DiscountAmount = 0m, Customer = customer };
                invoice.Lines.Add(new BE.InvoiceLine
                {
                    Quantity = 1,
                    UnitPrice = 4000m,
                    CatalogItem = new BE.CatalogItem { Name = "کالا", Kind = BE.ItemKind.Good, SalePrice = 4000m, Stock = 5 },
                });
                db.Invoices.Add(invoice);
                db.SaveChanges();

                db.Payments.Add(new BE.Payment
                {
                    Amount = 1500m,
                    RegDate = new DateTime(2026, 10, 4),
                    Instrument = BE.PaymentInstrument.Card,
                    Reference = "123456",
                    Invoice = invoice,
                });
                db.SaveChanges();

                var stored = db.Payments.Include("Invoice").Single();
                Assert.Equal(1500m, stored.Amount);
                Assert.Equal(BE.PaymentInstrument.Card, stored.Instrument);
                Assert.Equal("123456", stored.Reference);
                Assert.Equal(invoice.id, stored.Invoice.id);
            });
        }
    }
}
