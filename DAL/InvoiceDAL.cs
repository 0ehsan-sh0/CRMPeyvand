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

        // Creates invoice + lines atomically, validates/decrements Goods stock, returns saved invoice with id.
        // validateStock / computeDiscount are injected from BLL (StockPolicy.Validate / Pricing.ComputeDiscount)
        // because the DAL assembly cannot reference BLL (BLL references DAL).
        public Invoice Create(Invoice invoice, int customerId, IReadOnlyList<InvoiceLine> lines,
            Func<CatalogItem, int, InvalidOperationException> validateStock,
            Func<OffCode, decimal, decimal> computeDiscount)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    invoice.Customer = db.Customers.Find(customerId);
                    invoice.User = db.Users.Find(invoice.User.id);
                    foreach (var line in lines)
                    {
                        var item = db.CatalogItems.Find(line.CatalogItemId);
                        var rejection = validateStock(item, line.Quantity);
                        if (rejection != null) throw rejection;
                        if (item.Kind == ItemKind.Good) item.Stock -= line.Quantity;
                        line.UnitPrice = item.SalePrice;
                        line.CatalogItem = item;
                        invoice.Lines.Add(line);
                    }
                    invoice.DiscountAmount = computeDiscount(
                        string.IsNullOrEmpty(invoice.OffCode)
                            ? null
                            : db.OffCodes.FirstOrDefault(o => o.Code == invoice.OffCode),
                        invoice.SubTotal);
                    db.Invoices.Add(invoice);
                    db.SaveChanges();
                    transaction.Commit();
                    return invoice;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
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
            string Query = "SELECT   TOP (1000)   id AS [شماره فاکتور], IsCheckedout AS [وضعیت پرداخت], CheckoutDate AS [تاریخ پرداخت], OffCode AS [کد تخفیف], RegDate AS [تاریخ ثبت]\r\nFROM          dbo.Invoices\r\nWHERE      (DeleteStatus = 0) ORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var commandbuilder = new SqlCommandBuilder(sqlAdapter);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }

        public string Delete(int id)
        {
            try
            {
                var q = db.Invoices.Find(id);
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
            command.CommandText = "SELECT   TOP (1000)   id AS [شماره فاکتور], IsCheckedout AS [وضعیت پرداخت], CheckoutDate AS [تاریخ پرداخت], OffCode AS [کد تخفیف], RegDate AS [تاریخ ثبت]\r\nFROM          dbo.Invoices\r\nWHERE      (DeleteStatus = 0) AND (CONVERT(nvarchar(max), id) LIKE N'%' + @search + N'%')\r\nORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            command.Parameters.AddWithValue("@search", Filter);
            command.Connection = connection;
            var sqldataadpter = new SqlDataAdapter();
            sqldataadpter.SelectCommand = command;
            var dataset = new DataSet();
            sqldataadpter.Fill(dataset);
            return dataset.Tables[0];
        }
    }
}
