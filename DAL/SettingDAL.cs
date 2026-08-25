using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using System.IO;

namespace DAL
{
    public class SettingDAL
    {
        DB db = new DB();
        public string BackUp(string Path)
        {
            try
            {
                SqlCommand command = new SqlCommand();
                string connectionStringText = DB.ConnectionString;
                SqlConnection connection = new SqlConnection(connectionStringText);
                // BACKUP DATABASE cannot be parameterized for the db name; it is taken from
                // the live connection (not user input). The file path stays a parameter.
                command.CommandText = "BACKUP DATABASE [" + connection.Database + "] TO DISK = @path WITH INIT";
                command.Parameters.AddWithValue("@path", Path);
                command.Connection = connection;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return "ذخیره فایل با موفقیت انجام شد لطفا پوشه مورد نظر را بررسی کنید";
            }
            catch (Exception e)
            {
                return "ذخیره پشتیبان با مشکلی مواجه شد:\n" + e.Message;
            }
        }
        public string Recovery()
        {
            try
            {
                var q = db.Activities.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q)
                {
                    item.DeleteStatus = false;
                }
                var q1 = db.ActivityCategories.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q1)
                {
                    item.DeleteStatus = false;
                }
                var q2 = db.Customers.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q2)
                {
                    item.DeleteStatus = false;
                }
                var q3 = db.Invoices.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q3)
                {
                    item.DeleteStatus = false;
                }
                var q4 = db.Messages.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q4)
                {
                    item.DeleteStatus = false;
                }
                var q5 = db.OffCodes.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q5)
                {
                    item.DeleteStatus = false;
                }
                var q6 = db.CatalogItems.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q6)
                {
                    item.DeleteStatus = false;
                }
                var q7 = db.Reminders.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q7)
                {
                    item.DeleteStatus = false;
                }
                var q8 = db.Users.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q8)
                {
                    item.DeleteStatus = false;
                }
                db.SaveChanges();
                return "اطلاعات با موفقیت بازگردانی شد";
            }
            catch (Exception e)
            {
                return "بازگردانی اطلاعات حذف شده با مشکلی روبرو شد:\n" + e.Message;
            }
        }
    }
}
