using System;

namespace BE
{
    public class Payment
    {
        public Payment()
        {
            DeleteStatus = false;
            Instrument = PaymentInstrument.Cash;
            RegDate = DateTime.Now;
        }

        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime RegDate { get; set; }
        public PaymentInstrument Instrument { get; set; }
        public string Reference { get; set; }
        public bool DeleteStatus { get; set; }

        public Invoice Invoice { get; set; }
        public User User { get; set; }
    }
}