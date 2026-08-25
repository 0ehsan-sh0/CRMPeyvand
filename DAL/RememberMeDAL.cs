using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class RememberMeDAL
    {
        DB db = new DB();
        public bool Exist()
        {
            return db.RememberMe.Count() > 0;
        }
        public string Create(RememberMe rm)
        {
            try
            {
                db.RememberMe.Add(rm);
                db.SaveChanges();
                return "ثبت اطلاعات با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public string Update(RememberMe rm)
        {
            try
            {
                RememberMe q = db.RememberMe.FirstOrDefault();
                q.UserName = rm.UserName;
                q.IsRemembered = rm.IsRemembered;
                if (rm.LastLoginTime != null)
                {
                    q.LastLoginTime = rm.LastLoginTime;
                }
                db.SaveChanges();
                return "ویرایش با موفیقت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
        public RememberMe Load()
        {
            try
            {
                var q = db.RememberMe.FirstOrDefault();
                if (q != null)
                {
                    return q;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
