using BE;
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
using System.Windows.Shapes;
using BLL;
using System.IO;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }
        SettingBLL bll = new SettingBLL();
        User u = new User();
        UserBLL Ubll = new UserBLL();

        string BackUPDB(string userPath)
        {

            string path = userPath + @"/CRMPeyvand.bak";

            if (!Directory.Exists(userPath))
            {
                MessageBox.Show("مسیر مورد نظر برای ذخیره فایل پیدا نشد", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            else
            {
                return bll.BackUp(path);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ActivityCategoryForm activityCategoryForm = new ActivityCategoryForm();
            activityCategoryForm.ShowDialog();
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void btnSMSActivation_Click(object sender, RoutedEventArgs e)
        {
            ReportForm reportForm = new ReportForm();
            reportForm.ShowDialog();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
            }
        }

        private void btnDataBase_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.Cancel)
            {
                if (folderBrowserDialog1.SelectedPath != null)
                {
                    MessageBox.Show(BackUPDB(folderBrowserDialog1.SelectedPath), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

        }

        private void Info_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("برای حذف و بازیابی اطلاعات و پایگاه داده باید به برنامه مختص پایگاه داده مراجع نمایید\n" + "SQL Server\n" + "در صورت آشنا نبودن با برنامه با برنامه نویس تماس گرفته و یا در اینترنت آموزش بازگردانی پایگاه داده را سرچ کنید", "راهنمای بازیابی", MessageBoxButton.OK);
        }

        private void btnDeleteStatus_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirm = MessageBox.Show("آیا از این کار مطمعن هستید ؟ تمامی اطلاعاتی که شما حذف کرده اید مشتریان فعالیت ها و دیگر اطلاعات بازگردانی میشود", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.Yes)
            {
                MessageBox.Show(bll.Recovery(), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!Ubll.Access(u, "بخش تنظیمات", 2))
            {
                Grid grid = this.FindName("MainGrid") as Grid;
                for (int i = grid.Children.Count - 1; i >= 0; i--)
                {
                    if (grid.Children[i] is Border)
                    {
                        Border b = (Border)grid.Children[i];
                        if (b.Child is Button)
                        {
                            Button btn = (Button)b.Child;
                            btn.IsEnabled = false;
                        }
                    }
                }
            }
            else
            {
                Grid grid = this.FindName("MainGrid") as Grid;
                for (int i = grid.Children.Count - 1; i >= 0; i--)
                {
                    if (grid.Children[i] is Border)
                    {
                        Border b = (Border)grid.Children[i];
                        if (b.Child is Button)
                        {
                            Button btn = (Button)b.Child;
                            btn.IsEnabled = true;
                        }
                    }
                }
            }

        }
    }
}
