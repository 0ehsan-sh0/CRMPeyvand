using BE;

namespace DAL
{
    /// <summary>
    /// What one customer owes, in the shape the grids and the statement need.
    ///
    /// A DTO rather than the Customer entity because a Customer's balance cannot be
    /// computed from a Customer alone: it is a sum over that customer's invoices,
    /// and summing it means loading every invoice's lines and payments. See
    /// BalanceQuery for why that is done with two flat queries.
    ///
    /// Public because it crosses into BLL and on into a report model. It is a flat
    /// bag of figures with no behaviour, so there is nothing to encapsulate.
    /// </summary>
    public class CustomerBalance
    {
        public CustomerBalance(Customer customer)
        {
            Customer = customer;
        }

        public Customer Customer { get; }

        public int InvoiceCount { get; set; }
        public decimal PayableTotal { get; set; }
        public decimal PaidTotal { get; set; }
        public decimal Balance { get; set; }
    }
}