using System.Collections.Generic;
using System.Linq;

namespace CRMPeyvand.Reports.Models
{
    public class CatalogItemRowModel
    {
        public int RowIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public int Stock { get; set; }
        public double Price { get; set; }
    }

    public class CatalogItemReportModel
    {
        public string ReportTitle { get; set; } = "گزارش محصولات و خدمات";
        public string GeneratedDatePersian { get; set; } = string.Empty;
        public List<CatalogItemRowModel> Items { get; set; } = new();
        public int TotalItemsCount => Items.Count;
        public int TotalStock => Items.Sum(i => i.Stock);
        public double TotalValue => Items.Sum(i => i.Stock * i.Price);
    }
}
