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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BE;
using Section = BE.Section;
using BLL;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(timer_tick);
            timer.Start();
        }
        UserBLL Ubll = new UserBLL();
        DashboardBLL Dbll = new DashboardBLL();
        public User loggedInUser = new User();
        bool EnterC;
        bool EnterP;
        bool EnterF;
        bool EnterA;
        bool EnterU;
        bool EnterM;
        bool EnterG;
        bool EnterR;
        bool EnterS;
        bool EnterV;
        public void LoadPage()
        {
            #region CanEnter
            if (loggedInUser != null)
            {
                if (!AccessGuard.Can(loggedInUser, Section.Customers, Operation.View))
                {
                    EnterC = false;
                    CustomerFormIcon.IsEnabled = false;
                    CustomerFormLabel.IsEnabled = false;
                    CustomerFormIcon.Opacity = 0.7;
                    CustomerFormLabel.Opacity = 0.7;
                }
                else
                {
                    EnterC = true;
                    CustomerFormIcon.IsEnabled = true;
                    CustomerFormLabel.IsEnabled = true;
                    CustomerFormIcon.Opacity = 1;
                    CustomerFormLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.CatalogItems, Operation.View))
                {
                    EnterP = false;
                    ProductIcon.IsEnabled = false;
                    ProductLabel.IsEnabled = false;
                    ProductIcon.Opacity = 0.7;
                    ProductLabel.Opacity = 0.7;
                }
                else
                {
                    EnterP = true;
                    ProductIcon.IsEnabled = true;
                    ProductLabel.IsEnabled = true;
                    ProductIcon.Opacity = 1;
                    ProductLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.Invoices, Operation.View))
                {
                    EnterF = false;
                    InvoiceIcon.IsEnabled = false;
                    InvoiceLabel.IsEnabled = false;
                    InvoiceIcon.Opacity = 0.7;
                    InvoiceLabel.Opacity = 0.7;
                }
                else
                {
                    EnterF = true;
                    InvoiceIcon.IsEnabled = true;
                    InvoiceLabel.IsEnabled = true;
                    InvoiceIcon.Opacity = 1;
                    InvoiceLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.Payments, Operation.View))
                {
                    EnterV = false;
                    PaymentIcon.IsEnabled = false;
                    PaymentLabel.IsEnabled = false;
                    PaymentIcon.Opacity = 0.7;
                    PaymentLabel.Opacity = 0.7;
                }
                else
                {
                    EnterV = true;
                    PaymentIcon.IsEnabled = true;
                    PaymentLabel.IsEnabled = true;
                    PaymentIcon.Opacity = 1;
                    PaymentLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.Activities, Operation.View))
                {
                    EnterA = false;
                    ActivitiesIcon.IsEnabled = false;
                    ActivitiesLabel.IsEnabled = false;
                    ActivitiesIcon.Opacity = 0.7;
                    ActivitiesLabel.Opacity = 0.7;
                }
                else
                {
                    EnterA = true;
                    ActivitiesIcon.IsEnabled = true;
                    ActivitiesLabel.IsEnabled = true;
                    ActivitiesIcon.Opacity = 1;
                    ActivitiesLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.Reminders, Operation.View))
                {
                    EnterR = false;
                    ReminderFormIcon.IsEnabled = false;
                    ReminderFormLabel.IsEnabled = false;
                    ReminderFormIcon.Opacity = 0.7;
                    ReminderFormLabel.Opacity = 0.7;
                }
                else
                {
                    EnterR = true;
                    ReminderFormIcon.IsEnabled = true;
                    ReminderFormLabel.IsEnabled = true;
                    ReminderFormIcon.Opacity = 1;
                    ReminderFormLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.Users, Operation.View))
                {
                    EnterU = false;
                    UserIcon.IsEnabled = false;
                    UserLabel.IsEnabled = false;
                    UserIcon.Opacity = 0.7;
                    UserLabel.Opacity = 0.7;
                }
                else
                {
                    EnterU = true;
                    UserIcon.IsEnabled = true;
                    UserLabel.IsEnabled = true;
                    UserIcon.Opacity = 1;
                    UserLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.SmsPanel, Operation.View))
                {
                    EnterM = false;
                    SMSIcon.IsEnabled = false;
                    SMSLabel.IsEnabled = false;
                    SMSIcon.Opacity = 0.7;
                    SMSLabel.Opacity = 0.7;
                }
                else
                {
                    EnterM = true;
                    SMSIcon.IsEnabled = true;
                    SMSLabel.IsEnabled = true;
                    SMSIcon.Opacity = 1;
                    SMSLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.Reports, Operation.View))
                {
                    EnterG = false;
                    ReportIcon.IsEnabled = false;
                    ReportLabel.IsEnabled = false;
                    ReportIcon.Opacity = 0.7;
                    ReportLabel.Opacity = 0.7;
                }
                else
                {
                    EnterG = true;
                    ReportIcon.IsEnabled = true;
                    ReportLabel.IsEnabled = true;
                    ReportIcon.Opacity = 1;
                    ReportLabel.Opacity = 1;
                }

                if (!AccessGuard.Can(loggedInUser, Section.Settings, Operation.View))
                {
                    EnterS = false;
                    SettingIcon.IsEnabled = false;
                    SettingIcon.Visibility = Visibility.Hidden;
                }
                else
                {
                    EnterS=true;
                    SettingIcon.IsEnabled = true;
                    SettingIcon.Visibility = Visibility.Visible;
                }
            }
            else
            {
                CustomerFormIcon.IsEnabled = false;
                CustomerFormLabel.IsEnabled = false;
                ProductIcon.IsEnabled = false;
                ProductLabel.IsEnabled = false;
                InvoiceIcon.IsEnabled = false;
                InvoiceLabel.IsEnabled = false;
                ActivitiesIcon.IsEnabled = false;
                ActivitiesLabel.IsEnabled = false;
                ReminderFormIcon.IsEnabled = false;
                ReminderFormLabel.IsEnabled = false;
                UserIcon.IsEnabled = false;
                UserLabel.IsEnabled = false;
                SMSIcon.IsEnabled = false;
                SMSLabel.IsEnabled = false;
                ReportIcon.IsEnabled = false;
                ReportLabel.IsEnabled = false;
                PaymentIcon.IsEnabled = false;
                PaymentLabel.IsEnabled = false;
                SettingIcon.IsEnabled = false;
                SettingIcon.Visibility = Visibility.Hidden;
            }
            #endregion
            RefreshPage();
        }
        public void RefreshPage()
        {
            if (loggedInUser != null)
            {
                lblUsername.Content = loggedInUser.Name ?? "";

                // Safe user avatar loading with fallback
                try
                {
                    if (!string.IsNullOrWhiteSpace(loggedInUser.Picture) && System.IO.File.Exists(loggedInUser.Picture))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(loggedInUser.Picture, UriKind.RelativeOrAbsolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        UserImage.Source = bitmap;
                    }
                    else
                    {
                        UserImage.Source = new BitmapImage(new Uri("/Images/User.png", UriKind.RelativeOrAbsolute));
                    }
                }
                catch
                {
                    try
                    {
                        UserImage.Source = new BitmapImage(new Uri("/Images/User.png", UriKind.RelativeOrAbsolute));
                    }
                    catch
                    {
                    }
                }

                try
                {
                    TodaySells.Content = Dbll.SellsCountToday();
                    lblCustomersCount.Content = Dbll.CustomersCount();
                    lblCountSellsWeek.Content = Dbll.SellsCountWeek();
                    lblReminderCount.Content = Dbll.UserReminderCount(loggedInUser);
                    lblDebtorCount.Content = Dbll.DebtorCustomerCount();
                }
                catch
                {
                }

                int a = 0;
                // Get the Grid from the MainWindow. 
                Grid grid = this.FindName("MainGrid") as Grid;

                if (grid != null)
                {
                    // Loop through all the children of the Grid. 
                    for (int i = grid.Children.Count - 1; i >= 0; i--)
                    {
                        // Check if the child is a ReminderUC. 
                        if (grid.Children[i] is ReminderUC)
                        {
                            grid.Children.RemoveAt(i);
                        }
                    }
                }

                try
                {
                    var userReminders = Dbll.GetUserReminder(loggedInUser);
                    if (userReminders != null)
                    {
                        foreach (var item in userReminders)
                        {
                            if (a < 5)
                            {
                                //For UserControl Reminder
                                ReminderUC uC_Reminder = new ReminderUC();
                                uC_Reminder.txtReminderTitle.Text = item.Title;
                                uC_Reminder.txtReminderInfo.Text = item.Info;
                                uC_Reminder.ReminderID.Text = item.id.ToString();
                                // Add in Children. Row 7 is the first row inside the
                                // reminders panel, which starts at row 6 now that the
                                // small stat boxes take row 5.
                                Grid.SetRow(uC_Reminder, 7 + a);
                                Grid.SetColumn(uC_Reminder, 0);
                                Grid.SetColumnSpan(uC_Reminder, 10);
                                uC_Reminder.Width = 1050;
                                MainGrid.Children.Add(uC_Reminder);
                                a++;
                            }
                        }
                    }
                }
                catch
                {
                }

                if (a == 0)
                {
                    lblReminderStatus.Content = "...در حال حاضر شما یادآوری برای امروز ندارید";
                }
                else
                {
                    lblReminderStatus.Content = "";
                }
            }
            else
            {
                CustomerFormIcon.IsEnabled = false;
                CustomerFormLabel.IsEnabled = false;
                ProductIcon.IsEnabled = false;
                ProductLabel.IsEnabled = false;
                InvoiceIcon.IsEnabled = false;
                InvoiceLabel.IsEnabled = false;
                ActivitiesIcon.IsEnabled = false;
                ActivitiesLabel.IsEnabled = false;
                ReminderFormIcon.IsEnabled = false;
                ReminderFormLabel.IsEnabled = false;
                UserIcon.IsEnabled = false;
                UserLabel.IsEnabled = false;
                SMSIcon.IsEnabled = false;
                SMSLabel.IsEnabled = false;
                ReportIcon.IsEnabled = false;
                ReportLabel.IsEnabled = false;
                PaymentIcon.IsEnabled = false;
                PaymentLabel.IsEnabled = false;
                SettingIcon.IsEnabled = false;
                SettingIcon.Visibility = Visibility.Hidden;
            }
        }
        void openform(Window f)
        {
            BlurEffect bme = new BlurEffect();
            this.Effect = bme;
            bme.Radius = 15;
            f.ShowDialog();
            Effect = null;
        }

        private void timer_tick(object sender, EventArgs e)
        {
            TimeText.Text = DateTime.Now.ToString();
        }

        private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult exit = MessageBox.Show("آیا میخواهید از برنامه خارج شوید ؟", "خروج", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (exit == MessageBoxResult.Yes)
            {
                App.Current.Shutdown();
            }
        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            label.Foreground = new SolidColorBrush(Color.FromRgb(92, 122, 255));
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            label.Foreground = new SolidColorBrush(Color.FromRgb(74, 143, 231));
        }

        private void today_reminders_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RemindersLine.Visibility = Visibility.Visible;
        }
        private void ProductIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProductForm l = new ProductForm();
            openform(l);
            RefreshPage();
        }

        private void CustomerFormIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CustomerForm l = new CustomerForm();
            openform(l);
            RefreshPage();
        }

        private void ReminderFormIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReminderForm l = new ReminderForm();
            openform(l);
            RefreshPage();
        }

        private void ActivitiesIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ActivitiesForm l = new ActivitiesForm();
            openform(l);
            RefreshPage();
        }

        private void UserIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UsersForm l = new UsersForm();
            openform(l);
            RefreshPage();
        }

        private void SMSIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Dbll.PanelIsActive())
            {
                SMSForm l = new SMSForm();
                openform(l);
                RefreshPage();
            }
            else
            {
                ReportForm l = new ReportForm();
                openform(l);
                RefreshPage();
            }
        }

        private void PaymentIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PaymentsForm list = new PaymentsForm();
            openform(list);
            RefreshPage();
        }

        private void ReportIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReportsWindow list = new ReportsWindow();
            openform(list);
            RefreshPage();
        }

        private void InvoiceIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InvoiceForm list = new InvoiceForm();
            openform(list);
            RefreshPage();
        }

        private void SettingIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Setting list = new Setting();
            openform(list);
            RefreshPage();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            LoginForm list = new LoginForm();
            openform(list);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            LoginForm list = new LoginForm();
            openform(list);
        }
        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            AboutUsForm l = new AboutUsForm();
            openform(l);
            RefreshPage();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C:
                    if (EnterC)
                    {
                        CustomerForm l = new CustomerForm();
                        openform(l);
                        RefreshPage();
                    }
                    break;
                case Key.P:
                    if (EnterP)
                    {
                        ProductForm l1 = new ProductForm();
                        openform(l1);
                        RefreshPage();
                    }
                    break;
                case Key.A:
                    if (EnterA)
                    {
                        ActivitiesForm A = new ActivitiesForm();
                        openform(A);
                        RefreshPage();
                    }
                    break;
                case Key.U:
                    if (EnterU)
                    {
                        UsersForm U = new UsersForm();
                        openform(U);
                        RefreshPage();
                    }
                    break;
                case Key.R:
                    if (EnterR)
                    {
                        ReminderForm R = new ReminderForm();
                        openform(R);
                        RefreshPage();
                    }
                    break;
                case Key.F:
                    if (EnterF)
                    {
                        InvoiceForm list = new InvoiceForm();
                        openform(list);
                        RefreshPage();
                    }
                    break;
                case Key.V:
                    if (EnterV)
                    {
                        PaymentsForm payments = new PaymentsForm();
                        openform(payments);
                        RefreshPage();
                    }
                    break;
                case Key.G:
                    if (EnterG)
                    {
                        ReportsWindow list1 = new ReportsWindow();
                        openform(list1);
                        RefreshPage();
                    }
                    break;
                case Key.M:
                    if (EnterM)
                    {
                        if (Dbll.PanelIsActive())
                        {
                            SMSForm l2 = new SMSForm();
                            openform(l2);
                            RefreshPage();
                        }
                        else
                        {
                            ReportForm l2 = new ReportForm();
                            openform(l2);
                            RefreshPage();
                        }
                    }
                    break;
                case Key.S:
                    if (EnterS)
                    {
                        Setting s = new Setting();
                        openform(s);
                        RefreshPage();
                    }
                    break;
                case Key.L:
                    this.Visibility = Visibility.Hidden;
                    LoginForm list4 = new LoginForm();
                    openform(list4);
                    break;
                case Key.E:
                    MessageBoxResult exit = MessageBox.Show("آیا میخواهید از برنامه خارج شوید ؟", "خروج", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (exit == MessageBoxResult.Yes)
                    {
                        App.Current.Shutdown();
                    }
                    break;
                    
            }
        }
    }
}
