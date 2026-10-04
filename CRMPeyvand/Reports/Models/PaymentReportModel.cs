namespace CRMPeyvand.Reports.Models
{
    /// <summary>
    /// One رسید دریافت. Money is double, as it is on every other report model in
    /// this project; the entity side is decimal and the cast happens in the factory.
    /// </summary>
    public class PaymentReportModel
    {
        public string ReceiptTitle { get; set; } = "رسید دریافت";
        public string ReceiptNumber { get; set; } = string.Empty;
        public string PaymentDatePersian { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceDatePersian { get; set; } = string.Empty;
        public double InvoiceTotal { get; set; }
        public double InvoicePaidAfterThisReceipt { get; set; }
        public double Amount { get; set; }
        public double RemainingBalance { get; set; }
        public string InstrumentTitle { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string ReceivedBy { get; set; } = string.Empty;
    }
}