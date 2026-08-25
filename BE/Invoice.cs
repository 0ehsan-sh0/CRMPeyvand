using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Invoice
    {
        public Invoice()
        {
            DeleteStatus = false;
            IsCheckedout = false;
            CheckoutDate = null;
        }
        public int id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime RegDate { get; set; }
        public bool IsCheckedout { get; set; }
        public Nullable<DateTime> CheckoutDate { get; set; }
        public int TotalCount { get; set; }
        public double TotalPrice { get; set; }
        public bool DeleteStatus { get; set; }
        public string OffCode { get; set; }
        public Customer Customer { get; set; }
        public User User { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();

    }
}
