using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ActivityDAL
    {
        DB db;

        public ActivityDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public ActivityDAL(DB db)
        {
            this.db = db;
        }

        public string Create(Activity activity)
        {
            try
            {
                activity.User = db.Users.Find(activity.User.id);
                activity.Customer = db.Customers.Find(activity.Customer.id);
                activity.ActivityCategory = db.ActivityCategories.Find(activity.ActivityCategory.id);
                db.Activities.Add(activity);
                db.SaveChanges();
                return "ثبت اطلاعات با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی مواجه شد" + e.Message;
            }
        }
        // Copied verbatim from the SQL this replaces. Note "توضبحات" is spelled
        // without a ی, and the fifth column is Users.UserName rather than
        // Users.Name. Both look like mistakes and both are what employees see
        // today, so they are preserved rather than corrected here.
        private static readonly string[] ReadColumns =
            { "ردیف", "عنوان", "توضبحات", "دسته بندی", "نام کاربر", "تاریخ ثبت" };

        /// <summary>
        /// The rows behind the grid, newest first. The category and the user are
        /// reached through Include rather than the joins the old SQL spelled
        /// out: EF6 does not lazy load, so touching ActivityCategory or User
        /// without it comes back null.
        /// </summary>
        private List<object[]> ActivityRows()
        {
            return db.Activities
                .Include("ActivityCategory")
                .Include("User")
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[]
                {
                    i.id,
                    i.Title,
                    i.Info,
                    i.ActivityCategory == null ? null : i.ActivityCategory.CategoryName,
                    i.User == null ? null : i.User.UserName,
                    i.RegDate,
                })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, ActivityRows());
        }

        public List<Activity> ReadAllWithDetails()
        {
            return db.Activities
                .Include("User")
                .Include("Customer")
                .Include("ActivityCategory")
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .ToList();
        }

        public Activity ReadById(int id)
        {
            return db.Activities.Include("User").Include("Customer").Include("ActivityCategory").Where(i => i.id == id).FirstOrDefault();
        }
        public string Update(Activity a, int id)
        {
            try
            {
                var q = db.Activities.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.Title = a.Title;
                    q.Info = a.Info;
                    q.ActivityCategory = db.ActivityCategories.Find(a.ActivityCategory.id);
                    q.User = db.Users.Find(a.User.id);
                    q.Customer = db.Customers.Find(a.Customer.id);

                    db.SaveChanges();
                    return "ویرایش با موفیقت انجام شد";
                }
                else
                {
                    return "فعالیت مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "ویرایش اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }


        public string Delete(int id)
        {
            try
            {
                var q = db.Activities.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "فعالیت مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public DataTable Search(string Filter)
        {
            // The old query matched Title, Info, CategoryName and UserName -
            // not the customer, because the customer is not in this grid.
            var rows = ActivityRows()
                .Where(r => GridTable.Matches(Filter,
                    (string)r[1], (string)r[2], (string)r[3], (string)r[4]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }


        public string ReadInfo(int id)
        {
            return db.Activities.FirstOrDefault(i => i.id == id).Info;
        }
        public string Count()
        {
            return db.Activities.Where(i => i.DeleteStatus == false).Count().ToString();
        }

    }
}
