using BLL;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public LoginForm()
        {
            InitializeComponent();
        }
        UserBLL userBLL = new UserBLL();
        bool _HasAnyUser;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //t1.Enabled = true;
            //t1.Interval = 15;
            //t1.Elapsed += Timer_Tike;
            //t1.Start();
            lblLoad.Visibility = Visibility.Visible;
        }

        private void Timer_Tike(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }
        int i;
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            for (i = 0; i <= 100; i++)
            {
                if (i == 40)
                {
                    try
                    {
                        _HasAnyUser = userBLL.HasAnyUser();
                    }
                    catch (Exception ex)
                    {
                        _HasAnyUser = false;
                    }
                }
                worker?.ReportProgress(i);
                Thread.Sleep(15);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbLoad.Value = e.ProgressPercentage;
            if (e.ProgressPercentage >= 100)
            {
                pbLoad.Visibility = Visibility.Hidden;
                lblLoad.Visibility = Visibility.Hidden;
                imgLoad.Visibility = Visibility.Hidden;
                if (_HasAnyUser)
                {
                    LoginUC Luc = new LoginUC();
                    Grid.SetRow(Luc, 1);
                    Grid.SetColumn(Luc, 0);
                    Grid.SetColumnSpan(Luc, 12);
                    Grid.SetRowSpan(Luc, 12);
                    Luc.HorizontalAlignment = HorizontalAlignment.Center;
                    Luc.VerticalAlignment = VerticalAlignment.Center;
                    MainGrid.Children.Add(Luc);
                }
                else
                {
                    RegisterUC Luc = new RegisterUC();
                    Grid.SetRow(Luc, 1);
                    Grid.SetColumn(Luc, 0);
                    Grid.SetColumnSpan(Luc, 12);
                    Grid.SetRowSpan(Luc, 12);
                    Luc.HorizontalAlignment = HorizontalAlignment.Center;
                    Luc.VerticalAlignment = VerticalAlignment.Center;
                    MainGrid.Children.Add(Luc);
                }
            }
        }

        private void Exit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult exit = MessageBox.Show("آیا میخواهید از برنامه خارج شوید ؟", "خروج", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (exit == MessageBoxResult.Yes)
            {
                App.Current.Shutdown();
            }
        }
    }
}
