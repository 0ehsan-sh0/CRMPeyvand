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
    public class CustomerDAL
    {
        DB db = new DB();
        public bool Exist(Customer c)
        {
            return db.Customers.Any(i => i.Phone == c.Phone);
        }
        public Customer GetCustomer(string phone)
        {
            var q = db.Customers.Where(i => i.Phone == phone).FirstOrDefault();
            return q;
        }
        public string CustomerCount()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).Count().ToString();
        }
        public string Create(Customer c)
        {
            try
            {
                db.Customers.Add(c);
                db.SaveChanges();
                return "ثبت اطلاعات مشتری با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public DataTable Read()
        {
            string Query = "SELECT  TOP (1000)    Name AS [نام], Phone AS [شماره تماس], RegDate AS [تاریخ ثبت]\r\nFROM          dbo.Customers\r\nWHERE      (DeleteStatus = 0) ORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var commandbuilder = new SqlCommandBuilder(sqlAdapter);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }
        public Customer ReadById(int id)
        {
            return db.Customers.Find(id);
        }


        public string Update(Customer c, int id)
        {
            try
            {
                Customer customer = ReadById(id);
                customer.Name = c.Name;
                customer.Phone = c.Phone;
                db.SaveChanges();
                return "ویرایش با موفیقت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public string Delete(int id)
        {
            try
            {
                var q = db.Customers.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "مشتری مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public List<string> ReadPhoneNumbers()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).Select(i => i.Phone).ToList();
        }
        public DataTable Search(string Filter)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = "SearchCustomers";
            string connectionStringText = DB.ConnectionString;
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
        public List<Customer> ReadWithDateTime()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).ToList();
        }
    }
}
