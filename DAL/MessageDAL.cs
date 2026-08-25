using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class MessageDAL
    {
        DB db = new DB();
        public string Create(Message m)
        {
            try
            {
                db.Messages.Add(m);
                db.SaveChanges();
                return "ثبت اطلاعات با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }

        }
        public bool Exist(string content)
        {
            return db.Messages.Any(i => i.Content == content);
        }
        public DataTable Read()
        {
            string Query = "SELECT   TOP (1000)  [Content] AS [متن پیام]\r\nFROM          dbo.Messages\r\nWHERE      (DeleteStatus = 0)\r\nORDER BY id DESC";
            string connectionStringText = @"Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true";
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var commandbuilder = new SqlCommandBuilder(sqlAdapter);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }
        public DataTable Search(string Filter)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = "SearchMessage";
            string connectionStringText = @"Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true";
            SqlConnection connection = new SqlConnection(connectionStringText);
            command.Parameters.AddWithValue("@search", Filter);
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            var sqldataadpter = new SqlDataAdapter();
            sqldataadpter.SelectCommand = command;
            var dataset = new DataSet();
            sqldataadpter.Fill(dataset);
            return dataset.Tables[0];
        }
        public string Count()
        {
            return db.Messages.Count().ToString();
        }
        public List<string> First10()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.id).Select(i => i.Phone).Take(10).ToList();
        }
        public List<string> First100()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.id).Select(i => i.Phone).Take(100).ToList();
        }
        public List<string> First1000()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.id).Select(i => i.Phone).Take(1000).ToList();
        }
        public List<string> First10000()
        {
            return db.Customers.Where(i => i.DeleteStatus == false).OrderByDescending(i => i.id).Select(i => i.Phone).Take(10000).ToList();
        }
        public List<string> FirstBuy10()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Invoices.Count).Select(i => i.Phone).Take(10).ToList();
        }
        public List<string> FirstBuy100()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Invoices.Count).Select(i => i.Phone).Take(100).ToList();
        }

        public List<string> FirstBuy1000()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Invoices.Count).Select(i => i.Phone).Take(1000).ToList();
        }
        public List<string> FirstBuy10000()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false).OrderByDescending(i => i.Invoices.Count).Select(i => i.Phone).Take(10000).ToList();
        }
        public List<string> FirstBuy()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false && i.Invoices.Count == 1).Select(i => i.Phone).ToList();
        }
        public List<string> NoBuy()
        {
            return db.Customers.Include("Invoices").Where(i => i.DeleteStatus == false && i.Invoices.Count == 0).Select(i => i.Phone).ToList();
        }
        public List<string> IsNotCheckedOut()
        {
            List<string> list = new List<string>();
            foreach (var item in db.Customers.Include("Invoices"))
            {
                bool IsNotCheckedOut = false;
                foreach (var Iitem in item.Invoices)
                {
                    if (Iitem.IsCheckedout == false)
                    {
                        IsNotCheckedOut = true;
                    }
                }
                if (IsNotCheckedOut)
                {
                    list.Add(item.Phone);
                }
            }
            return list;
        }
    }
}
