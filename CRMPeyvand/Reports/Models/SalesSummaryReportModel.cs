using System.Collections.Generic;
using System.Linq;

namespace CRMPeyvand.Reports.Models
{
    public class UserSalesRowModel
    {
        public int RowIndex { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int InvoicesCount { get; set; }
        public double TotalAmount { get; set; }
    }

    public class SalesSummaryReportModel
    {
        public string ReportTitle { get; set; } = "گزارش فروش";
        public string GeneratedDatePersian { get; set; } = string.Empty;
        public string StartDatePersian { get; set; } = string.Empty;
        public string EndDatePersian { get; set; } = string.Empty;
        public List<UserSalesRowModel> UserSales { get; set; } = new();
        public int TotalInvoicesCount => UserSales.Sum(u => u.InvoicesCount);
        public double GrandTotalAmount => UserSales.Sum(u => u.TotalAmount);
    }
}
