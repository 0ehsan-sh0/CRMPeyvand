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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BE;
using BLL;
using IPE.SmsIrClient.Models.Requests;
using IPE.SmsIrClient;
using IPE.SmsIrClient.Models.Results;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for SMSForm.xaml
    /// </summary>
    public partial class SMSForm : Window
    {
        public SMSForm()
        {
            InitializeComponent();
        }
        MessageBLL bll = new MessageBLL();
        MessagePanelBLL MPbll = new MessagePanelBLL();
        CustomerBLL Cbll = new CustomerBLL();
        List<string> PhoneNumbers = new List<string>();
        Customer Cgroup = new Customer();
        Customer Csingle = new Customer();
        User u = new User();
        UserBLL Ubll = new UserBLL();
        void openform(Window f)
        {
            BlurEffect bme = new BlurEffect();
            this.Effect = bme;
            bme.Radius = 15;
            f.ShowDialog();
            Effect = null;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!Ubll.Access(u, "پنل پیامکی", 2))
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
                txtSuggestionList.IsEnabled = false;
                txtSearch.IsEnabled = false;
                txtCustomerGroupSearch.IsEnabled = false;
                txtCustomerSingleSearch.IsEnabled = false;
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
                txtSuggestionList.IsEnabled = true;
                txtSearch.IsEnabled = true;
                txtCustomerGroupSearch.IsEnabled = true;
                txtCustomerSingleSearch.IsEnabled = true;
            }
            

            List<string> list = new List<string>()
            {
                "10 مشتری تازه ثبت نام شده",
                "100 مشتری تازه ثبت نام شده",
                "1000 مشتری تازه ثبت نام شده",
                "10000 مشتری تازه ثبت نام شده",
                "10 مشتری اول با بیشترین خرید",
                "100 مشتری اول با بیشترین خرید",
                "1000 مشتری اول با بیتشرین خرید",
                "10000 مشتری اول با بیتشرین خرید",
                "مشتری هایی که یکبار خرید کرده اند",
                "مشتری هایی که خریدی انجام نداده اند",
                "مشتری هایی که خرید بدون پرداخت دارند"
            };
            txtSuggestionList.ItemsSource = list;
            Cgroup = null;
            lblCount.Content = bll.Count();
            txtCustomerGroupSearch.ItemsSource = Cbll.ReadPhoneNumbers();
            txtCustomerSingleSearch.ItemsSource = Cbll.ReadPhoneNumbers();
            PublicMethods.dgvFiller(dgvSMS, bll.Read());
            PublicMethods.DGVAutoSizeColumnFill(dgvSMS);
        }

        private void IsChecked_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            PublicMethods.CheckBoxMouseLeave(border);
        }

        private void IsChecked_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            PublicMethods.CheckBoxMouseEnter(border);
        }

        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void btnOffCode_Click(object sender, RoutedEventArgs e)
        {
            OffCodeForm form = new OffCodeForm();
            openform(form);
        }
        private void btnSingleMessageSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtSMSSingleInfo.Text == "متن پیام")
            {
                MessageBox.Show("فیلد متن پیام خالی است", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (txtSMSSingleInfo.Text != String.Empty)
            {
                Message m = new Message();
                m.Content = txtSMSSingleInfo.Text;
                MessageBox.Show(bll.Create(m), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                PublicMethods.dgvFiller(dgvSMS, bll.Read());
            }
            else MessageBox.Show("فیلد متن پیام خالی است", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnGroupMessageSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtSMSGroupInfo.Text == "متن پیام")
            {
                MessageBox.Show("فیلد متن پیام خالی است", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (txtSMSGroupInfo.Text != String.Empty)
            {
                Message m = new Message();
                m.Content = txtSMSGroupInfo.Text;
                MessageBox.Show(bll.Create(m), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                PublicMethods.dgvFiller(dgvSMS, bll.Read());
            }
            else MessageBox.Show("فیلد متن پیام خالی است", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtSMSSingleInfo_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (t.Text == "متن پیام")
            {
                t.Clear();
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text != String.Empty)
            {
                PublicMethods.dgvFiller(dgvSMS, bll.Search(txtSearch.Text));
            }
            else PublicMethods.dgvFiller(dgvSMS, bll.Read());                
        }

        private void btnSendToListBox_Click(object sender, RoutedEventArgs e)
        {
            if (Cgroup != null)
            {
                bool Exist = false;
                foreach (var item in PhoneNumbers)
                {
                    if (Cgroup.Phone == item)
                    {
                        Exist = true;
                    }
                }
                if (!Exist)
                {
                    PhoneNumbers.Add(Cgroup.Phone);
                }
                else MessageBox.Show("شماره تلفن در لیست وجود دارد", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);

                lstCustomers.ItemsSource = null;
                lstCustomers.ItemsSource = PhoneNumbers;
            }
            else MessageBox.Show("شما هنوز شماره تلفنی را انتخاب نکرده اید", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtCustomerGroupSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtCustomerGroupSearch.SelectedItem != null)
            {
                Cgroup = Cbll.GetCustomer(txtCustomerGroupSearch.SelectedItem.ToString());
            }
        }

        private void txtCustomerSingleSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtCustomerSingleSearch.SelectedItem != null)
            {
                Csingle = Cbll.GetCustomer(txtCustomerSingleSearch.SelectedItem.ToString());
            }
        }

        private void AddPhoneToList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSuggestionList.SelectedItem != null)
            {
                switch (txtSuggestionList.SelectedItem.ToString())
                {
                    case "10 مشتری تازه ثبت نام شده":
                        foreach (var item in bll.First10())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "100 مشتری تازه ثبت نام شده":
                        foreach (var item in bll.First100())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "1000 مشتری تازه ثبت نام شده":
                        foreach (var item in bll.First1000())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "10000 مشتری تازه ثبت نام شده":
                        foreach (var item in bll.First10000())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "10 مشتری اول با بیشترین خرید":
                        foreach (var item in bll.FirstBuy10())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "100 مشتری اول با بیشترین خرید":
                        foreach (var item in bll.FirstBuy100())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "1000 مشتری اول با بیتشرین خرید":
                        foreach (var item in bll.FirstBuy1000())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "10000 مشتری اول با بیتشرین خرید":
                        foreach (var item in bll.FirstBuy10000())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "مشتری هایی که یکبار خرید کرده اند":
                        foreach (var item in bll.FirstBuy())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "مشتری هایی که خریدی انجام نداده اند":
                        foreach (var item in bll.NoBuy())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;

                    case "مشتری هایی که خرید بدون پرداخت دارند":
                        foreach (var item in bll.IsNotCheckedOut())
                        {
                            bool Exist = false;
                            foreach (var Mitem in PhoneNumbers)
                            {
                                if (item == Mitem)
                                {
                                    Exist = true;
                                }
                            }
                            if (!Exist)
                            {
                                PhoneNumbers.Add(item);
                            }
                        }
                        lstCustomers.ItemsSource = null;
                        lstCustomers.ItemsSource = PhoneNumbers;
                        break;
                }
            }
        }

        private void EmptyList_Click(object sender, RoutedEventArgs e)
        {
            lstCustomers.ItemsSource = null;
            PhoneNumbers.Clear();
        }
        //SmsIr Sending >>>>>>>>>>>>>
        private async void btnSendSingle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessagePanel m = MPbll.GetMessagePanel();
                SmsIr smsIr = new SmsIr(m.APIToken);
                var bulkSendResult = await smsIr.BulkSendAsync(Convert.ToInt64(m.LineNumber), txtSMSSingleInfo.Text, new string[] { Csingle.Phone });
                MessageBox.Show(bulkSendResult.Message, "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCustomerSingleSearch.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ارسال پیام رخ داد.مطمعن شوید که کد ای پی آی و شماره ارسال پیامک را به  درستی وارد کرده اید \n" + "خطا\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnSendGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessagePanel m = MPbll.GetMessagePanel();
                SmsIr smsIr = new SmsIr(m.APIToken);
                var bulkSendResult = await smsIr.BulkSendAsync(Convert.ToInt64(m.LineNumber), txtSMSGroupInfo.Text, PhoneNumbers.ToArray());
                MessageBox.Show(bulkSendResult.Message, "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCustomerGroupSearch.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ارسال پیام رخ داد.مطمعن شوید که کد ای پی آی و شماره ارسال پیامک را به  درستی وارد کرده اید \n" + "خطا\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void btnSendAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessagePanel m = MPbll.GetMessagePanel();
                SmsIr smsIr = new SmsIr(m.APIToken);
                var bulkSendResult = await smsIr.BulkSendAsync(Convert.ToInt64(m.LineNumber), txtSMSGroupInfo.Text, Cbll.ReadPhoneNumbers().ToArray());
                MessageBox.Show(bulkSendResult.Message, "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCustomerGroupSearch.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ارسال پیام رخ داد.مطمعن شوید که کد ای پی آی و شماره ارسال پیامک را به  درستی وارد کرده اید \n" + "خطا\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //<<<<<<<<<<<<< SmsIr Sending
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
