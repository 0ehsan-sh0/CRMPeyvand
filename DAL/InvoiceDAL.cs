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
    public class InvoiceDAL
    {
        DB db = new DB();

        public string Create(Invoice invoice, Customer customer, List<Product> products)
        {
            try
            {
                invoice.Customer = db.Customers.Find(customer.id);
                invoice.User = db.Users.Find(invoice.User.id);
                foreach (var item in products.ToList())
                {
                    invoice.Products.Add(db.Products.Find(item.id));
                    Product p = new Product();
                    p = db.Products.Find(item.id);
                    if (p.Type == "محصول")
                    {
                        p.Total -= item.Count;
                    }
                    
                }
                Random random = new Random();
                string numberrandom = random.Next(100000000).ToString();
                var q = db.Invoices.Where(x => x.InvoiceNumber == numberrandom);
                while (q.Count() > 0)
                {
                    numberrandom = random.Next(100000000).ToString();
                }
                invoice.InvoiceNumber = numberrandom;
                db.Invoices.Add(invoice);
                db.SaveChanges();
                return "ثبت فاکتور با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت فاکتور با مشکلی مواجه شد" + e.Message;
            }
        }


        public string Done(int id)
        {
            try
            {
                var q = db.Invoices.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.IsCheckedout = true;
                    q.CheckoutDate = DateTime.Now;
                    db.SaveChanges();
                    return "تسویه حساب انجام شد";
                }
                else
                {
                    return "ستون مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "تسویه حساب شخص  با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }


        public string ReadInvoiceNumIsReport()
        {
            var q = db.Invoices.OrderByDescending(i => i.id).FirstOrDefault();
            return q.InvoiceNumber;
        }
        public int ReadInvoiceLastID()
        {
            var q = db.Invoices.OrderByDescending(i => i.id).FirstOrDefault();
            return q.id;
        }


        public string CountInvoices()
        {
            return db.Invoices.Where(i => i.DeleteStatus == false).Count().ToString();
        }


        public DataTable Read()
        {
            string Query = "SELECT   TOP (1000)   InvoiceNumber AS [شماره فاکتور], IsCheckedout AS [وضعیت پرداخت], CheckoutDate AS [تاریخ پرداخت], TotalCount AS [تعداد کالاهای فاکتور], TotalPrice AS [هزینه پرداختی], OffCode AS [کد تخفیف], RegDate AS [تاریخ ثبت]\r\nFROM          dbo.Invoices\r\nWHERE      (DeleteStatus = 0) ORDER BY id DESC";
            string connectionStringText = @"Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true";
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var commandbuilder = new SqlCommandBuilder(sqlAdapter);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }
        public Invoice Read(string number)
        {
            var q = db.Invoices.Where(i => i.InvoiceNumber == number).FirstOrDefault();
            return q;
        }

        public string Delete(string number)
        {
            try
            {
                var q = db.Invoices.Where(i => i.InvoiceNumber == number).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "فاکتور مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }


        public Invoice ReadById(int id)
        {
            return db.Invoices.Find(id);
        }
        public DataTable Search(string Filter)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = "SearchInvoice";
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
    }
}
