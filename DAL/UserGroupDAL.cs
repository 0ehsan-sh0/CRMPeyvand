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
    public class UserGroupDAL
    {
        DB db = new DB();
        public string Create(UserGroup userGroup)
        {
            try
            {
                db.UserGroups.Add(userGroup);
                db.SaveChanges();
                return "ثبت گروه کاربری با موفقیت انجام شد";
            }
            catch (Exception e)
            {

                return "در ثبت گروه کاربری مشکلی به وجود آمد" + e.Message;
            }
        }
        public DataTable Read()
        {
            string Query = "SELECT   TOP (1000)  Title AS [نام گروه کاربری]\r\nFROM          dbo.UserGroups where (dbo.UserGroups.Title <> N'مدیریت') ORDER BY id DESC";
            string connectionStringText = DB.ConnectionString;
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var commandbuilder = new SqlCommandBuilder(sqlAdapter);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }
        public string Update(string title, List<UserAccessRole> uar)
        {
            try
            {
                UserGroup ug = db.UserGroups.Include("UserAccessRoles").Where(g => g.Title == title).FirstOrDefault();
                foreach (var item in uar.ToList())
                {
                    UserAccessRole q = db.UserAccessRoles.Find(item.id);
                    q.CanEnter = item.CanEnter;
                    q.CanDelete = item.CanDelete;
                    q.CanUpdate = item.CanUpdate;
                    q.CanCreate = item.CanCreate;
                    ug.UserAccessRoles.Add(q);
                }
                db.SaveChanges();
                return "ویرایش با موفیقت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }

        public bool Exist(string Title)
        {
            return db.UserGroups.Any(x => x.Title == Title);
        }
        public List<string> ReadTitles()
        {
            return db.UserGroups.Where(i => i.Title != "مدیریت").Select(x => x.Title).ToList();
        }
        public bool ReadByTitle(string Title)
        {
            return !db.UserGroups.Any(x => x.Title == Title);
        }

        public UserGroup ReadBySingelTitle(string title)
        {
            return db.UserGroups.Include("UserAccessRoles").Where(x => x.Title == title).SingleOrDefault();
        }
        public UserGroup AdminUserGruop()
        {
            var q = db.UserGroups.FirstOrDefault();
            return q;
        }
    }
}
