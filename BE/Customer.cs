using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Customer
    {
        public Customer()
        {
            DeleteStatus = false;
        }
        public int id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime RegDate { get; set; }
        public bool DeleteStatus { get; set; }
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public List<Activity> Activities { get; set; } = new List<Activity>();

        /// <summary>
        /// What this customer still owes across their live invoices.
        ///
        /// All three walk Invoices, so any query reading them must Include the
        /// invoices' Lines and Payments too — EF6 does not lazy load. See
        /// CustomerDAL.Read and PaymentDAL.ReadCustomerBalances.
        /// </summary>
        [NotMapped] public decimal PayableTotal =>
            Invoices.Where(i => !i.DeleteStatus).Sum(i => i.Payable);

        [NotMapped] public decimal PaidTotal =>
            Invoices.Where(i => !i.DeleteStatus).Sum(i => i.Paid);

        [NotMapped] public decimal Balance => Math.Max(0m, PayableTotal - PaidTotal);
    }
}
