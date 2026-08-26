using BE;
using Section = BE.Section;
using BLL;
using Microsoft.Win32;
using Stimulsoft.Report.Dashboard;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
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

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for UsersForm.xaml
    /// </summary>
    public partial class UsersForm : Window
    {
        public UsersForm()
        {
            InitializeComponent();
        }
        OpenFileDialog ofd = new OpenFileDialog();
        Image pic = new Image();
        Image picEdit = new Image();
        Image picLoad = new Image();
        UserBLL bll = new UserBLL();
        User userEdit = new User();
        UserGroupBLL UGbll = new UserGroupBLL();
        UserGroup ugEdit = new UserGroup();
        UserGroup ugUser = new UserGroup();
        string username;
        User u = new User();
        bool Add;

        private static readonly Section[] AllSections = (Section[])Enum.GetValues(typeof(Section));
        private static readonly Operation[] AllOperations = (Operation[])Enum.GetValues(typeof(Operation));

        private readonly Dictionary<Section, Dictionary<Operation, CheckBox>> _cells =
            new Dictionary<Section, Dictionary<Operation, CheckBox>>();

        private static readonly Dictionary<Section, string> SectionCaptions = new Dictionary<Section, string>
        {
            { Section.Customers, "بخش مشتریان" },
            { Section.CatalogItems, "بخش کالاها" },
            { Section.Invoices, "بخش فاکتورها" },
            { Section.Activities, "بخش فعالیت ها" },
            { Section.Reminders, "بخش یادآور ها" },
            { Section.Users, "بخش کاربران" },
            { Section.SmsPanel, "پنل پیامکی" },
            { Section.Reports, "بخش گزارشات" },
            { Section.Settings, "بخش تنظیمات" },
            { Section.Discounts, "بخش تخفیف ها" },
        };

        private void BuildPermissionMatrix()
        {
            _cells.Clear();
            PermissionGrid.Children.Clear();
            PermissionGrid.RowDefinitions.Clear();

            PermissionGrid.RowDefinitions.Add(NewRow());
            for (int i = 0; i < AllSections.Length; i++)
                PermissionGrid.RowDefinitions.Add(NewRow());

            Place(new TextBlock { Text = "بخش", FontWeight = FontWeights.Bold }, 0, 0);

            foreach (var op in AllOperations)
            {
                var captured = op;
                var header = new CheckBox
                {
                    Content = OperationCaption(captured),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                header.Click += (s, e) => SetColumn(captured, header.IsChecked == true);
                Place(header, 0, Array.IndexOf(AllOperations, captured) + 1);
            }

            for (int r = 0; r < AllSections.Length; r++)
            {
                var section = AllSections[r];
                string caption;
                if (!SectionCaptions.TryGetValue(section, out caption))
                    caption = section.ToString();
                Place(new TextBlock { Text = caption }, r + 1, 0);

                var cells = new Dictionary<Operation, CheckBox>();
                foreach (var op in AllOperations)
                {
                    var cell = new CheckBox { HorizontalAlignment = HorizontalAlignment.Center };
                    cells[op] = cell;
                    Place(cell, r + 1, Array.IndexOf(AllOperations, op) + 1);
                }
                _cells[section] = cells;
            }

            cbAll.Click += (s, e) =>
            {
                foreach (var op in AllOperations)
                    SetColumn(op, cbAll.IsChecked == true);
            };
        }

        private static RowDefinition NewRow()
        {
            return new RowDefinition { Height = new GridLength(28) };
        }

        private void Place(FrameworkElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            PermissionGrid.Children.Add(element);
        }

        private void SetColumn(Operation operation, bool value)
        {
            foreach (var cells in _cells.Values)
                cells[operation].IsChecked = value;
        }

        private static string OperationCaption(Operation operation)
        {
            switch (operation)
            {
                case Operation.View: return "مشاهده";
                case Operation.Create: return "ایجاد";
                case Operation.Edit: return "ویرایش";
                case Operation.Delete: return "حذف";
                default: throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unexpected Operation value");
            }
        }

        private List<AccessGrant> CollectGrants()
        {
            var grants = new List<AccessGrant>();
            foreach (var section in AllSections)
                foreach (var op in AllOperations)
                    if (_cells[section][op].IsChecked == true)
                        grants.Add(new AccessGrant { Section = section, Operation = op });
            return grants;
        }

        private void ApplyGrants(IEnumerable<AccessGrant> grants)
        {
            var present = new HashSet<Tuple<Section, Operation>>(
                grants.Select(g => Tuple.Create(g.Section, g.Operation)));

            foreach (var section in AllSections)
                foreach (var op in AllOperations)
                    _cells[section][op].IsChecked = present.Contains(Tuple.Create(section, op));
        }

        string SavePic(string UserName)
        {
            //Run NewFolder Pic + Selectd Target => @\UserPisc\
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\UserPisc\";
            // If Status => true وجو داشت عکسی
            if (!Directory.Exists(path))
            {
                // If NotFolder is  Create NewFolder And Is Address Path
                Directory.CreateDirectory(path);
            }
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
        string SavePicUpdate(string UserName)
        {
            //Run NewFolder Pic + Selectd Target => @\UserPisc\
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\UserPisc\";
            // If Status => true وجو داشت عکسی
            if (!Directory.Exists(path))
            {
                // If NotFolder is  Create NewFolder And Is Address Path
                Directory.CreateDirectory(path);
            }
            string PicName = UserName + ".JPG";
            try
            {
                string PicPath = ofd.FileName;
                File.Copy(PicPath, path + PicName, true);

            }
            catch (Exception)
            {

            }
            return path + PicName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!AccessGuard.Can(u, Section.Users, Operation.Create))
            {
                btnAdd.IsEnabled = false;
                btnAddUserGroup.IsEnabled = false;
                Add = false;
            }
            else
            {
                btnAdd.IsEnabled = true;
                btnAddUserGroup.IsEnabled = true;
                Add = true;
            }
            if (!AccessGuard.Can(u, Section.Users, Operation.Edit))
            {
                miEdit.IsEnabled = false;
                miEditUserGroup.IsEnabled = false;
            }
            else
            {
                miEdit.IsEnabled = true;
                miEditUserGroup.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Users, Operation.Delete))
            {
                miDelete.IsEnabled = false;
            }
            else
            {
                miDelete.IsEnabled = true;
            }

            cbUserGroup.ItemsSource = UGbll.ReadTitles();
            PublicMethods.dgvFiller(dgvUsers, bll.Read());
            PublicMethods.dgvFiller(dgvUserGroups, UGbll.Read());
            PublicMethods.DGVAutoSizeColumnFill(dgvUsers);
            
            BuildPermissionMatrix();
        }


        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ofd.Filter = "JPG(*.JPG)|*.JPG";
            ofd.Title = "تصویر کاربر را انتخاب کنید";
            if (ofd.ShowDialog() == true)
            {
                pic.Source = new BitmapImage(new Uri(ofd.FileName));
                picBoxUser.Source = pic.Source;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            User u = new User();
            u.Name = txtName.Text;
            u.UserName = txtUsername.Text;
            if (txtName.Text != "" && txtUsername.Text != "" && ugUser != null && picBoxUser.Source.ToString() != SourceOfImage.Source.ToString())
            {

                string password = txtPass.Password;
                string repeat = txtReapetPass.Password;
                bool editing = userEdit != null;

                if (!editing || password.Length > 0)
                {
                    if (password.Length < 8 || password.Length > 32)
                    {
                        MessageBox.Show("رمز عبور باید بین ۸ تا ۳۲ کاراکتر باشد");
                        return;
                    }
                    if (password != repeat)
                    {
                        MessageBox.Show("تکرار رمز عبور با رمز عبور مطابقت ندارد");
                        return;
                    }
                }

                u.Password = password.Length > 0 ? password : null;

                if (btnAdd.Content.ToString() == "ثبت")
                {
                    u.Picture = SavePic(txtUsername.Text);
                    u.UserGroup = ugUser;
                    MessageBox.Show(bll.Create(u), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtName.Clear();
                    txtPass.Clear();
                    txtReapetPass.Clear();
                    txtUsername.Clear();
                    txtUserGroupName.Clear();
                    picBoxUser.Source = SourceOfImage.Source;
                    cbUserGroup.Text = "";
                    ugEdit = null;
                    PublicMethods.dgvFiller(dgvUsers, bll.Read());
                    txtName.Focus();
                }
                else
                {

                    u.Picture = SavePic(txtUsername.Text);
                    u.UserGroup = ugUser;
                    MessageBox.Show(bll.Update(u, userEdit.id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    picBoxUser.Source = SourceOfImage.Source;
                    txtUsername.IsEnabled = true;
                    txtName.Clear();
                    txtPass.Clear();
                    txtReapetPass.Clear();
                    txtUsername.Clear();
                    txtUserGroupName.Clear();
                    cbUserGroup.Text = "";
                    ugEdit = null;
                    btnAdd.Content = "ثبت";
                    btnAdd.IsEnabled = Add;
                    PublicMethods.dgvFiller(dgvUsers, bll.Read());
                    txtName.Focus();
                }

            }
            else
            {
                MessageBox.Show("لطفا تمامی فیلد هارا به درستی پر کنید و عکس خود را وارد کنید");
            }


        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                bll.Delete(userEdit.id);
                PublicMethods.dgvFiller(dgvUsers, bll.Read());
            }
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = userEdit.Name;
            txtUsername.Text = userEdit.UserName;
            cbUserGroup.Text = userEdit.UserGroup.Title;
            ugUser = userEdit.UserGroup;
            txtUsername.IsEnabled = false;
            btnAdd.Content = "ویرایش";
            btnAdd.IsEnabled = true;
        }

        private void dgvUsers_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvUsers, 1) != null)
            {
                dgvUsers.ContextMenu.IsEnabled = true;
                username = PublicMethods.ReadTheEntityCode(dgvUsers, 1);
                userEdit = bll.ReadByUserName(username);
            }
            else
            {
                dgvUsers.ContextMenu.IsEnabled = false;
            }

            ;
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            PublicMethods.FilterPersian(t);
        }

        private void txtPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtPass.Password.Length > 8 && txtPass.Password.Length < 32)
            {
                lblPassWarn.Content = "";
            }
            else lblPassWarn.Content = "رمز باید بیشتر از 8 کاراکتر و کمتر از 32 کاراکتر باشد";
        }

        private void btnAddUserGroup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserGroupName.Text))
            {
                MessageBox.Show("عنوان گروه را وارد کنید");
                return;
            }

            if (btnAddUserGroup.Content.ToString() == "ایجاد گروه کاربری")
            {
                UGbll.Create(new UserGroup { Title = txtUserGroupName.Text.Trim(), IsBuiltIn = false, AccessGrants = CollectGrants() });
                ClearMatrix();
                PublicMethods.dgvFiller(dgvUserGroups, UGbll.Read());
                txtUserGroupName.Focus();
            }
            else if (btnAddUserGroup.Content.ToString() == "ویرایش")
            {
                if (ugEdit.IsBuiltIn)
                {
                    MessageBox.Show("گروه پیش‌فرض قابل ویرایش نیست");
                    return;
                }
                UGbll.Update(ugEdit.id, ugEdit.Title, CollectGrants());
                ClearMatrix();
                btnAddUserGroup.Content = "ایجاد گروه کاربری";
                btnAddUserGroup.IsEnabled = Add;
                txtUserGroupName.Text = "";
                txtUserGroupName.IsEnabled = true;
                txtUserGroupName.Focus();
            }
            else MessageBox.Show("لطفا نام گروه کاربری را خالی نگذارید");
        }

        private void ClearMatrix()
        {
            foreach (var section in AllSections)
                foreach (var op in AllOperations)
                    _cells[section][op].IsChecked = false;
            cbAll.IsChecked = false;
        }

        private void miEditUserGroup_Click(object sender, RoutedEventArgs e)
        {
            if (ugEdit.IsBuiltIn)
            {
                MessageBox.Show("گروه پیش‌فرض قابل ویرایش نیست");
                return;
            }
            txtUserGroupName.Text = ugEdit.Title;
            txtUserGroupName.IsEnabled = false;
            ApplyGrants(ugEdit.AccessGrants);
            btnAddUserGroup.Content = "ویرایش";
            btnAddUserGroup.IsEnabled = true;
        }

        private void dgvUserGroups_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvUserGroups, 0) != null)
            {
                dgvUserGroups.ContextMenu.IsEnabled = true;
                string title = PublicMethods.ReadTheEntityCode(dgvUserGroups, 0);
                ugEdit = UGbll.ReadBySingelTitle(title);
            }
            else
            {
                dgvUserGroups.ContextMenu.IsEnabled = false;
            }

        }

        private void cbUserGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbUserGroup.SelectedItem != null)
            {
                ugUser = UGbll.ReadBySingelTitle(cbUserGroup.SelectedItem.ToString());
            }
        }

        private void miShow_Click(object sender, RoutedEventArgs e)
        {
            UserPictureForm pictureForm = new UserPictureForm();
            pictureForm.image.Source = new BitmapImage(new Uri(userEdit.Picture));
            pictureForm.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
            }
        }

        private void Info_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("حواستان باشد در صورت دادن اجازه ورود و ثبت برای تنظیمات و پنل پیامکی کاربر مورد نظر به تمامی این بخش ها دسترسی کامل دارد", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}