using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BLL;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for ReminderUC.xaml
    /// </summary>
    public partial class ReminderUC : UserControl
    {
        public ReminderUC()
        {
            InitializeComponent();
        }
        ReminderBLL bll = new ReminderBLL();
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(txtReminderInfo.Text,"توضیحات");
        }

        private void miDone_Click(object sender, RoutedEventArgs e)
        {
            
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            w.RefreshPage();
            MessageBox.Show(bll.Done(Convert.ToInt32(ReminderID.Text)) + " لطفا برای دیدن یاد آور های جدیدتر در صورت وجود برنامه را بسته و مجددا باز کنید", "اطلاعیه");
        }
    }
}
