using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class ActivityCategoryBLL
    {
        ActivityCategoryDAL dal = new ActivityCategoryDAL();
        public string Create(ActivityCategory a)
        {
            if (!dal.Exist(a))
            {
                return dal.Create(a);
            }
            else
            {
                return "دسته بندی با همین نام در سیستم وجود دارد اگر نمیتوانید آنرا پیدا کنید از منوی تنظیمات بخش بازیابی اطلاعات را مشاهده کنید";
            }
        }


        public DataTable Read()
        {
            return dal.Read();
        }


        public ActivityCategory ReadById(int id)
        {
            return dal.ReadById(id);
        }


        public string Delete(int id)
        {
            return dal.Delete(id);
        }



        public string Update(ActivityCategory activity, int id)
        {
            if (!dal.Exist(activity))
            {

                return dal.Update(activity, id);
            }
            else
            {
                return "دسته بندی با همین نام در سیستم وجود دارد اگر نمیتوانید آنرا پیدا کنید از منوی تنظیمات بخش بازیابی اطلاعات را مشاهده کنید";
            }

        }


        public List<string> ActivityCategoryReadNames()
        {
            return dal.ActivityCategoryReadNames();
        }



        public ActivityCategory ReadByName(string Name)
        {
            return dal.ReadByName(Name);
        }

    }
}
