using BE;
using System;
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
            Assert.Equal(0d, model.PaidAmount);
            Assert.Equal(0d, model.RemainingBalance);
            Assert.Empty(model.Items);
        }

        [Fact]
        public void Paid_and_remaining_come_from_the_invoices_payments()
        {
            var customer = new BE.Customer { Name = "مشتری", Phone = "09120000000" };
            var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m, Customer = customer };
            invoice.Lines.Add(new BE.InvoiceLine { Quantity = 1, UnitPrice = 10000m });
            invoice.Payments.Add(new BE.Payment { Amount = 4000m, RegDate = new DateTime(2026, 10, 2) });

            var model = InvoiceReportModelFactory.FromInvoice(invoice);

            Assert.Equal(4000d, model.PaidAmount);
            Assert.Equal(6000d, model.RemainingBalance);
        }

        [Fact]
        public void An_invoice_with_no_payments_owes_its_full_total()
        {
            var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m };
            invoice.Lines.Add(new BE.InvoiceLine { Quantity = 1, UnitPrice = 7000m });

            var model = InvoiceReportModelFactory.FromInvoice(invoice);

            Assert.Equal(0d, model.PaidAmount);
            Assert.Equal(7000d, model.RemainingBalance);
        }

        [Fact]
        public void A_settled_invoice_reports_nothing_outstanding()
        {
            var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m };
            invoice.Lines.Add(new BE.InvoiceLine { Quantity = 1, UnitPrice = 7000m });
            invoice.Payments.Add(new BE.Payment { Amount = 7000m, RegDate = new DateTime(2026, 10, 2) });

            var model = InvoiceReportModelFactory.FromInvoice(invoice);

            Assert.Equal(7000d, model.PaidAmount);
            Assert.Equal(0d, model.RemainingBalance);
        }

        [Fact]
        public void A_voided_payment_does_not_reduce_what_the_invoice_shows_as_owed()
        {
            var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m };
            invoice.Lines.Add(new BE.InvoiceLine { Quantity = 1, UnitPrice = 7000m });
            invoice.Payments.Add(new BE.Payment { Amount = 7000m, RegDate = new DateTime(2026, 10, 2), DeleteStatus = true });

            var model = InvoiceReportModelFactory.FromInvoice(invoice);

            Assert.Equal(0d, model.PaidAmount);
            Assert.Equal(7000d, model.RemainingBalance);
        }
    }
}
