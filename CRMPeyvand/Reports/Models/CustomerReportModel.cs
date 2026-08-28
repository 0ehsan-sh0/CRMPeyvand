using System.Collections.Generic;

namespace CRMPeyvand.Reports.Models
{
    public class CustomerRowModel
    {
        public int RowIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string RegDatePersian { get; set; } = string.Empty;
    }

    public class CustomerReportModel
    {
        public string ReportTitle { get; set; } = "گزارش مشتریان";
        public string GeneratedDatePersian { get; set; } = string.Empty;
        public List<CustomerRowModel> Customers { get; set; } = new();
        public int TotalCount => Customers.Count;
    }
}
