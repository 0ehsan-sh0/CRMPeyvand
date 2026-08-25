using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        public int id { get; set; }                 // human-facing number == id
        public DateTime RegDate { get; set; }
        public bool IsCheckedout { get; set; }
        public Nullable<DateTime> CheckoutDate { get; set; }
        public bool DeleteStatus { get; set; }
        public string OffCode { get; set; }
        public decimal DiscountAmount { get; set; }

        public Customer Customer { get; set; }
        public User User { get; set; }
        public List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();

        [NotMapped] public decimal SubTotal => Lines.Sum(l => l.LineTotal);
        [NotMapped] public decimal Payable => Math.Max(0m, SubTotal - DiscountAmount);
        [NotMapped] public int TotalQuantity => Lines.Sum(l => l.Quantity);
    }
}
