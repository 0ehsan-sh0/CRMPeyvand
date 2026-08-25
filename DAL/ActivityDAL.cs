using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ActivityDAL
    {
        DB db = new DB();
        public string Create(Activity activity)
        {
            try
            {
                activity.User = db.Users.Find(activity.User.id);
                activity.Customer = db.Customers.Find(activity.Customer.id);
                activity.ActivityCategory = db.ActivityCategories.Find(activity.ActivityCategory.id);
                db.Activities.Add(activity);
                db.SaveChanges();
                return "ثبت اطلاعات با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی مواجه شد" + e.Message;
            }
        }
        public DataTable Read()
        {
            string Query = "SELECT TOP (1000)  dbo.Activities.id AS ردیف ,  dbo.Activities.Title AS عنوان, dbo.Activities.Info AS توضبحات, dbo.ActivityCategories.CategoryName AS [دسته بندی], dbo.Users.UserName AS [نام کاربر], dbo.Activities.RegDate AS [تاریخ ثبت]\r\nFROM          dbo.Activities INNER JOIN\r\n                      dbo.ActivityCategories ON dbo.Activities.ActivityCategory_id = dbo.ActivityCategories.id INNER JOIN\r\n                      dbo.Users ON dbo.Activities.User_id = dbo.Users.id\r\nWHERE      (dbo.Activities.DeleteStatus = 0) ORDER BY dbo.Activities.id DESC";
            string connectionStringText = @"Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true";
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var commandbuilder = new SqlCommandBuilder(sqlAdapter);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }

        public Activity ReadById(int id)
        {
            return db.Activities.Include("User").Include("Customer").Include("ActivityCategory").Where(i => i.id == id).FirstOrDefault();
        }
        public string Update(Activity a, int id)
        {
            try
            {
                var q = db.Activities.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.Title = a.Title;
                    q.Info = a.Info;
                    q.ActivityCategory = db.ActivityCategories.Find(a.ActivityCategory.id);
                    q.User = db.Users.Find(a.User.id);
                    q.Customer = db.Customers.Find(a.Customer.id);

                    db.SaveChanges();
                    return "ویرایش با موفیقت انجام شد";
                }
                else
                {
                    return "فعالیت مورد نظر یافت نشد";
                }
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
                var q = db.Activities.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "فعالیت مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public DataTable Search(string Filter)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = "SearchActivity";
            string connectionStringText = @"Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true";
            SqlConnection connection = new SqlConnection(connectionStringText);
            command.Parameters.AddWithValue("@Search", Filter);
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            var sqldataadpter = new SqlDataAdapter();
            sqldataadpter.SelectCommand = command;
            var dataset = new DataSet();
            sqldataadpter.Fill(dataset);
            return dataset.Tables[0];
        }


        public string ReadInfo(int id)
        {
            return db.Activities.FirstOrDefault(i => i.id == id).Info;
        }
        public string Count()
        {
            return db.Activities.Where(i => i.DeleteStatus == false).Count().ToString();
        }

    }
}
