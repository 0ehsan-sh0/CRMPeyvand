using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class DashboardDAL
    {
        public string CustomersCount()
        {
            try
            {
                using (var db = new DB())
                {
                    return db.Customers.Count(i => i.DeleteStatus == false).ToString();
                }
            }
            catch
            {
                return "0";
            }
        }

        public string SellsCountToday()
        {
            try
            {
                using (var db = new DB())
                {
                    DateTime today = DateTime.Today;
                    return db.Invoices.Count(i => i.DeleteStatus == false && System.Data.Entity.DbFunctions.TruncateTime(i.RegDate) == today).ToString();
                }
            }
            catch
            {
                return "0";
            }
        }

        public string SellsCountWeek()
        {
            try
            {
                string queryString = "SELECT COUNT(*) FROM Invoices WHERE (DeleteStatus = 0) AND Invoices.RegDate BETWEEN DATEADD(WEEK, -1, GETDATE()) AND GETDATE()";
                string connectionString = DB.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    object countResult = command.ExecuteScalar();
                    return countResult != null ? countResult.ToString() : "0";
                }
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
                using (var db = new DB())
                {
                    DateTime today = DateTime.Today;
                    return db.Reminders.Count(i => i.User.id == u.id && i.DeleteStatus == false && i.IsReminded == false && System.Data.Entity.DbFunctions.TruncateTime(i.RemindDate) == today).ToString();
                }
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
                using (var db = new DB())
                {
                    DateTime today = DateTime.Today;
                    return db.Reminders.Include("User")
                        .Where(i => i.User.id == user.id && i.DeleteStatus == false && i.IsReminded == false && System.Data.Entity.DbFunctions.TruncateTime(i.RemindDate) == today)
                        .ToList();
                }
            }
            catch
            {
                return new List<Reminder>();
            }
        }

        public bool PanelIsActive()
        {
            try
            {
                using (var db = new DB())
                {
                    return db.MessagePanels.Any();
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
