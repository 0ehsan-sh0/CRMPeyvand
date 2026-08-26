using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class UserGroupBLL
    {
        UserGroupDAL dal = new UserGroupDAL();

        public string Create(UserGroup userGroup)
        {
            if (!dal.Exist(userGroup.Title))
            {
                return dal.Create(userGroup);
            }
            else
            {
                return "گروه کاربری با همین نام ثبت شده است";
            }
        }
        public string CreateAdmin(UserGroup userGroup)
        {
            return dal.Create(userGroup);
        }
        public List<string> ReadTitles()
        {
            return dal.ReadTitles();
        }

        public DataTable Read()
        {
            return dal.Read();
        }

        public bool ReadByTitle(string Title)
        {
            return dal.ReadByTitle(Title);
        }

        public UserGroup ReadBySingelTitle(string title)
        {
            return dal.ReadBySingelTitle(title);
        }
        public void Update(int groupId, string title, List<AccessGrant> grants)
        {
            dal.Update(groupId, title, grants);
        }
        public UserGroup AdminUserGroup()
        {
            return UserGroupDAL.AdminUserGroup();
        }
    }
}
