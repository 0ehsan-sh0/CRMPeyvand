using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class UserDAL
    {
        DB db;

        public UserDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public UserDAL(DB db)
        {
            this.db = db;
        }

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
        // Copied verbatim from the SQL this replaces. The fourth column is
        // Users.RegDate and the third is the joined UserGroups.Title.
        private static readonly string[] ReadColumns =
            { "نام", "نام کاربری", "گروه کاربری", "تاریخ ثبت" };

        /// <summary>
        /// The rows behind the grid, newest first. The group title is reached
        /// through Include rather than the join the old SQL spelled out: EF6 does
        /// not lazy load, so touching UserGroup without it comes back null.
        /// </summary>
        private List<object[]> UserRows()
        {
            return db.Users
                .Include("UserGroup")
                .Where(i => i.DeleteStatus == false)
                // The built-in administrator group is not a manageable group and
                // was excluded in the original query. A user with no group is
                // kept, matching the null-guard in the projection below.
                .Where(i => i.UserGroup == null || i.UserGroup.IsBuiltIn == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[]
                {
                    i.Name,
                    i.UserName,
                    i.UserGroup == null ? null : i.UserGroup.Title,
                    i.RegDate,
                })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, UserRows());
        }

        public DataTable Search(string Filter)
        {
            var rows = UserRows()
                .Where(r => GridTable.Matches(Filter,
                    (string)r[0], (string)r[1], (string)r[2]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
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
