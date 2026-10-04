using BE;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using System;

namespace CRMPeyvand.Reports.Services
{
    /// <summary>
    /// Single mapping from a persisted Payment to the printable receipt, so the
    /// screen and the PDF can never disagree about a payment. Null-tolerant in the
    /// same way InvoiceReportModelFactory is.
    /// </summary>
    public static class PaymentReportModelFactory
    {
        public static PaymentReportModel FromPayment(Payment payment)
        {
            if (payment == null)
                return new PaymentReportModel();

            var invoice = payment.Invoice;

            // What the invoice has received including this receipt. Invoice.Paid
            // already covers the payment when the caller loaded the whole graph;
            // the extra term covers the caller holding a payment saved after the
            // invoice's payments were read.
            double paidAfter = invoice == null
                ? 0d
                : (double)(invoice.Paid + (invoice.Payments == null || invoice.Payments.Contains(payment)
                        ? 0m
                        : payment.Amount));

            double total = invoice == null ? 0d : (double)invoice.Payable;

            return new PaymentReportModel
            {
                ReceiptNumber = payment.Id.ToString(),
                PaymentDatePersian = PersianReportStyle.FormatPersianDate(payment.RegDate),
                CustomerName = invoice?.Customer?.Name ?? string.Empty,
                CustomerPhone = invoice?.Customer?.Phone ?? string.Empty,
                InvoiceNumber = invoice?.id.ToString() ?? string.Empty,
                InvoiceDatePersian = invoice == null
                    ? string.Empty
                    : PersianReportStyle.FormatPersianDate(invoice.RegDate),
                InvoiceTotal = total,
                InvoicePaidAfterThisReceipt = paidAfter,
                Amount = (double)payment.Amount,
                RemainingBalance = Math.Max(0d, total - paidAfter),
                InstrumentTitle = PaymentInstrumentTitles.Of(payment.Instrument),
                Reference = payment.Reference ?? string.Empty,
                // Name-then-UserName, the fallback every other report uses.
                ReceivedBy = payment.User?.Name ?? payment.User?.UserName ?? string.Empty,
            };
        }
    }
}