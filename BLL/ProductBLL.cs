using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class ProductBLL
    {
        ProductDAL dal = new ProductDAL();
        public string Create(Product p)
        {
            if (dal.Exist(p) == false)
            {
                return dal.Create(p);
            }
            return "محصول تکراری است ممکن است در سطل زباله باشد";
        }


        public DataTable Read()
        {
            return dal.Read();
        }
        public DataTable Read(string type)
        {
            return dal.Read(type);
        }
        public List<string> ReadNames()
        {
            return dal.ReadNames();
        }

        public Product ReadByid(int id)
        {
            return dal.ReadById(id);
        }


        public string Update(Product p, int id)
        {
            return dal.Update(p, id);
        }


        public string Delete(int id)
        {
            return dal.Delete(id);
        }



        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }


        public List<Product> ReadByTotal()
        {
            return dal.ReadByTotal();
        }


        public Product ReadByName(string product)
        {
            return dal.ReadByName(product);
        }


        public string ProductsCount()
        {
            return dal.ProductsCount();
        }

    }
}
