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
        public List<Payment> Payments { get; set; } = new List<Payment>();

        [NotMapped] public decimal SubTotal => Lines.Sum(l => l.LineTotal);
        [NotMapped] public decimal Payable => Math.Max(0m, SubTotal - DiscountAmount);
        [NotMapped] public int TotalQuantity => Lines.Sum(l => l.Quantity);

        /// <summary>
        /// Money actually received against this invoice, ignoring voided payments:
        /// a void is a soft delete, so the flag alone is not enough.
        ///
        /// Read paths use this. The write path in PaymentDAL recomputes the same
        /// figure as a SQL aggregate, because a payment being written is not yet in
        /// this collection.
        /// </summary>
        [NotMapped] public decimal Paid =>
            (Payments ?? new List<Payment>()).Where(p => !p.DeleteStatus).Sum(p => p.Amount);

        [NotMapped] public decimal Balance => Math.Max(0m, Payable - Paid);

        [NotMapped] public bool IsSettled => Balance <= 0m;
    }
}
