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
using BE;
using BLL;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for LoginUC.xaml
    /// </summary>
    public partial class LoginUC : UserControl
    {
        public LoginUC()
        {
            InitializeComponent();
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            t.Clear();
            t.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }
        UserBLL bll = new UserBLL();
        User u = new User();
        RememberMeBLL RMbll = new RememberMeBLL();
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text != "" && txtUserName.Text != "نام کاربری" && txtPass.Password != "" && txtPass.Password != "رمز عبور")
            {
                u = bll.Login(txtUserName.Text, txtPass.Password);
                if (u != null)
                {
                    var loginForm = Window.GetWindow(this) as LoginForm;
                    if (IsCheckedImage.Visibility == Visibility.Visible)
                    {
                        RememberMe rememberMe = new RememberMe();
                        rememberMe.UserName = txtUserName.Text;
                        RMbll.Create(rememberMe);
                    }
                    else if (IsCheckedImage.Visibility == Visibility.Hidden)
                    {
                        RememberMe rememberMe = new RememberMe();
                        if (RMbll.Load() != null)
                        {
                            rememberMe.LastLoginTime = (RMbll.Load()).LastLoginTime;
                            rememberMe.IsRemembered = false;
                            RMbll.Create(rememberMe);
                        }
                    }

                    MainWindow w = (MainWindow)Application.Current.MainWindow;
                    if (w != null)
                    {
                        w.loggedInUser = u;
                        w.LoadPage();
                        w.Visibility = Visibility.Visible;
                    }
                    loginForm?.Close();
                    MessageBox.Show("به نرم افزار خوش آمدید", "خوش آمد گویی", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else MessageBox.Show("نام کاربری یا رمز عبور اشتباه است", "اخطار", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else MessageBox.Show("لطفا تمامی فیلد هارا پر کنید");
        }

        private void txtPass_GotFocus(object sender, RoutedEventArgs e)
        {
            txtPass.Clear();
            txtPass.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        private void IsChecked_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsCheckedImage.Visibility == Visibility.Visible)
            {
                IsCheckedImage.Visibility = Visibility.Hidden;
               
            }
            else if (IsCheckedImage.Visibility == Visibility.Hidden)
            {
                IsCheckedImage.Visibility = Visibility.Visible;
                
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RememberMe rememberMe = RMbll.Load();
            if (rememberMe != null)
            {
                if (rememberMe.IsRemembered)
                {
                    IsCheckedImage.Visibility = Visibility.Visible;
                    txtUserName.Text = rememberMe.UserName;
                    txtUserName.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    txtPass.TabIndex = 0;
                    btnLogin.TabIndex = 1;
                }
                else
                {
                    txtUserName.TabIndex = 0;
                    txtPass.TabIndex = 1;
                    btnLogin.TabIndex = 2;
                }
            }
        }
    }
}
