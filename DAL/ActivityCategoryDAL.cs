using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class ActivityCategoryDAL
    {
        DB db;

        public ActivityCategoryDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public ActivityCategoryDAL(DB db)
        {
            this.db = db;
        }

        public string Create(ActivityCategory a)
        {
            try
            {
                db.ActivityCategories.Add(a);
                db.SaveChanges();
                return "دسته بندی با موفقیت ثبت شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;

            }
        }

        public bool Exist(ActivityCategory a)
        {
            return db.ActivityCategories.Any(i => i.CategoryName == a.CategoryName);
        }

        private static readonly string[] ReadColumns =
            { "ردیف", "نام دسته بندی" };

        public DataTable Read()
        {
            var rows = db.ActivityCategories
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(100)
                .ToList()
                .Select(i => new object[] { i.id, i.CategoryName })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }

        public DataTable Search(string Filter)
        {
            var rows = db.ActivityCategories
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(100)
                .AsEnumerable()
                .Where(i => GridTable.Matches(Filter, i.CategoryName))
                .Select(i => new object[] { i.id, i.CategoryName })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }



        public ActivityCategory ReadById(int id)
        {
            return db.ActivityCategories.Find(id);
        }



        public string Delete(int id)
        {
            try
            {
                var q = db.ActivityCategories.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "دسته بندی مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }



        public string Update(ActivityCategory activity, int id)
        {
            try
            {
                var q = db.ActivityCategories.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.CategoryName = activity.CategoryName;
                    db.SaveChanges();
                    return "ویرایش با موفیقت انجام شد";
                }
                else
                {
                    return "دسته بندی مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "ویرایش اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }



        public List<string> ActivityCategoryReadNames()
        {
            return db.ActivityCategories.Where(i => i.DeleteStatus == false).Select(i => i.CategoryName).ToList();
        }


        public ActivityCategory ReadByName(string Name)
        {
            return db.ActivityCategories.Where(u => u.CategoryName == Name).SingleOrDefault();
        }

    }
}
