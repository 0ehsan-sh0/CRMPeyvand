using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BLL
{
    public class UserBLL
    {
        //Encode For Password Is Security...
        private string Encode(string Pass)
        {
            try
            {
                byte[] enData_byte = new byte[Pass.Length];
                enData_byte = Encoding.UTF8.GetBytes(Pass);
                string encodeData = Convert.ToBase64String(enData_byte);
                return encodeData;
            }
            catch (Exception e)
            {

                return " اطلاعات با مشکلاتی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }

        }
        public bool IsActive()
        {
            return dal.IsActive();
        }

        public List<string> ReadUserNamesList()
        {
            return dal.ReadUserNamesList();
        }

        //Decode For Pass
        private string Decode(string EncodePass)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder decoder = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(EncodePass);
            int charCount = decoder.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] charArray = new char[charCount];
            decoder.GetChars(todecode_byte, 0, todecode_byte.Length, charArray, 0);
            string result = new string(charArray);
            return result;
        }

        UserDAL dal = new UserDAL();
        public string Create(User user)
        {
            if (!dal.Exsit(user))
            {
                user.Password = Encode(user.Password);
                return dal.Create(user);
            }
            else
            {
                return "نام کاربری وارد شده تکراری است";
            }
        }
        public string Update(User user, int UserId)
        {
            user.Password = Encode(user.Password);
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
            return dal.Login(UserName, Encode(Password));
        }
        public bool Access(User user, string Section, int number)
        {
            return dal.Access(user, Section, number);
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
