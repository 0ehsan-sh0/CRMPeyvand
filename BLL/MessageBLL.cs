using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class MessageBLL
    {
        MessageDAL dal = new MessageDAL();
        public string Create(Message m)
        {
            if (!dal.Exist(m.Content))
            {
                return dal.Create(m);
            }
            else
            {
                return "پیامی با همین محتوا در سیستم وجود دارد";
            }
        }
        public DataTable Read()
        {
            return dal.Read();
        }
        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }
        public string Count()
        {
            return dal.Count();
        }
        public List<string> First10()
        {
            return dal.First10();
        }
        public List<string> First100()
        {
            return dal.First100();
        }
        public List<string> First1000()
        {
            return dal.First1000();
        }
        public List<string> First10000()
        {
            return dal.First10000();
        }
        public List<string> FirstBuy10()
        {
            return dal.FirstBuy10();
        }
        public List<string> FirstBuy100()
        {
            return dal.FirstBuy100();
        }
        public List<string> FirstBuy1000()
        {
            return dal.FirstBuy1000();
        }
        public List<string> FirstBuy10000()
        {
            return dal.FirstBuy10000();
        }
        public List<string> FirstBuy()
        {
            return dal.FirstBuy();
        }
        public List<string> NoBuy()
        {
            return dal.NoBuy();
        }
        public List<string> IsNotCheckedOut()
        {
            return dal.IsNotCheckedOut();
        }
    }
}
