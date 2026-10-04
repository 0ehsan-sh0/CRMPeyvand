using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class PaymentBLL
    {
        PaymentDAL dal = new PaymentDAL();

        public string Create(Payment payment, int invoiceId)
        {
            return dal.Create(payment, invoiceId, SettlementPolicy.ValidateRecording);
        }

        public string Void(int id)
        {
            return dal.Void(id);
        }

        public DataTable Read()
        {
            return dal.Read();
        }

        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }

        public string Count()
        {
            return dal.Count();
        }

        public Payment ReadById(int id)
        {
            return dal.ReadById(id);
        }

        public List<CustomerBalance> ReadCustomerBalances()
        {
            return dal.ReadCustomerBalances();
        }
    }
}