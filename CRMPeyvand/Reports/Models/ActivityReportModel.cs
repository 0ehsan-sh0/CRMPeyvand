using System.Collections.Generic;

namespace CRMPeyvand.Reports.Models
{
    public class ActivityRowModel
    {
        public int RowIndex { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DatePersian { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ActivityReportModel
    {
        public string ReportTitle { get; set; } = "گزارش فعالیت‌ها";
        public string GeneratedDatePersian { get; set; } = string.Empty;
        public string StartDatePersian { get; set; } = string.Empty;
        public string EndDatePersian { get; set; } = string.Empty;
        public List<ActivityRowModel> Activities { get; set; } = new();
        public int TotalCount => Activities.Count;
    }
}
