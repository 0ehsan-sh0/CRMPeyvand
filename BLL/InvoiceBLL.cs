using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class InvoiceBLL
    {
        InvoiceDAL dal = new InvoiceDAL();

        /// <summary>
        /// settledBy is non-null when the «وضعیت پرداخت» checkbox was ticked,
        /// meaning the full payable arrived at the counter as the invoice was
        /// written.
        /// </summary>
        public Invoice Create(Invoice invoice, int customerId, IReadOnlyList<InvoiceLine> lines, Payment settledBy)
        {
            return dal.Create(invoice, customerId, lines, StockPolicy.Validate, Pricing.ComputeDiscount, settledBy);
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
