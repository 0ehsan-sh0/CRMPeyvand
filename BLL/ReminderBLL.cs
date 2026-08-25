using BE;
using DAL;
using System.Data;

namespace BLL
{
    public class ReminderBLL
    {
        ReminderDAL dal = new ReminderDAL();
        public string Create(Reminder r, User u)
        {
            return dal.Create(r, u);
        }


        public DataTable Read()
        {
            return dal.Read();
        }


        public Reminder ReadById(int id)
        {
            return dal.ReadByID(id);
        }


        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }


        public string Update(Reminder r, int id)
        {
            return dal.Update(r, id);
        }


        public string Delete(int id)
        {
            return dal.Delete(id);
        }


        public string Done(int id)
        {
            return dal.Done(id);
        }
        public string Count()
        {
            return dal.Count();
        }


    }
}
