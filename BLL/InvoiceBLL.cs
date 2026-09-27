using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class InvoiceBLL
    {
        InvoiceDAL dal = new InvoiceDAL();

        public Invoice Create(Invoice invoice, int customerId, IReadOnlyList<InvoiceLine> lines)
        {
            return dal.Create(invoice, customerId, lines, StockPolicy.Validate, Pricing.ComputeDiscount);
        }


        public int ReadInvoiceLastID()
        {
            return dal.ReadInvoiceLastID();
        }


        public string CountInvoices()
        {
            return dal.CountInvoices();
        }


        public DataTable Read()
        {
            return dal.Read();
        }


        public string Delete(int id)
        {
            return dal.Delete(id);
        }


        public string Done(int id)
        {
            return dal.Done(id);
        }


        public Invoice ReadById(int id)
        {
            return dal.ReadById(id);
        }

        public Invoice ReadDetails(int id)
        {
            return dal.ReadDetails(id);
        }

        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }
    }
}
