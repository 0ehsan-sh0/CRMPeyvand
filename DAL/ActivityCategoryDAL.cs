using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class ActivityCategoryDAL
    {
        DB db = new DB();
        public string Create(ActivityCategory a)
        {
            try
            {
                db.ActivityCategories.Add(a);
                db.SaveChanges();
                return "دسته بندی با موفقیت ثبت شد";
            }
            catch (Exception e)
            {
                return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;

            }
        }

        public bool Exist(ActivityCategory a)
        {
            return db.ActivityCategories.Any(i => i.CategoryName == a.CategoryName);
        }

        public DataTable Read()
        {
            string Query = "SELECT  TOP (100)   id AS ردیف, CategoryName AS [نام دسته بندی]\r\nFROM          dbo.ActivityCategories\r\nWHERE      (DeleteStatus = 0) ORDER BY id DESC";
            string connectionStringText = @"Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true";
            SqlConnection connection = new SqlConnection(connectionStringText);
            var sqlAdapter = new SqlDataAdapter(Query, connection);
            var commandbuilder = new SqlCommandBuilder(sqlAdapter);
            var dataset = new DataSet();
            sqlAdapter.Fill(dataset);
            return dataset.Tables[0];
        }



        public ActivityCategory ReadById(int id)
        {
            return db.ActivityCategories.Find(id);
        }



        public string Delete(int id)
        {
            try
            {
                var q = db.ActivityCategories.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.DeleteStatus = true;
                    db.SaveChanges();
                    return "حذف با موفیقت انجام شد";
                }
                else
                {
                    return "دسته بندی مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "حذف اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }



        public string Update(ActivityCategory activity, int id)
        {
            try
            {
                var q = db.ActivityCategories.Where(i => i.id == id).FirstOrDefault();
                if (q != null)
                {
                    q.CategoryName = activity.CategoryName;
                    db.SaveChanges();
                    return "ویرایش با موفیقت انجام شد";
                }
                else
                {
                    return "دسته بندی مورد نظر یافت نشد";
                }
            }
            catch (Exception e)
            {

                return "ویرایش اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
            }
        }



        public List<string> ActivityCategoryReadNames()
        {
            return db.ActivityCategories.Where(i => i.DeleteStatus == false).Select(i => i.CategoryName).ToList();
        }


        public ActivityCategory ReadByName(string Name)
        {
            return db.ActivityCategories.Where(u => u.CategoryName == Name).SingleOrDefault();
        }

    }
}
