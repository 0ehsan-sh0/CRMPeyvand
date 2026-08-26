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
    public class UserDAL
    {
        DB db = new DB();
        public string Create(User user)
        {

            try
            {
                user.UserGroup = db.UserGroups.Find(user.UserGroup.id);
                db.Users.Add(user);
                db.SaveChanges();
                return "ثبت اطلاعات با موفقیت انجام شد";

            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public bool Exsit(User user)
        {
            return db.Users.Any(i => i.UserName == user.UserName);
        }
        public User ReadById(int UserId)
        {
            return db.Users.Where(i => i.id == UserId).FirstOrDefault();
        }
        public string Update(User user, int UserId)
        {
            try
            {
                User user1 = ReadById(UserId);
                user1.UserGroup = db.UserGroups.Find(user.UserGroup.id);
                user1.Name = user.Name;
                user1.UserName = user.UserName;
                if (!string.IsNullOrWhiteSpace(user.Password))
                    user1.Password = user.Password;
                user1.Picture = user.Picture;
                db.SaveChanges();
                return "اطلاعات با موفقیت ویرایش شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public User ReadByUserName(string Name)
        {
            return db.Users.Include("UserGroup").Where(u => u.UserName == Name && u.DeleteStatus == false).SingleOrDefault();
        }
        public DataTable Read()
        {
            string Query = "SELECT    TOP (1000)  dbo.Users.Name AS نام, dbo.Users.UserName AS [نام کاربری], dbo.UserGroups.Title AS [گروه کاربری], dbo.Users.RegDate AS [تاریخ ثبت]\r\nFROM          dbo.Users INNER JOIN\r\n                      dbo.UserGroups ON dbo.Users.UserGroup_id = dbo.UserGroups.id\r\nWHERE      (dbo.Users.DeleteStatus = 0) and (dbo.UserGroups.IsBuiltIn = 0) ORDER BY dbo.Users.id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }
        public string Delete(int id)
        {
            try
            {
                User u = ReadById(id);
                u.DeleteStatus = true;
                db.SaveChanges();
                return "مشتری مورد نظر حذف شد";
            }
            catch (Exception e)
            {
                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public bool HasAnyUser()
        {
            return db.Users.Any();
        }
        public List<string> ReadUserNamesList()
        {
            return db.Users.Where(i => i.DeleteStatus == false).Select(i => i.UserName).ToList();
        }
        public User FindByUserName(string UserName)
        {
            return db.Users.Include("UserGroup").FirstOrDefault(i => i.UserName == UserName && !i.DeleteStatus);
        }
        public List<User> ReadInvoicesList()
        {
            return db.Users.Include("Invoices").Where(i => i.DeleteStatus == false).ToList();
        }
        public List<User> ReadActivitiesList()
        {
            return db.Users.Include("Activities").Where(i => i.DeleteStatus == false).ToList();
        }
    }
}
