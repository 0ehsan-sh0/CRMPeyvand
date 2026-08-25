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
    public class OffCodeDAL
    {
        DB db = new DB();
        public bool Exist(string code)
        {
            return db.OffCodes.Any(x => x.Code == code);
        }
        public string Create(OffCode o)
        {
            try
            {
                db.OffCodes.Add(o);
                db.SaveChanges();
                return "ثبت اطلاعات کد تخفیف با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }

        }
        public OffCode GetOffCode(string code)
        {
            return db.OffCodes.Where(i => i.Code == code).FirstOrDefault();
        }
        public DataTable Read()
        {
            string Query = "SELECT  TOP (1000)   Code AS [کد تخفیف], Price AS [مبلغ تخفیف], [Percent] AS [درصد تخفیف], LimitCount AS [محدودیت مصرف], ExpireDate AS [تاریخ انقضا], RegDate AS [تاریخ ثبت] \r\nFROM          dbo.OffCodes\r\nWHERE      (DeleteStatus = 0)\r\nORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
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
            command.CommandText = "SearchOffCode";
            string connectionStringText = DB.ConnectionString;
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
        public string OffCodeCount()
        {
            return db.OffCodes.Where(i => i.DeleteStatus == false).Count().ToString();
        }
        public string Delete(string code)
        {
            try
            {
                var q = db.OffCodes.Where(i => i.Code == code).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "کد نخفیف مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public string Update(OffCode c, string code)
        {
            try
            {
                OffCode offCode = GetOffCode(code);
                offCode.IsPrice = c.IsPrice;
                if (c.IsPrice)
                {
                    offCode.Price = c.Price;
                    offCode.Percent = null;
                }
                else
                {
                    offCode.Percent = c.Percent;
                    offCode.Price = null;
                }
                offCode.ExpireDate = c.ExpireDate;
                offCode.LimitCount = c.LimitCount;
                db.SaveChanges();
                return "ویرایش با موفیقت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public string CanUse(string code, Customer c)
        {
            OffCode offCode = GetOffCode(code);
            bool IsExpired = false;
            bool IsLimited = false;
            if (offCode != null)
            {
                if (offCode.ExpireDate != null)
                {
                    if (offCode.ExpireDate >= DateTime.Today.Date)
                    {
                        IsExpired = false;
                    }
                    else IsExpired = true;
                }
                if (offCode.LimitCount != null)
                {
                    int a = db.Invoices.Include("Customer").Where(i => i.Customer.Phone == c.Phone && i.OffCode == offCode.Code).Count();
                    if (a < offCode.LimitCount)
                    {
                        IsLimited = false;
                    }
                    else
                    {
                        IsLimited = true;
                    }
                }

                if (IsExpired)
                {
                    return "کد تخفیف منقضی شده است";
                }
                else if(IsLimited)
                {
                    return "مشتری کد تخفیف را مصرف کرده است";
                }
                else
                {
                    if (offCode.IsPrice)
                    {
                        return offCode.Price.ToString();
                    }
                    else return offCode.Percent.ToString();
                }
            }
            else
            {
                return "کد تخفیف مورد نظر یافت نشد";
            }
        }
    }
}
