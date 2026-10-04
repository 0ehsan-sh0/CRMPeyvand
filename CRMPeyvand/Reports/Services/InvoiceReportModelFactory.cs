using BE;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using System.Collections.Generic;
using System.Linq;

namespace CRMPeyvand.Reports.Services
{
    /// <summary>
    /// Single mapping from a persisted Invoice to the printable/displayable report model,
    /// so the details screen and the PDF can never disagree about an invoice.
    /// </summary>
    public static class InvoiceReportModelFactory
    {
        public static InvoiceReportModel FromInvoice(Invoice invoice)
        {
            if (invoice == null)
                return new InvoiceReportModel();

            return new InvoiceReportModel
            {
                InvoiceNumber = invoice.id.ToString(),
                IssueDatePersian = PersianReportStyle.FormatPersianDate(invoice.RegDate),
                CustomerName = invoice.Customer?.Name ?? string.Empty,
                CustomerPhone = invoice.Customer?.Phone ?? string.Empty,
                DiscountAmount = (double)invoice.DiscountAmount,
                PaidAmount = (double)invoice.Paid,
                RemainingBalance = (double)invoice.Balance,
                Items = BuildItems(invoice),
                Note = BuildNote(invoice)
            };
        }

        private static List<InvoiceItemRowModel> BuildItems(Invoice invoice)
        {
            var lines = invoice.Lines ?? new List<InvoiceLine>();
            return lines
                .OrderBy(l => l.Id)
                .Select((line, index) => new InvoiceItemRowModel
                {
                    RowIndex = index + 1,
                    ItemName = line.CatalogItem?.Name ?? string.Empty,
                    Quantity = line.Quantity,
                    UnitPrice = (double)line.UnitPrice
                })
                .ToList();
        }

        private static string BuildNote(Invoice invoice)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(invoice.OffCode))
                parts.Add($"کد تخفیف: {invoice.OffCode}");

            string issuedBy = invoice.User?.Name ?? invoice.User?.UserName;
            if (!string.IsNullOrWhiteSpace(issuedBy))
                parts.Add($"ثبت توسط: {issuedBy}");

            return string.Join(" - ", parts);
        }
    }
}
