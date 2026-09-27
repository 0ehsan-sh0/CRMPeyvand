using BE;
using CRMPeyvand.Reports.Services;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class InvoiceReportModelFactoryTests
    {
        private static Invoice SampleInvoice()
        {
            return new Invoice
            {
                id = 42,
                RegDate = new System.DateTime(2026, 3, 21),
                OffCode = "OFF10",
                DiscountAmount = 1500m,
                Customer = new Customer { id = 3, Name = "زهرا", Phone = "09120000000" },
                User = new User { id = 1, Name = "احسان" },
                Lines = new System.Collections.Generic.List<InvoiceLine>
                {
                    new InvoiceLine { Id = 2, CatalogItemId = 20, Quantity = 2, UnitPrice = 1000m,
                                      CatalogItem = new CatalogItem { Id = 20, Name = "کابل شبکه" } },
                    new InvoiceLine { Id = 1, CatalogItemId = 10, Quantity = 1, UnitPrice = 2500m,
                                      CatalogItem = new CatalogItem { Id = 10, Name = "خدمات نصب" } }
                }
            };
        }

        [Fact]
        public void Maps_invoice_identity_customer_and_discount()
        {
            var model = InvoiceReportModelFactory.FromInvoice(SampleInvoice());

            Assert.Equal("42", model.InvoiceNumber);
            Assert.Equal("1405/01/01", model.IssueDatePersian);
            Assert.Equal("زهرا", model.CustomerName);
            Assert.Equal("09120000000", model.CustomerPhone);
            Assert.Equal(1500d, model.DiscountAmount);
        }

        [Fact]
        public void Items_are_one_based_and_ordered_by_line_id()
        {
            var model = InvoiceReportModelFactory.FromInvoice(SampleInvoice());

            Assert.Equal(2, model.Items.Count);
            Assert.Equal(1, model.Items[0].RowIndex);
            Assert.Equal("خدمات نصب", model.Items[0].ItemName);
            Assert.Equal(2, model.Items[1].RowIndex);
            Assert.Equal("کابل شبکه", model.Items[1].ItemName);
        }

        [Fact]
        public void Totals_are_derived_from_lines_minus_discount()
        {
            var model = InvoiceReportModelFactory.FromInvoice(SampleInvoice());

            Assert.Equal(4500d, model.SubTotal);
            Assert.Equal(3000d, model.FinalTotal);
        }

        [Fact]
        public void Note_carries_discount_code_and_issuer()
        {
            var model = InvoiceReportModelFactory.FromInvoice(SampleInvoice());

            Assert.Equal("کد تخفیف: OFF10 - ثبت توسط: احسان", model.Note);
        }

        [Fact]
        public void Missing_relations_do_not_throw_and_yield_empty_model()
        {
            var bare = new Invoice { id = 7, RegDate = new System.DateTime(2026, 3, 21) };

            var model = InvoiceReportModelFactory.FromInvoice(bare);

            Assert.Equal("7", model.InvoiceNumber);
            Assert.Equal(string.Empty, model.CustomerName);
            Assert.Equal(string.Empty, model.CustomerPhone);
            Assert.Equal(string.Empty, model.Note);
            Assert.Empty(model.Items);
        }

        [Fact]
        public void Line_without_loaded_catalog_item_still_maps()
        {
            var invoice = new Invoice
            {
                id = 9,
                RegDate = new System.DateTime(2026, 3, 21),
                Lines = new System.Collections.Generic.List<InvoiceLine>
                {
                    new InvoiceLine { Id = 1, Quantity = 3, UnitPrice = 100m }
                }
            };

            var model = InvoiceReportModelFactory.FromInvoice(invoice);

            Assert.Equal(string.Empty, Assert.Single(model.Items).ItemName);
        }

        [Fact]
        public void Null_invoice_yields_empty_model()
        {
            var model = InvoiceReportModelFactory.FromInvoice(null);

            Assert.Equal(string.Empty, model.InvoiceNumber);
            Assert.Equal(0d, model.SubTotal);
            Assert.Empty(model.Items);
        }
    }
}
