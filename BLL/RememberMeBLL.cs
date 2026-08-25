using BE;
using DAL;

namespace BLL
{
    public class RememberMeBLL
    {
        RememberMeDAL dal = new RememberMeDAL();
        public string Create(RememberMe rm)
        {
            if (!dal.Exist())
            {
                return dal.Create(rm);
            }
            else
            {
                return dal.Update(rm);
            }
        }
        public RememberMe Load()
        {
            return dal.Load();
        }

    }
}
