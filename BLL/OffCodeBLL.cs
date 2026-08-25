using BE;
using DAL;
using System.Data;

namespace BLL
{
    public class OffCodeBLL
    {
        OffCodeDAL dal = new OffCodeDAL();
        public string Create(OffCode o)
        {
            if (!dal.Exist(o.Code))
            {
                return dal.Create(o);
            }
            return "کد تخفیفی با همین کد قبلا ثیت شده است";
        }
        public OffCode GetOffCode(string code)
        {
            return dal.GetOffCode(code);
        }
        public DataTable Read()
        {
            return dal.Read();
        }
        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }
        public string OffCodeCount()
        {
            return dal.OffCodeCount();
        }
        public string Delete(string code)
        {
            return dal.Delete(code);
        }
        public string Update(OffCode c, string code)
        {
            return dal.Update(c, code);
        }
        public string CanUse(string code, Customer c)
        {
            return dal.CanUse(code, c);
        }

    }
}
