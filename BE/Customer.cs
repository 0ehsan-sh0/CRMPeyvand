using System;
using System.Collections.Generic;
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

        // What this customer owes is deliberately NOT a property here. It is a sum
        // over the customer's invoices, and summing it means loading every
        // invoice's lines and payments - which SQLite cannot be asked to do in one
        // query, because a collection include under a collection include makes EF6
        // emit APPLY. A property that reads as 0 whenever the collections happen
        // not to be loaded is worse than no property, so the figure is computed
        // where it is loaded: DAL.BalanceQuery, which reads two flat queries and
        // returns a DAL.CustomerBalance per customer.
    }
}
