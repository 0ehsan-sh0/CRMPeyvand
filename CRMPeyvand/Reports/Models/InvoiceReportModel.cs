using System.Collections.Generic;
using System.Linq;

namespace CRMPeyvand.Reports.Models
{
    public class InvoiceItemRowModel
    {
        public int RowIndex { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice => Quantity * UnitPrice;
    }

    public class InvoiceReportModel
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string IssueDatePersian { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public List<InvoiceItemRowModel> Items { get; set; } = new();
        public double DiscountAmount { get; set; }
        public double SubTotal => Items.Sum(i => i.TotalPrice);
        public double FinalTotal => SubTotal - DiscountAmount;
        public string Note { get; set; } = string.Empty;
    }
}
