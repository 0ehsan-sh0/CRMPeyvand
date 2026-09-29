using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class OffCodeDAL
    {
        DB db;

        public OffCodeDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public OffCodeDAL(DB db)
        {
            this.db = db;
        }

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
        // Copied verbatim from the SQL this replaces, including the order: the
        // grid binds with AutoGenerateColumns, so these are the headers.
        private static readonly string[] ReadColumns =
            { "کد تخفیف", "مبلغ تخفیف", "درصد تخفیف", "محدودیت مصرف", "تاریخ انقضا", "تاریخ ثبت" };

        /// <summary>
        /// The rows behind the grid, newest first. Materialised before the
        /// projection because EF6 cannot translate a projection into
        /// object[] into SQL.
        /// </summary>
        private List<object[]> OffCodeRows()
        {
            return db.OffCodes
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[]
                {
                    i.Code,
                    i.Price,
                    i.Percent,
                    i.LimitCount,
                    i.ExpireDate,
                    i.RegDate,
                })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, OffCodeRows());
        }

        public DataTable Search(string Filter)
        {
            // The old LIKE clause covered Code only. The amount and the
            // percentage are shown in the grid but were never searchable, so
            // they stay that way.
            var rows = OffCodeRows()
                .Where(r => GridTable.Matches(Filter, (string)r[0]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
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
