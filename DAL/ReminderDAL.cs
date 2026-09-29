using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ReminderDAL
    {
        DB db;

        public ReminderDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public ReminderDAL(DB db)
        {
            this.db = db;
        }

        public string Create(Reminder r, User u)
        {
            try
            {
                r.User = db.Users.Find(u.id);
                db.Reminders.Add(r);
                db.SaveChanges();
                return "ثبت اطلاعات ب موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public Reminder ReadByID(int id)
        {
            var q = db.Reminders.Include("User").Where(i => i.id == id).FirstOrDefault();
            return q;
        }
        // Copied verbatim from the SQL this replaces. The last column has no
        // alias in the original, so the grid header is the literal string
        // "RegDate". That is ugly but it is what employees see today, so it is
        // preserved; renaming it is a separate, deliberate change.
        private static readonly string[] ReadColumns =
            { "ردیف", "موضوع", "توضیحات", "تاریخ یادآوری", "وضعیت یادآور", "نام کاربری", "RegDate" };

        /// <summary>
        /// The rows behind the grid, newest first. The owner's name is reached
        /// through Include rather than the join the old SQL spelled out: EF6 does
        /// not lazy load, so touching User without it comes back null.
        /// </summary>
        private List<object[]> ReminderRows()
        {
            return db.Reminders
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
                    i.RemindDate,
                    i.IsReminded,
                    // The header says "نام کاربری" but the old query took
                    // dbo.Users.Name, not UserName. That is what employees see
                    // today, so it stays.
                    i.User == null ? null : i.User.Name,
                    i.RegDate,
                })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, ReminderRows());
        }

        public string Update(Reminder r, int id)
        {
            try
            {

                Reminder reminder = ReadByID(id);
                reminder.RemindDate = r.RemindDate;
                reminder.Info = r.Info;
                reminder.Title = r.Title;
                reminder.User = db.Users.Find(r.User.id);
                db.SaveChanges();
                return "ویرایش با موفیقت انجام شد";
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
                var q = db.Reminders.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "یادآور مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public string Done(int id)
        {
            try
            {
                var q = db.Reminders.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.IsReminded = true;
                    db.SaveChanges();
                    return "انجام شدن فعالیت ثبت شد";
                }
                else
                {
                    return "یادآور مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }

        public DataTable Search(string Filter)
        {
            // The old query matched Title and Info only - not the user name.
            var rows = ReminderRows()
                .Where(r => GridTable.Matches(Filter, (string)r[1], (string)r[2]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
        public string Count()
        {
            return db.Reminders.Where(i => i.DeleteStatus == false).Count().ToString();
        }

    }
}
