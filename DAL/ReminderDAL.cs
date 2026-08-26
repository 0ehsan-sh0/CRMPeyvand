using BE;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ReminderDAL
    {
        DB db = new DB();
        public string Create(Reminder r, User u)
        {
            try
            {
                r.User = db.Users.Find(u.id);
                db.Reminders.Add(r);
                db.SaveChanges();
                return "ثبت اطلاعات ب موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public Reminder ReadByID(int id)
        {
            var q = db.Reminders.Include("User").Where(i => i.id == id).FirstOrDefault();
            return q;
        }
        public DataTable Read()
        {
            string Query = "SELECT    TOP (1000)  dbo.Reminders.id AS ردیف, dbo.Reminders.Title AS موضوع, dbo.Reminders.Info AS توضیحات, dbo.Reminders.RemindDate AS [تاریخ یادآوری], dbo.Reminders.IsReminded AS [وضعیت یادآور], dbo.Users.Name AS [نام کاربری], \r\n                      dbo.Reminders.RegDate\r\nFROM          dbo.Reminders INNER JOIN\r\n                      dbo.Users ON dbo.Reminders.User_id = dbo.Users.id\r\nWHERE      (dbo.Reminders.DeleteStatus = 0) ORDER BY dbo.Reminders.id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }

        public string Update(Reminder r, int id)
        {
            try
            {

                Reminder reminder = ReadByID(id);
                reminder.RemindDate = r.RemindDate;
                reminder.Info = r.Info;
                reminder.Title = r.Title;
                reminder.User = db.Users.Find(r.User.id);
                db.SaveChanges();
                return "ویرایش با موفیقت انجام شد";
            }
            catch (Exception e)
            {
                return "ویرایش اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public string Delete(int id)
        {
            try
            {
                var q = db.Reminders.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "یادآور مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public string Done(int id)
        {
            try
            {
                var q = db.Reminders.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.IsReminded = true;
                    db.SaveChanges();
                    return "انجام شدن فعالیت ثبت شد";
                }
                else
                {
                    return "یادآور مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }

        public DataTable Search(string Filter)
        {
            SqlCommand command = new SqlCommand("SELECT    TOP (1000)  dbo.Reminders.id AS ردیف, dbo.Reminders.Title AS موضوع, dbo.Reminders.Info AS توضیحات, dbo.Reminders.RemindDate AS [تاریخ یادآوری], dbo.Reminders.IsReminded AS [وضعیت یادآور], dbo.Users.Name AS [نام کاربری], \r\n                      dbo.Reminders.RegDate\r\nFROM          dbo.Reminders INNER JOIN\r\n                      dbo.Users ON dbo.Reminders.User_id = dbo.Users.id\r\nWHERE      (dbo.Reminders.DeleteStatus = 0) AND ((dbo.Reminders.Title LIKE N'%' + @Search + N'%') OR (dbo.Reminders.Info LIKE N'%' + @Search + N'%')) ORDER BY dbo.Reminders.id DESC");
            string connectionStringText = DB.ConnectionString;
            command.Parameters.AddWithValue("@Search", Filter);
            SqlConnection connection = new SqlConnection(connectionStringText);
            command.Connection = connection;
            var sqldataadpter = new SqlDataAdapter();
            sqldataadpter.SelectCommand = command;
            var dataset = new DataSet();
            sqldataadpter.Fill(dataset);
            return dataset.Tables[0];
        }
        public string Count()
        {
            return db.Reminders.Where(i => i.DeleteStatus == false).Count().ToString();
        }

    }
}
