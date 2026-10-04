using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class DashboardDAL
    {
        DB db;

        public DashboardDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public DashboardDAL(DB db)
        {
            this.db = db;
        }

        public string CustomersCount()
        {
            try
            {
                return db.Customers.Count(i => i.DeleteStatus == false).ToString();
            }
            catch
            {
                return "0";
            }
        }

        /// <summary>
        /// Invoices registered today, as a Persian-formatted counter string.
        ///
        /// This used to read
        /// <c>DbFunctions.TruncateTime(RegDate) == today</c>, which compiles
        /// to a SQL Server TRUNCATE and has no SQLite implementation: the
        /// provider answers "no such function: TruncateTime" at runtime, and
        /// the catch below would turn that into a dashboard reading of 0. The
        /// half-open range below is the same question asked of the column, so
        /// the provider can also use an index on RegDate - wrapping the column
        /// in TRUNCATE never could.
        ///
        /// Both ends of the range are computed here rather than inside the
        /// predicate because LINQ to Entities translates the expression tree
        /// and cannot evaluate a method call against a captured variable:
        /// <c>i.RegDate &lt; today.Date.AddDays(1)</c> throws
        /// NotSupportedException, and a private static helper would throw the
        /// same way. Only the comparisons cross into SQL; the arithmetic does
        /// not.
        /// </summary>
        public string SellsCountToday()
        {
            try
            {
                var from = DateTime.Today.Date;
                var to = from.AddDays(1);
                return db.Invoices.Count(i =>
                    i.DeleteStatus == false &&
                    i.RegDate >= from &&
                    i.RegDate < to).ToString();
            }
            catch
            {
                return "0";
            }
        }

        /// <summary>
        /// Invoices registered in the last seven days, counted while this
        /// method runs. This was a raw SqlCommand carrying
        /// <c>RegDate BETWEEN DATEADD(WEEK, -1, GETDATE()) AND GETDATE()</c>,
        /// which is a rolling window on both providers. <c>AddDays(-7)</c>
        /// is the same window, and the upper bound stays inclusive because
        /// BETWEEN's is.
        /// </summary>
        public string SellsCountWeek()
        {
            try
            {
                var to = DateTime.Now;
                var from = to.AddDays(-7);
                return db.Invoices.Count(i =>
                    i.DeleteStatus == false &&
                    i.RegDate >= from &&
                    i.RegDate <= to).ToString();
            }
            catch
            {
                return "0";
            }
        }

        public string UserReminderCount(User u)
        {
            if (u == null) return "0";
            try
            {
                // Same same-day range as SellsCountToday; see there for why
                // TruncateTime is gone and why the ends are computed here.
                var from = DateTime.Today.Date;
                var to = from.AddDays(1);
                return db.Reminders.Count(i =>
                    i.User.id == u.id &&
                    i.DeleteStatus == false &&
                    i.IsReminded == false &&
                    i.RemindDate >= from &&
                    i.RemindDate < to).ToString();
            }
            catch
            {
                return "0";
            }
        }

        public List<Reminder> GetUserReminder(User user)
        {
            if (user == null) return new List<Reminder>();
            try
            {
                var from = DateTime.Today.Date;
                var to = from.AddDays(1);
                return db.Reminders.Include("User")
                    .Where(i =>
                        i.User.id == user.id &&
                        i.DeleteStatus == false &&
                        i.IsReminded == false &&
                        i.RemindDate >= from &&
                        i.RemindDate < to)
                    .ToList();
            }
            catch
            {
                return new List<Reminder>();
            }
        }

        /// <summary>
        /// How many customers owe something, as a counter string.
        ///
        /// Counted in memory over the book rather than in SQL because the balance
        /// is a sum over an invoice's lines and payments, and SQLite cannot be
        /// asked for all three in one query. This is the only dashboard figure that
        /// materialises; the rest of this class is a plain Count, so on a very
        /// large book this is the expensive card. Try/catch to "0" like its
        /// neighbours.
        /// </summary>
        public string DebtorCustomerCount()
        {
            try
            {
                return BalanceQuery.LoadDebtors(db).Count.ToString();
            }
            catch
            {
                return "0";
            }
        }

        public bool PanelIsActive()
        {
            try
            {
                return db.MessagePanels.Any();
            }
            catch
            {
                return false;
            }
        }
    }
}
