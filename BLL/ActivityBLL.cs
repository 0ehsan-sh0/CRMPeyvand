using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class ActivityBLL
    {
        ActivityDAL dal = new ActivityDAL();
        public string Create(Activity activity)
        {
            return dal.Create(activity);
        }

        public List<Activity> ReadAllWithDetails()
        {
            return dal.ReadAllWithDetails();
        }

        public DataTable Read()
        {
            return dal.Read();
        }
        public string Update(Activity a, int id)
        {
            return dal.Update(a, id);
        }

        public Activity ReadById(int id)
        {
            return dal.ReadById(id);
        }


        public string Delete(int id)
        {
            return dal.Delete(id);
        }
        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }


        public string ReadInfo(int id)
        {
            return dal.ReadInfo(id);
        }
        public string Count()
        {
            return dal.Count();
        }

    }
}
