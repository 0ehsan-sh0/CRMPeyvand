using BE;
using DAL;
using System.Collections.Generic;

namespace BLL
{
    public class DashboardBLL
    {
        DashboardDAL dal = new DashboardDAL();
        public string CustomersCount()
        {
            return dal.CustomersCount();
        }


        public string SellsCountToday()
        {
            return dal.SellsCountToday();
        }
        public string UserReminderCount(User u)
        {
            return dal.UserReminderCount(u);
        }
        public List<Reminder> GetUserReminder(User user)
        {
            return dal.GetUserReminder(user);
        }
        public string SellsCountWeek()
        {
            return dal.SellsCountWeek();
        }
        public bool PanelIsActive()
        {
            return dal.PanelIsActive();
        }

        public string DebtorCustomerCount()
        {
            return dal.DebtorCustomerCount();
        }
    }
}
