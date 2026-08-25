using DAL;

namespace BLL
{
    public class SettingBLL
    {
        SettingDAL dal = new SettingDAL();
        public string BackUp(string Path)
        {
            return dal.BackUp(Path);
        }
        public string Recovery()
        {
            return dal.Recovery();
        }

    }
}
