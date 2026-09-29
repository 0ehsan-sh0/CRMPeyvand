using BE;
using Section = BE.Section;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for RegisterUC.xaml
    /// </summary>
    public partial class RegisterUC : UserControl
    {
        public RegisterUC()
        {
            InitializeComponent();
        }
        OpenFileDialog ofd = new OpenFileDialog();
        UserGroupBLL UGbll = new UserGroupBLL();
        UserBLL uBLL = new UserBLL();

        UserGroup CreateAdminGroup()
        {
            var grants = Enum.GetValues(typeof(Section)).Cast<Section>()
                .SelectMany(s => Enum.GetValues(typeof(Operation)).Cast<Operation>()
                    .Select(o => new AccessGrant { Section = s, Operation = o }))
                .ToList();

            var admin = new UserGroup
            {
                Title = "مدیریت",           // display text only — identity is IsBuiltIn
                IsBuiltIn = true,
                AccessGrants = grants,
            };

            UGbll.Create(admin);

            return UGbll.AdminUserGroup();
        }

        string SavePic(string UserName)
        {
            //Run NewFolder Pic + Selectd Target => %ProgramData%\CRMPeyvand\UserPisc\
            string path = PublicMethods.UserPicturesDirectory;
            string PicName = UserName + ".JPG";
            try
            {
                string PicPath = ofd.FileName;
                File.Copy(PicPath, path + PicName, true);

            }
            catch (Exception e)
            {
                MessageBox.Show("سیستم قادر به ذخیره عکس نمی باشد \n" + e.Message);
            }
            return path + PicName;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            t.Clear();
            t.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            PublicMethods.FilterPersian(textBox);
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text != "" && txtUserName.Text != "" && txtName.Text != "نام و نام خانوادگی" && txtUserName.Text != "نام کاربری" && picBoxUser.Source.ToString() != SourceOfImage.Source.ToString())
            {
                if (txtPass.Password.Length > 8 && txtPass.Password.Length < 32)
                {

                    if (txtPass.Password == txtRepeatPass.Text)
                    {
                        User u = new User();
                        u.UserGroup = CreateAdminGroup();
                        u.UserName = txtUserName.Text;
                        u.Name = txtName.Text;
                        u.Password = txtPass.Password;
                        u.RegDate = DateTime.Now;
                        u.Picture = SavePic(txtUserName.Text);
                        string result = uBLL.Create(u);
                        MessageBox.Show(result, "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (result == "ثبت اطلاعات با موفقیت انجام شد")
                        {
                            this.Visibility = Visibility.Hidden;
                            LoginUC luc = new LoginUC();
                            Grid.SetRow(luc, 1);
                            Grid.SetColumn(luc, 0);
                            Grid.SetColumnSpan(luc, 12);
                            Grid.SetRowSpan(luc, 12);
                            luc.HorizontalAlignment = HorizontalAlignment.Center;
                            luc.VerticalAlignment = VerticalAlignment.Center;
                            ((Panel)this.Parent).Children.Add(luc);
                        }
                    }
                    else
                    {
                        MessageBox.Show("کلمه عبور با تکرار آن همخوانی ندارد", "اخطار", MessageBoxButton.OK, MessageBoxImage.Error);
                        txtPass.Clear();
                        txtRepeatPass.Clear();
                    }

                    //((LoginForm)Application.Current.Windows[index]).Show();
                }
                else MessageBox.Show("رمز باید بیشتر از هشت کاراکتر و کمتر از 32 کاراکتر باشد");
            }
            else
            {
                MessageBox.Show("لطفا تمامی فیلد هارا به درستی پر کنید و عکس خود را وارد کنید");
            }
        }

        private void picBoxUser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ofd.Filter = "JPG(*.JPG)|*.JPG";
            ofd.Title = "تصویر کاربر را انتخاب کنید";
            if (ofd.ShowDialog() == true)
            {
                Image pic = new Image();
                pic.Source = new BitmapImage(new Uri(ofd.FileName));
                picBoxUser.Source = pic.Source;
            }

        }

        private void txtPass_GotFocus(object sender, RoutedEventArgs e)
        {
            txtPass.Clear();
            txtPass.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }
    }
}
