using BE;
using DAL;

namespace BLL
{
    public class MessagePanelBLL
    {
        MessagePanelDAL dal = new MessagePanelDAL();
        public string Create(MessagePanel m)
        {
            if (!dal.Exist())
            {
                return dal.Create(m);
            }
            else return "شما در حال حاضر اطلاعات خود را ثبت کرده اید\n" + "اگر اطلاعات نیاز به تغییر دارد لطفا از منوی تنظیمات اقدام کنید";
        }
        public bool Exist()
        {
            return dal.Exist();
        }
        public MessagePanel GetMessagePanel()
        {
            return dal.GetMessagePanel();
        }
        public string Update(MessagePanel m)
        {
            return dal.Update(m);
        }
    }
}
