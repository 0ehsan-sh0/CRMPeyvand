using System;
using System.Collections.Generic;
using BE;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using CRMPeyvand.Reports.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class PaymentReportTests
    {
        public PaymentReportTests()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// An invoice with two payments on it, returning the second. Pass
        /// alreadyPaid = 0 for an invoice that has never been touched.
        /// </summary>
        private static Payment PaymentWith(decimal invoiceTotal, decimal alreadyPaid, decimal amount)
        {
            var customer = new Customer { Name = "علی رضایی", Phone = "09121234567" };
            var invoice = new Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m, Customer = customer };
            invoice.Lines.Add(new InvoiceLine
            {
                Quantity = 1,
                UnitPrice = invoiceTotal,
                CatalogItem = new CatalogItem { Name = "کالا", Kind = ItemKind.Good, SalePrice = invoiceTotal },
            });
invoice.Payments.Add(new Payment
            {
                Amount = alreadyPaid,
                RegDate = new DateTime(2026, 10, 2),
                Invoice = invoice,
            });
            invoice.Payments.Add(new Payment
            {
                Amount = amount,
                RegDate = new DateTime(2026, 10, 4),
                Invoice = invoice,
            });

            return invoice.Payments[1];
        }

private static void AssertRendersPdf(IDocument document)
        {
            byte[] pdfBytes = document.GeneratePdf();

            Assert.True(pdfBytes.Length > 0);
            Assert.Equal((byte)'%', pdfBytes[0]);
            Assert.Equal((byte)'P', pdfBytes[1]);
            Assert.Equal((byte)'D', pdfBytes[2]);
            Assert.Equal((byte)'F', pdfBytes[3]);
        }

        [Fact]
        public void The_receipt_carries_the_numbers_the_customer_needs()
        {
            var model = PaymentReportModelFactory.FromPayment(PaymentWith(10000m, 4000m, 6000m));

            Assert.Equal("رسید دریافت", model.ReceiptTitle);
            Assert.Equal("10000", model.InvoiceTotal.ToString("0"));
            Assert.Equal("10000", model.InvoicePaidAfterThisReceipt.ToString("0"));
            Assert.Equal("6000", model.Amount.ToString("0"));
            Assert.Equal("0", model.RemainingBalance.ToString("0"));
            Assert.Equal("علی رضایی", model.CustomerName);
            Assert.Equal("09121234567", model.CustomerPhone);
        }

        [Fact]
        public void The_receipt_shows_what_is_still_owed_after_a_partial_payment()
        {
            var model = PaymentReportModelFactory.FromPayment(PaymentWith(10000m, 4000m, 2000m));

            Assert.Equal("6000", model.InvoicePaidAfterThisReceipt.ToString("0"));
            Assert.Equal("4000", model.RemainingBalance.ToString("0"));
        }

        [Fact]
        public void A_receipt_for_a_first_payment_adds_it_to_nothing_previous()
        {
            var model = PaymentReportModelFactory.FromPayment(PaymentWith(10000m, 0m, 1000m));

            Assert.Equal("1000", model.InvoicePaidAfterThisReceipt.ToString("0"));
            Assert.Equal("9000", model.RemainingBalance.ToString("0"));
        }

        [Fact]
        public void A_voided_payment_does_not_count_towards_the_receipts_running_total()
        {
            var payment = PaymentWith(10000m, 6000m, 4000m);
            payment.Invoice.Payments[0].DeleteStatus = true;

            var model = PaymentReportModelFactory.FromPayment(payment);

            Assert.Equal("4000", model.InvoicePaidAfterThisReceipt.ToString("0"));
            Assert.Equal("6000", model.RemainingBalance.ToString("0"));
        }

        [Fact]
        public void The_receipt_names_the_instrument_and_the_operator()
        {
            var payment = PaymentWith(10000m, 0m, 1000m);
            payment.Instrument = PaymentInstrument.Cheque;
            payment.Reference = "7788";
            payment.User = new User { Name = "احسان", UserName = "ehsan" };

            var model = PaymentReportModelFactory.FromPayment(payment);

            Assert.Equal("چک", model.InstrumentTitle);
            Assert.Equal("7788", model.Reference);
            Assert.Equal("احسان", model.ReceivedBy);
        }

        [Fact]
        public void A_receipt_falls_back_to_the_username_when_the_operator_has_no_name()
        {
            var payment = PaymentWith(10000m, 0m, 1000m);
            payment.User = new User { UserName = "ehsan" };

            Assert.Equal("ehsan", PaymentReportModelFactory.FromPayment(payment).ReceivedBy);
        }

        [Fact]
        public void An_absent_reference_prints_as_empty_rather_than_null()
        {
            var payment = PaymentWith(10000m, 0m, 1000m);
            payment.Reference = null;

            Assert.Equal(string.Empty, PaymentReportModelFactory.FromPayment(payment).Reference);
        }

        [Fact]
        public void A_null_payment_still_produces_a_printable_receipt()
        {
            var model = PaymentReportModelFactory.FromPayment(null);

            Assert.Equal("رسید دریافت", model.ReceiptTitle);
            AssertRendersPdf(new PaymentReceiptDocument(model));
        }

        [Fact]
        public void The_receipt_renders()
        {
            AssertRendersPdf(new PaymentReceiptDocument(
                PaymentReportModelFactory.FromPayment(PaymentWith(10000m, 4000m, 6000m))));
        }

        [Fact]
        public void The_statement_totals_the_balance_column()
        {
            var model = new CustomerBalanceReportModel
            {
                Customers = new List<CustomerBalanceRowModel>
                {
                    new CustomerBalanceRowModel { RowIndex = 1, Name = "علی", Balance = 5000d },
                    new CustomerBalanceRowModel { RowIndex = 2, Name = "سمیرا", Balance = 2500d },
                },
            };

Assert.Equal(2, model.DebtorCount);
            Assert.Equal(7500d, model.TotalBalance);
        }

        [Fact]
        public void An_empty_statement_totals_zero_rather_than_null()
        {
            var model = new CustomerBalanceReportModel();

Assert.Equal(0, model.DebtorCount);
            Assert.Equal(0d, model.TotalBalance);
        }

        [Fact]
        public void The_statement_renders()
        {
            var model = new CustomerBalanceReportModel
            {
                GeneratedDatePersian = "1405/07/12",
                Customers = new List<CustomerBalanceRowModel>
                {
                    new CustomerBalanceRowModel
                    {
                        RowIndex = 1,
                        Name = "علی رضایی",
                        Phone = "09121234567",
                        InvoiceCount = 3,
                        TotalBilled = 12000d,
                        TotalReceived = 7000d,
                        Balance = 5000d,
                    },
                },
            };

            AssertRendersPdf(new CustomerBalanceDocument(model));
        }

        [Fact]
        public void An_empty_statement_renders_rather_than_throwing()
        {
            AssertRendersPdf(new CustomerBalanceDocument(null));
        }
    }
}