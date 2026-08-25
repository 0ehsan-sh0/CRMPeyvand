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
    public class ProductDAL
    {
        DB db = new DB();
        public string Create(Product p)
        {
            try
            {
                db.Products.Add(p);
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
            string Query = "SELECT   TOP (1000)   Name AS نام, Price AS قیمت, Type AS نوع, Total AS موجودی\r\nFROM          dbo.Products\r\nWHERE      (DeleteStatus = 0) ORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }
        public DataTable Read(string type)
        {
            SqlCommand command = new SqlCommand("ReadByType");
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            command.Parameters.AddWithValue("@Search", type);
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            var sqldataadpter = new SqlDataAdapter();
            sqldataadpter.SelectCommand = command;
            var dataset = new DataSet();
            sqldataadpter.Fill(dataset);
            return dataset.Tables[0];
        }
        public List<string> ReadNames()
        {
            return db.Products.Where(i => i.DeleteStatus == false).Select(i => i.Name).ToList();
        }

        public bool Exist(Product p)
        {
            return db.Products.Any(i => i.Name == p.Name);
        }


        public Product ReadById(int id)
        {
            return db.Products.Find(id);
        }


        public string Update(Product p, int id)
        {
            try
            {
                Product product = ReadById(id);
                if (!product.DeleteStatus)
                {
                    product.Name = p.Name;
                    product.Price = p.Price;
                    product.Type = p.Type;
                    product.Total = p.Total;
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
                var q = db.Products.Where(i => i.DeleteStatus == false && i.id == id).FirstOrDefault();
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
            SqlCommand command = new SqlCommand("SearchProduct");
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



        public List<Product> ReadByTotal()
        {
            return db.Products.Where(i => i.DeleteStatus == false && i.Total != 0).ToList();
        }



        public Product ReadByName(string product)
        {
            return db.Products.Where(i => i.DeleteStatus == false && i.Name == product).SingleOrDefault();
        }


        public string ProductsCount()
        {
            return db.Products.Where(i => i.DeleteStatus == false).Count().ToString();
        }

    }
}
