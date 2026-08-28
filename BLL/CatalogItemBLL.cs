using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class CatalogItemBLL
    {
        CatalogItemDAL dal = new CatalogItemDAL();
        public string Create(CatalogItem p)
        {
            if (dal.Exist(p) == false)
            {
                return dal.Create(p);
            }
            return "محصول تکراری است ممکن است در سطل زباله باشد";
        }


        public List<CatalogItem> ReadAll()
        {
            return dal.ReadAll();
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

        public CatalogItem ReadByid(int id)
        {
            return dal.ReadById(id);
        }


        public string Update(CatalogItem p, int id)
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


        public CatalogItem ReadByName(string product)
        {
            return dal.ReadByName(product);
        }


        public string ProductsCount()
        {
            return dal.ProductsCount();
        }

    }
}
