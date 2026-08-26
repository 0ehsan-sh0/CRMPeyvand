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
        DB db = new DB();
        public string CustomersCount()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).Count().ToString();
        }


        public string SellsCountToday()
        {
            int sum = 0;
            foreach (var item in db.Invoices.Where(i => i.DeleteStatus == false))
            {
                if (item.RegDate.Date == DateTime.Today)
                {
                    sum = sum + 1;
                }
            }
            return sum.ToString();
        }
        public string SellsCountWeek()
        {
            string queryString = "SELECT COUNT(*) \r\nFROM Invoices \r\nWHERE (DeleteStatus = 0) AND Invoices.RegDate BETWEEN DATEADD(WEEK, -1, GETDATE()) AND GETDATE()";
            string connectionString = DB.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                int countResult = (int)command.ExecuteScalar();
                connection.Close();
                return countResult.ToString();
            }

        }
        public string UserReminderCount(User u)
        {
            User q = db.Users.Include("Reminders").Where(i => i.id == u.id).FirstOrDefault();
            int a = 0;
            foreach (var item in q.Reminders)
            {
                if (item.RemindDate.Date == DateTime.Today && item.IsReminded == false && item.DeleteStatus == false)
                {
                    a++;
                }
            }
            return a.ToString();
        }
        public List<Reminder> GetUserReminder(User user)
        {
            List<Reminder> reminders = new List<Reminder>();
            foreach (var item in db.Reminders.Include("User").Where(i => i.User.id == user.id).ToList())
            {
                if (item.RemindDate.Date == DateTime.Today && item.IsReminded == false && item.DeleteStatus == false)
                {
                   reminders.Add(item);
                }
            }
            return reminders;
        }
        public bool PanelIsActive()
        {
            return db.MessagePanels.Count() > 0;
        }
    }
}
