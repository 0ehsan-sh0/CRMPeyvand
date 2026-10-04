using System.Collections.Generic;
using System.Linq;

namespace CRMPeyvand.Reports.Models
{
    /// <summary>
    /// One customer's outstanding position. Per customer rather than per invoice,
    /// because the question this answers is "who owes me and how much".
    /// </summary>
    public class CustomerBalanceRowModel
    {
        public int RowIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int InvoiceCount { get; set; }
        public double TotalBilled { get; set; }
        public double TotalReceived { get; set; }
        public double Balance { get; set; }
    }

    public class CustomerBalanceReportModel
    {
        public string ReportTitle { get; set; } = "گزارش مانده حساب مشتریان";
        public string GeneratedDatePersian { get; set; } = string.Empty;
        public List<CustomerBalanceRowModel> Customers { get; set; } = new();
        public int DebtorCount => Customers.Count;
        public double TotalBalance => Customers.Sum(c => c.Balance);
    }
}