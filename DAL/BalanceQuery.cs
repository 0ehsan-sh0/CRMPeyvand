using BE;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DAL
{
    /// <summary>
    /// Works out what every customer owes, from two flat queries.
    ///
    /// The obvious spelling - db.Customers.Include("Invoices.Lines") - does not
    /// work here. A collection include under a collection include makes EF6 emit
    /// APPLY, and the SQLite provider answers "APPLY joins are not supported", the
    /// same class of portability trap as the DbFunctions.TruncateTime and
    /// CHARINDEX problems this codebase already documents elsewhere. So the
    /// invoices are loaded from their own root, where a collection include is a
    /// plain LEFT JOIN, and the two sets are zipped in memory.
    ///
    /// AsNoTracking on both queries is load-bearing rather than an optimisation.
    /// The dashboard and the reports window each hold one context for as long as
    /// they are open, and a tracked entity keeps the collection it was loaded
    /// with - so without this the debtor count would freeze at whatever it was the
    /// first time the screen was opened.
    /// </summary>
    internal static class BalanceQuery
    {
        /// <summary>
        /// One entry per live customer, debtors first then alphabetically, so two
        /// callers can present the list in a stable order without sorting again.
        /// </summary>
        internal static List<CustomerBalance> Load(DB db)
        {
            var customers = db.Customers
                .AsNoTracking()
                .Where(c => c.DeleteStatus == false)
                .ToList();

            var invoices = db.Invoices
                .AsNoTracking()
                .Include("Customer")
                .Include("Lines")
                .Include("Payments")
                .Where(i => i.DeleteStatus == false)
                .ToList();

            var balances = customers.ToDictionary(c => c.id, c => new CustomerBalance(c));

            foreach (var invoice in invoices)
            {
                if (invoice.Customer == null || !balances.TryGetValue(invoice.Customer.id, out var balance))
                {
                    // An invoice whose customer was deleted outright. Soft-deleted
                    // customers are already excluded above, so this only happens
                    // if the row went away under a hard delete.
                    continue;
                }

                balance.InvoiceCount++;
                balance.PayableTotal += invoice.Payable;
                balance.PaidTotal += invoice.Paid;
            }

            foreach (var balance in balances.Values)
            {
                balance.Balance = System.Math.Max(0m, balance.PayableTotal - balance.PaidTotal);
            }

            return balances.Values
                .OrderByDescending(b => b.Balance)
                .ThenBy(b => b.Customer.Name)
                .ToList();
        }

        /// <summary>Only the customers who still owe something.</summary>
        internal static List<CustomerBalance> LoadDebtors(DB db)
        {
            return Load(db).Where(b => b.Balance > 0m).ToList();
        }
    }
}