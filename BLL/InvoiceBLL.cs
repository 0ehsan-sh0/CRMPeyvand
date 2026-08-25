using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class InvoiceBLL
    {
        InvoiceDAL dal = new InvoiceDAL();

        public string Create(Invoice invoice, Customer customer, List<CatalogItem> products)
        {
            return dal.Create(invoice, customer, products);
        }


        public string ReadInvoiceNumIsReport()
        {
            return dal.ReadInvoiceNumIsReport();
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


        public string Delete(string n)
        {
            return dal.Delete(n);
        }


        public string Done(int id)
        {
            return dal.Done(id);
        }


        public Invoice ReadById(int id)
        {
            return dal.ReadById(id);
        }

        public Invoice Read(string number)
        {
            return dal.Read(number);
        }
        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }
    }
}
