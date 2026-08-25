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
    public class CatalogItemDAL
    {
        DB db = new DB();
        public string Create(CatalogItem p)
        {
            try
            {
                db.CatalogItems.Add(p);
                db.SaveChanges();
                return "ثبت اطلاعات کالا با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;

            }
        }


        public DataTable Read()
        {
            string Query = "SELECT   TOP (1000)   Name AS نام, SalePrice AS قیمت, (CASE Kind WHEN 1 THEN N'محصول' WHEN 2 THEN N'خدمات' END) AS نوع, Stock AS موجودی\r\nFROM          dbo.CatalogItems\r\nWHERE      (DeleteStatus = 0) ORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }
        public DataTable Read(string type)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT   TOP (1000)   Name AS نام, SalePrice AS قیمت, (CASE Kind WHEN 1 THEN N'محصول' WHEN 2 THEN N'خدمات' END) AS نوع, Stock AS موجودی\r\nFROM          dbo.CatalogItems\r\nWHERE      (DeleteStatus = 0) AND ((CASE Kind WHEN 1 THEN N'محصول' WHEN 2 THEN N'خدمات' END) = @Search)\r\nORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            command.Parameters.AddWithValue("@Search", type);
            command.Connection = connection;
            var sqldataadpter = new SqlDataAdapter();
            sqldataadpter.SelectCommand = command;
            var dataset = new DataSet();
            sqldataadpter.Fill(dataset);
            return dataset.Tables[0];
        }
        public List<string> ReadNames()
        {
            return db.CatalogItems.Where(i => i.DeleteStatus == false).Select(i => i.Name).ToList();
        }

        public bool Exist(CatalogItem p)
        {
            return db.CatalogItems.Any(i => i.Name == p.Name);
        }


        public CatalogItem ReadById(int id)
        {
            return db.CatalogItems.Find(id);
        }


        public string Update(CatalogItem p, int id)
        {
            try
            {
                CatalogItem product = ReadById(id);
                if (!product.DeleteStatus)
                {
                    product.Name = p.Name;
                    product.SalePrice = p.SalePrice;
                    product.Kind = p.Kind;
                    product.Stock = p.Stock;
                    db.SaveChanges();
                    return "ویرایش با موفیقت انجام شد";
                }
                else return "کالا یافت نشد";
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
                var q = db.CatalogItems.Where(i => i.DeleteStatus == false && i.Id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "کالا مورد نظر یافت نشد";
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
            command.CommandText = "SELECT   TOP (1000)   Name AS نام, SalePrice AS قیمت, (CASE Kind WHEN 1 THEN N'محصول' WHEN 2 THEN N'خدمات' END) AS نوع, Stock AS موجودی\r\nFROM          dbo.CatalogItems\r\nWHERE      (DeleteStatus = 0) AND (Name LIKE N'%' + @Search + N'%')\r\nORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            command.Parameters.AddWithValue("@Search", Filter);
            command.Connection = connection;
            var sqldataadpter = new SqlDataAdapter();
            sqldataadpter.SelectCommand = command;
            var dataset = new DataSet();
            sqldataadpter.Fill(dataset);
            return dataset.Tables[0];
        }


        public CatalogItem ReadByName(string product)
        {
            return db.CatalogItems.Where(i => i.DeleteStatus == false && i.Name == product).SingleOrDefault();
        }


        public string ProductsCount()
        {
            return db.CatalogItems.Where(i => i.DeleteStatus == false).Count().ToString();
        }

    }
}
