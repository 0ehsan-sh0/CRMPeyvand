using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class CustomerBLL
    {
        CustomerDAL dal = new CustomerDAL();
        public string Create(Customer c)
        {
            if (!dal.Exist(c))
            {
                return dal.Create(c);
            }
            return "کاربری با این شماره تلفن در سیستم ثبت شده است.";
        }
        public Customer ReadByid(int id)
        {
            return dal.ReadById(id);
        }
        public DataTable Read()
        {
            return dal.Read();
        }
        public string Update(Customer customer, int id)
        {
            return dal.Update(customer, id);

        }
        public string Delete(int id)
        {
            return dal.Delete(id);
        }
        public List<string> ReadPhoneNumbers()
        {
            return dal.ReadPhoneNumbers();
        }
        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }
        public Customer GetCustomer(string phone)
        {
            return dal.GetCustomer(phone);
        }
        public string CustomerCount()
        {
            return dal.CustomerCount();
        }
        public List<Customer> ReadWithDateTime()
        {
            return dal.ReadWithDateTime();
        }

    }
}
