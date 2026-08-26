using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class UserBLL
    {
        public bool HasAnyUser()
        {
            return dal.HasAnyUser();
        }

        public List<string> ReadUserNamesList()
        {
            return dal.ReadUserNamesList();
        }

        UserDAL dal = new UserDAL();
        public string Create(User user)
        {
            if (!dal.Exsit(user))
            {
                user.Password = PasswordHasher.Hash(user.Password);
                return dal.Create(user);
            }
            else
            {
                return "نام کاربری وارد شده تکراری است";
            }
        }
        public string Update(User user, int UserId)
        {
            if (!string.IsNullOrWhiteSpace(user.Password))
                user.Password = PasswordHasher.Hash(user.Password);
            return dal.Update(user, UserId);
        }
        public DataTable Read()
        {
            return dal.Read();
        }
        public User ReadByUserName(string Name)
        {
            return dal.ReadByUserName(Name);
        }
        public string Delete(int UserId)
        {
            return dal.Delete(UserId);
        }
        public User Login(string UserName, string Password)
        {
            User user = dal.FindByUserName(UserName);
            if (user == null || string.IsNullOrEmpty(user.Password))
                return null;
            return PasswordHasher.Verify(Password, user.Password) ? user : null;
        }
        public List<User> ReadInvoicesList()
        {
            return dal.ReadInvoicesList();
        }
        public List<User> ReadActivitiesList()
        {
            return dal.ReadActivitiesList();
        }
    }
}
