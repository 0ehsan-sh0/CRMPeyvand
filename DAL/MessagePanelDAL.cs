using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class MessagePanelDAL
    {
        DB db = new DB();
        public string Create(MessagePanel m)
        {
            try
            {
                db.MessagePanels.Add(m);
                db.SaveChanges();
                return "ثبت اطلاعات با موفقیت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }

        }
        public bool Exist()
        {
            return db.MessagePanels.Count() > 0;
        }
        public MessagePanel GetMessagePanel()
        {
            return db.MessagePanels.FirstOrDefault();
        }
        public string Update(MessagePanel m)
        {
            try
            {
                MessagePanel p = db.MessagePanels.FirstOrDefault();
                p.APIToken = m.APIToken;
                p.LineNumber = m.LineNumber;
                p.EditDate = m.EditDate;
                db.SaveChanges();
                return "ویرایش با موفیقت انجام شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }
    }
}
