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
using BE;
using BLL;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for ReportForm.xaml
    /// </summary>
    public partial class ReportForm : Window
    {
        public ReportForm()
        {
            InitializeComponent();
        }
        MessagePanelBLL bll = new MessagePanelBLL();
        private void txtLineNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            t.Clear();
            t.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!bll.Exist())
            {
                lblDetails.Content = "پنل پیامکی خود را فعال کنید";
                btnAdd.Content = "ثبت اطلاعات";
            }
            else
            {
                MessagePanel m = bll.GetMessagePanel();
                txtAPI.Text = m.APIToken;
                txtLineNumber.Text = m.LineNumber;
                lblDetails.Content = "تغییر اطلاعات پنل پیامکی";
                btnAdd.Content = "ویرایش اطلاعات";
                txtAPI.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                txtLineNumber.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }
        private void txtLineNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            PublicMethods.FilterNumber((TextBox)sender);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(
                "برای فعالسازی پنل پیامکی ابتدا باید در سایت زیر ثبت نام کنید\n" +
                "SMS.IR\n" +
                "سپس شما باید از بخش شماره پیامکی در زیر بخش شماره های من شماره ای که سایت به شما داده است را فعال و در فیلد دوم قرار دهید\n" +
                "در نهایت از منوی برنامه نویسان زیر بخش کلید های ای پی آی یک کلید جدید برای خود ایجاد و آنرا در فیلد اول قرار دهید\n" +
                "شما میتوانید این اطلاعات را از منوی تنظیمات تغییر دهید\n" +
                "برای مطمعن شدن از ارسال پیام ابتدا یک پیغام برای شماره خود ارسال کنید"
                );
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtAPI.Text != String.Empty && txtLineNumber.Text != String.Empty && btnAdd.Content.ToString() == "ثبت اطلاعات")
            {
                MessageBoxResult Confirmation = MessageBox.Show("آیا از درستی اطلاعاتی که در حال ثبت آن هستید مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (Confirmation == MessageBoxResult.Yes)
                {
                    MessagePanel messagePanel = new MessagePanel();
                    messagePanel.APIToken = txtAPI.Text;
                    messagePanel.LineNumber = txtLineNumber.Text;
                    MessageBox.Show(bll.Create(messagePanel), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            else if (txtAPI.Text != String.Empty && txtLineNumber.Text != String.Empty && btnAdd.Content.ToString() == "ویرایش اطلاعات")
            {
                MessageBoxResult Confirmation = MessageBox.Show("آیا از درستی اطلاعاتی که در حال ثبت آن هستید مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (Confirmation == MessageBoxResult.Yes)
                {
                    MessagePanel messagePanel = new MessagePanel();
                    messagePanel.APIToken = txtAPI.Text;
                    messagePanel.LineNumber = txtLineNumber.Text;
                    MessageBox.Show(bll.Update(messagePanel), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            else MessageBox.Show("لطفا تمامی فیلد هارا پر کنید", "اطلاعیه");
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
    }
}
