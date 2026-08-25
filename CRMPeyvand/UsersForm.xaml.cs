using BE;
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
        UserAccessRole FillAccessRole(string Section, Image CanEnter, Image CanCreate, Image CanUpdate, Image CanDelete)
        {
            UserAccessRole role = new UserAccessRole();
            role.Section = Section;
            if (CanEnter.Visibility == Visibility.Visible)
            {
                role.CanEnter = true;
            }
            else if (CanEnter.Visibility == Visibility.Hidden)
            {
                role.CanEnter = false;
            }

            if (CanCreate.Visibility == Visibility.Visible)
            {
                role.CanCreate = true;
            }
            else if (CanCreate.Visibility == Visibility.Hidden)
            {
                role.CanCreate = false;
            }

            if (CanUpdate.Visibility == Visibility.Visible)
            {
                role.CanUpdate = true;
            }
            else if (CanUpdate.Visibility == Visibility.Hidden)
            {
                role.CanUpdate = false;
            }

            if (CanDelete.Visibility == Visibility.Visible)
            {
                role.CanDelete = true;
            }
            else if (CanDelete.Visibility == Visibility.Hidden)
            {
                role.CanDelete = false;
            }
            return role;

        }
        UserAccessRole FillAccessRoleForUpdate(UserAccessRole uar, Image CanEnter, Image CanCreate, Image CanUpdate, Image CanDelete)
        {
            if (CanEnter.Visibility == Visibility.Visible)
            {
                uar.CanEnter = true;
            }
            else if (CanEnter.Visibility == Visibility.Hidden)
            {
                uar.CanEnter = false;
            }

            if (CanCreate.Visibility == Visibility.Visible)
            {
                uar.CanCreate = true;
            }
            else if (CanCreate.Visibility == Visibility.Hidden)
            {
                uar.CanCreate = false;
            }

            if (CanUpdate.Visibility == Visibility.Visible)
            {
                uar.CanUpdate = true;
            }
            else if (CanUpdate.Visibility == Visibility.Hidden)
            {
                uar.CanUpdate = false;
            }

            if (CanDelete.Visibility == Visibility.Visible)
            {
                uar.CanDelete = true;
            }
            else if (CanDelete.Visibility == Visibility.Hidden)
            {
                uar.CanDelete = false;
            }
            return uar;
        }

        void FillForUpdate(Image CanEnter, Image CanCreate, Image CanUpdate, Image CanDelete, bool CanEnterB, bool CanCreateB, bool CanUpdateB, bool CanDeleteB)
        {
            if (CanEnterB)
            {
                CanEnter.Visibility = Visibility.Visible;
            }
            else
            {
                CanEnter.Visibility = Visibility.Hidden;
            }

            if (CanCreateB)
            {
                CanCreate.Visibility = Visibility.Visible;
            }
            else
            {
                CanCreate.Visibility = Visibility.Hidden;
            }

            if (CanUpdateB)
            {
                CanUpdate.Visibility = Visibility.Visible;
            }
            else
            {
                CanUpdate.Visibility = Visibility.Hidden;
            }

            if (CanDeleteB)
            {
                CanDelete.Visibility = Visibility.Visible;
            }
            else
            {
                CanDelete.Visibility = Visibility.Hidden;
            }
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

        void ClearCB()
        {
            cbAllCheckBoxesImage.Visibility = Visibility.Hidden;

            cbDeleteImage.Visibility = Visibility.Hidden;
            cbDeleteActivityImage.Visibility = Visibility.Hidden;
            cbDeleteCustomerImage.Visibility = Visibility.Hidden;
            cbDeleteInvoiceImage.Visibility = Visibility.Hidden;
            cbDeleteMessageImage.Visibility = Visibility.Hidden;
            cbDeleteProductImage.Visibility = Visibility.Hidden;
            cbDeleteReminderImage.Visibility = Visibility.Hidden;
            cbDeleteReportImage.Visibility = Visibility.Hidden;
            cbDeleteSettingImage.Visibility = Visibility.Hidden;
            cbDeleteUserImage.Visibility = Visibility.Hidden;

            cbEditImage.Visibility = Visibility.Hidden;
            cbEditActivityImage.Visibility = Visibility.Hidden;
            cbEditCustomerImage.Visibility = Visibility.Hidden;
            cbEditInvoiceImage.Visibility = Visibility.Hidden;
            cbEditMessageImage.Visibility = Visibility.Hidden;
            cbEditProductImage.Visibility = Visibility.Hidden;
            cbEditReminderImage.Visibility = Visibility.Hidden;
            cbEditReportImage.Visibility = Visibility.Hidden;
            cbEditSettingImage.Visibility = Visibility.Hidden;
            cbEditUserImage.Visibility = Visibility.Hidden;

            cbAddImage.Visibility = Visibility.Hidden;
            cbAddActivityImage.Visibility = Visibility.Hidden;
            cbAddCustomerImage.Visibility = Visibility.Hidden;
            cbAddInvoiceImage.Visibility = Visibility.Hidden;
            cbAddMessageImage.Visibility = Visibility.Hidden;
            cbAddProductImage.Visibility = Visibility.Hidden;
            cbAddReminderImage.Visibility = Visibility.Hidden;
            cbAddReportImage.Visibility = Visibility.Hidden;
            cbAddSettingImage.Visibility = Visibility.Hidden;
            cbAddUserImage.Visibility = Visibility.Hidden;

            cbEnterImage.Visibility = Visibility.Hidden;
            cbEnterActivityImage.Visibility = Visibility.Hidden;
            cbEnterCustomerImage.Visibility = Visibility.Hidden;
            cbEnterInvoiceImage.Visibility = Visibility.Hidden;
            cbEnterMessageImage.Visibility = Visibility.Hidden;
            cbEnterProductImage.Visibility = Visibility.Hidden;
            cbEnterReminderImage.Visibility = Visibility.Hidden;
            cbEnterReportImage.Visibility = Visibility.Hidden;
            cbEnterSettingImage.Visibility = Visibility.Hidden;
            cbEnterUserImage.Visibility = Visibility.Hidden;

        }
        void ChangeVisibility(Image image)
        {
            if (image.Visibility == Visibility.Hidden)
            {
                image.Visibility = Visibility.Visible;
            }
            else if (image.Visibility == Visibility.Visible)
            {
                image.Visibility = Visibility.Hidden;
            }
        }
        void ChangeVisibilityToVisible(Image image)
        {
            if (image.Visibility == Visibility.Hidden)
            {
                image.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!bll.Access(u, "بخش کاربران", 2))
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
            if (!bll.Access(u, "بخش کاربران", 3))
            {
                miEdit.IsEnabled = false;
                miEditUserGroup.IsEnabled = false;
            }
            else
            {
                miEdit.IsEnabled = true;
                miEditUserGroup.IsEnabled = true;
            }
            if (!bll.Access(u, "بخش کاربران", 4))
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
        }


        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        //cbEvents <<<<<
        #region AllCheckBoxes
        private void cbEnter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (cbEnterImage.Visibility == Visibility.Hidden)
            {
                cbEnterImage.Visibility = Visibility.Visible;
                cbEnterActivityImage.Visibility = Visibility.Visible;
                cbEnterCustomerImage.Visibility = Visibility.Visible;
                cbEnterInvoiceImage.Visibility = Visibility.Visible;
                cbEnterMessageImage.Visibility = Visibility.Visible;
                cbEnterProductImage.Visibility = Visibility.Visible;
                cbEnterReminderImage.Visibility = Visibility.Visible;
                cbEnterReportImage.Visibility = Visibility.Visible;
                cbEnterSettingImage.Visibility = Visibility.Visible;
                cbEnterUserImage.Visibility = Visibility.Visible;

            }
            else if (cbEnterImage.Visibility == Visibility.Visible)
            {
                cbEnterImage.Visibility = Visibility.Hidden;
                cbEnterActivityImage.Visibility = Visibility.Hidden;
                cbEnterCustomerImage.Visibility = Visibility.Hidden;
                cbEnterInvoiceImage.Visibility = Visibility.Hidden;
                cbEnterMessageImage.Visibility = Visibility.Hidden;
                cbEnterProductImage.Visibility = Visibility.Hidden;
                cbEnterReminderImage.Visibility = Visibility.Hidden;
                cbEnterReportImage.Visibility = Visibility.Hidden;
                cbEnterSettingImage.Visibility = Visibility.Hidden;
                cbEnterUserImage.Visibility = Visibility.Hidden;

                cbAddImage.Visibility = Visibility.Hidden;
                cbAddActivityImage.Visibility = Visibility.Hidden;
                cbAddCustomerImage.Visibility = Visibility.Hidden;
                cbAddInvoiceImage.Visibility = Visibility.Hidden;
                cbAddMessageImage.Visibility = Visibility.Hidden;
                cbAddProductImage.Visibility = Visibility.Hidden;
                cbAddReminderImage.Visibility = Visibility.Hidden;
                cbAddReportImage.Visibility = Visibility.Hidden;
                cbAddSettingImage.Visibility = Visibility.Hidden;
                cbAddUserImage.Visibility = Visibility.Hidden;

                cbEditImage.Visibility = Visibility.Hidden;
                cbEditActivityImage.Visibility = Visibility.Hidden;
                cbEditCustomerImage.Visibility = Visibility.Hidden;
                cbEditInvoiceImage.Visibility = Visibility.Hidden;
                cbEditMessageImage.Visibility = Visibility.Hidden;
                cbEditProductImage.Visibility = Visibility.Hidden;
                cbEditReminderImage.Visibility = Visibility.Hidden;
                cbEditReportImage.Visibility = Visibility.Hidden;
                cbEditSettingImage.Visibility = Visibility.Hidden;
                cbEditUserImage.Visibility = Visibility.Hidden;

                cbDeleteImage.Visibility = Visibility.Hidden;
                cbDeleteActivityImage.Visibility = Visibility.Hidden;
                cbDeleteCustomerImage.Visibility = Visibility.Hidden;
                cbDeleteInvoiceImage.Visibility = Visibility.Hidden;
                cbDeleteMessageImage.Visibility = Visibility.Hidden;
                cbDeleteProductImage.Visibility = Visibility.Hidden;
                cbDeleteReminderImage.Visibility = Visibility.Hidden;
                cbDeleteReportImage.Visibility = Visibility.Hidden;
                cbDeleteSettingImage.Visibility = Visibility.Hidden;
                cbDeleteUserImage.Visibility = Visibility.Hidden;

                cbAllCheckBoxesImage.Visibility = Visibility.Hidden;
            }

        }

        private void cbAdd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (cbAddImage.Visibility == Visibility.Hidden)
            {
                cbAddImage.Visibility = Visibility.Visible;
                cbAddActivityImage.Visibility = Visibility.Visible;
                cbAddCustomerImage.Visibility = Visibility.Visible;
                cbAddInvoiceImage.Visibility = Visibility.Visible;
                cbAddMessageImage.Visibility = Visibility.Visible;
                cbAddProductImage.Visibility = Visibility.Visible;
                cbAddReminderImage.Visibility = Visibility.Visible;
                cbAddReportImage.Visibility = Visibility.Visible;
                cbAddSettingImage.Visibility = Visibility.Visible;
                cbAddUserImage.Visibility = Visibility.Visible;

                cbEnterImage.Visibility = Visibility.Visible;
                cbEnterActivityImage.Visibility = Visibility.Visible;
                cbEnterCustomerImage.Visibility = Visibility.Visible;
                cbEnterInvoiceImage.Visibility = Visibility.Visible;
                cbEnterMessageImage.Visibility = Visibility.Visible;
                cbEnterProductImage.Visibility = Visibility.Visible;
                cbEnterReminderImage.Visibility = Visibility.Visible;
                cbEnterReportImage.Visibility = Visibility.Visible;
                cbEnterSettingImage.Visibility = Visibility.Visible;
                cbEnterUserImage.Visibility = Visibility.Visible;


            }
            else if (cbAddImage.Visibility == Visibility.Visible)
            {
                cbAddImage.Visibility = Visibility.Hidden;
                cbAddActivityImage.Visibility = Visibility.Hidden;
                cbAddCustomerImage.Visibility = Visibility.Hidden;
                cbAddInvoiceImage.Visibility = Visibility.Hidden;
                cbAddMessageImage.Visibility = Visibility.Hidden;
                cbAddProductImage.Visibility = Visibility.Hidden;
                cbAddReminderImage.Visibility = Visibility.Hidden;
                cbAddReportImage.Visibility = Visibility.Hidden;
                cbAddSettingImage.Visibility = Visibility.Hidden;
                cbAddUserImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbEdit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (cbEditImage.Visibility == Visibility.Hidden)
            {
                cbEditImage.Visibility = Visibility.Visible;
                cbEditActivityImage.Visibility = Visibility.Visible;
                cbEditCustomerImage.Visibility = Visibility.Visible;
                cbEditInvoiceImage.Visibility = Visibility.Visible;
                cbEditMessageImage.Visibility = Visibility.Visible;
                cbEditProductImage.Visibility = Visibility.Visible;
                cbEditReminderImage.Visibility = Visibility.Visible;
                cbEditReportImage.Visibility = Visibility.Visible;
                cbEditSettingImage.Visibility = Visibility.Visible;
                cbEditUserImage.Visibility = Visibility.Visible;

                cbEnterImage.Visibility = Visibility.Visible;
                cbEnterActivityImage.Visibility = Visibility.Visible;
                cbEnterCustomerImage.Visibility = Visibility.Visible;
                cbEnterInvoiceImage.Visibility = Visibility.Visible;
                cbEnterMessageImage.Visibility = Visibility.Visible;
                cbEnterProductImage.Visibility = Visibility.Visible;
                cbEnterReminderImage.Visibility = Visibility.Visible;
                cbEnterReportImage.Visibility = Visibility.Visible;
                cbEnterSettingImage.Visibility = Visibility.Visible;
                cbEnterUserImage.Visibility = Visibility.Visible;


            }
            else if (cbEditImage.Visibility == Visibility.Visible)
            {
                cbEditImage.Visibility = Visibility.Hidden;
                cbEditActivityImage.Visibility = Visibility.Hidden;
                cbEditCustomerImage.Visibility = Visibility.Hidden;
                cbEditInvoiceImage.Visibility = Visibility.Hidden;
                cbEditMessageImage.Visibility = Visibility.Hidden;
                cbEditProductImage.Visibility = Visibility.Hidden;
                cbEditReminderImage.Visibility = Visibility.Hidden;
                cbEditReportImage.Visibility = Visibility.Hidden;
                cbEditSettingImage.Visibility = Visibility.Hidden;
                cbEditUserImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbDelete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (cbDeleteImage.Visibility == Visibility.Hidden)
            {
                cbDeleteImage.Visibility = Visibility.Visible;
                cbDeleteActivityImage.Visibility = Visibility.Visible;
                cbDeleteCustomerImage.Visibility = Visibility.Visible;
                cbDeleteInvoiceImage.Visibility = Visibility.Visible;
                cbDeleteMessageImage.Visibility = Visibility.Visible;
                cbDeleteProductImage.Visibility = Visibility.Visible;
                cbDeleteReminderImage.Visibility = Visibility.Visible;
                cbDeleteReportImage.Visibility = Visibility.Visible;
                cbDeleteSettingImage.Visibility = Visibility.Visible;
                cbDeleteUserImage.Visibility = Visibility.Visible;

                cbEnterImage.Visibility = Visibility.Visible;
                cbEnterActivityImage.Visibility = Visibility.Visible;
                cbEnterCustomerImage.Visibility = Visibility.Visible;
                cbEnterInvoiceImage.Visibility = Visibility.Visible;
                cbEnterMessageImage.Visibility = Visibility.Visible;
                cbEnterProductImage.Visibility = Visibility.Visible;
                cbEnterReminderImage.Visibility = Visibility.Visible;
                cbEnterReportImage.Visibility = Visibility.Visible;
                cbEnterSettingImage.Visibility = Visibility.Visible;
                cbEnterUserImage.Visibility = Visibility.Visible;


            }
            else if (cbDeleteImage.Visibility == Visibility.Visible)
            {
                cbDeleteImage.Visibility = Visibility.Hidden;
                cbDeleteActivityImage.Visibility = Visibility.Hidden;
                cbDeleteCustomerImage.Visibility = Visibility.Hidden;
                cbDeleteInvoiceImage.Visibility = Visibility.Hidden;
                cbDeleteMessageImage.Visibility = Visibility.Hidden;
                cbDeleteProductImage.Visibility = Visibility.Hidden;
                cbDeleteReminderImage.Visibility = Visibility.Hidden;
                cbDeleteReportImage.Visibility = Visibility.Hidden;
                cbDeleteSettingImage.Visibility = Visibility.Hidden;
                cbDeleteUserImage.Visibility = Visibility.Hidden;

            }


        }

        private void cbAllCheckBoxes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (cbAllCheckBoxesImage.Visibility == Visibility.Hidden)
            {
                cbAllCheckBoxesImage.Visibility = Visibility.Visible;

                cbDeleteImage.Visibility = Visibility.Visible;
                cbDeleteActivityImage.Visibility = Visibility.Visible;
                cbDeleteCustomerImage.Visibility = Visibility.Visible;
                cbDeleteInvoiceImage.Visibility = Visibility.Visible;
                cbDeleteMessageImage.Visibility = Visibility.Visible;
                cbDeleteProductImage.Visibility = Visibility.Visible;
                cbDeleteReminderImage.Visibility = Visibility.Visible;
                cbDeleteReportImage.Visibility = Visibility.Visible;
                cbDeleteSettingImage.Visibility = Visibility.Visible;
                cbDeleteUserImage.Visibility = Visibility.Visible;

                cbEditImage.Visibility = Visibility.Visible;
                cbEditActivityImage.Visibility = Visibility.Visible;
                cbEditCustomerImage.Visibility = Visibility.Visible;
                cbEditInvoiceImage.Visibility = Visibility.Visible;
                cbEditMessageImage.Visibility = Visibility.Visible;
                cbEditProductImage.Visibility = Visibility.Visible;
                cbEditReminderImage.Visibility = Visibility.Visible;
                cbEditReportImage.Visibility = Visibility.Visible;
                cbEditSettingImage.Visibility = Visibility.Visible;
                cbEditUserImage.Visibility = Visibility.Visible;

                cbAddImage.Visibility = Visibility.Visible;
                cbAddActivityImage.Visibility = Visibility.Visible;
                cbAddCustomerImage.Visibility = Visibility.Visible;
                cbAddInvoiceImage.Visibility = Visibility.Visible;
                cbAddMessageImage.Visibility = Visibility.Visible;
                cbAddProductImage.Visibility = Visibility.Visible;
                cbAddReminderImage.Visibility = Visibility.Visible;
                cbAddReportImage.Visibility = Visibility.Visible;
                cbAddSettingImage.Visibility = Visibility.Visible;
                cbAddUserImage.Visibility = Visibility.Visible;

                cbEnterImage.Visibility = Visibility.Visible;
                cbEnterActivityImage.Visibility = Visibility.Visible;
                cbEnterCustomerImage.Visibility = Visibility.Visible;
                cbEnterInvoiceImage.Visibility = Visibility.Visible;
                cbEnterMessageImage.Visibility = Visibility.Visible;
                cbEnterProductImage.Visibility = Visibility.Visible;
                cbEnterReminderImage.Visibility = Visibility.Visible;
                cbEnterReportImage.Visibility = Visibility.Visible;
                cbEnterSettingImage.Visibility = Visibility.Visible;
                cbEnterUserImage.Visibility = Visibility.Visible;



            }
            else if (cbAllCheckBoxesImage.Visibility == Visibility.Visible)
            {
                cbAllCheckBoxesImage.Visibility = Visibility.Hidden;

                cbDeleteImage.Visibility = Visibility.Hidden;
                cbDeleteActivityImage.Visibility = Visibility.Hidden;
                cbDeleteCustomerImage.Visibility = Visibility.Hidden;
                cbDeleteInvoiceImage.Visibility = Visibility.Hidden;
                cbDeleteMessageImage.Visibility = Visibility.Hidden;
                cbDeleteProductImage.Visibility = Visibility.Hidden;
                cbDeleteReminderImage.Visibility = Visibility.Hidden;
                cbDeleteReportImage.Visibility = Visibility.Hidden;
                cbDeleteSettingImage.Visibility = Visibility.Hidden;
                cbDeleteUserImage.Visibility = Visibility.Hidden;

                cbEditImage.Visibility = Visibility.Hidden;
                cbEditActivityImage.Visibility = Visibility.Hidden;
                cbEditCustomerImage.Visibility = Visibility.Hidden;
                cbEditInvoiceImage.Visibility = Visibility.Hidden;
                cbEditMessageImage.Visibility = Visibility.Hidden;
                cbEditProductImage.Visibility = Visibility.Hidden;
                cbEditReminderImage.Visibility = Visibility.Hidden;
                cbEditReportImage.Visibility = Visibility.Hidden;
                cbEditSettingImage.Visibility = Visibility.Hidden;
                cbEditUserImage.Visibility = Visibility.Hidden;

                cbAddImage.Visibility = Visibility.Hidden;
                cbAddActivityImage.Visibility = Visibility.Hidden;
                cbAddCustomerImage.Visibility = Visibility.Hidden;
                cbAddInvoiceImage.Visibility = Visibility.Hidden;
                cbAddMessageImage.Visibility = Visibility.Hidden;
                cbAddProductImage.Visibility = Visibility.Hidden;
                cbAddReminderImage.Visibility = Visibility.Hidden;
                cbAddReportImage.Visibility = Visibility.Hidden;
                cbAddSettingImage.Visibility = Visibility.Hidden;
                cbAddUserImage.Visibility = Visibility.Hidden;

                cbEnterImage.Visibility = Visibility.Hidden;
                cbEnterActivityImage.Visibility = Visibility.Hidden;
                cbEnterCustomerImage.Visibility = Visibility.Hidden;
                cbEnterInvoiceImage.Visibility = Visibility.Hidden;
                cbEnterMessageImage.Visibility = Visibility.Hidden;
                cbEnterProductImage.Visibility = Visibility.Hidden;
                cbEnterReminderImage.Visibility = Visibility.Hidden;
                cbEnterReportImage.Visibility = Visibility.Hidden;
                cbEnterSettingImage.Visibility = Visibility.Hidden;
                cbEnterUserImage.Visibility = Visibility.Hidden;


            }

        }

        #endregion
        #region EnterCheckboxes
        private void cbEnterCustomer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterCustomerImage);
            if (cbEnterCustomerImage.Visibility == Visibility.Hidden)
            {
                cbAddCustomerImage.Visibility = Visibility.Hidden;
                cbEditCustomerImage.Visibility = Visibility.Hidden;
                cbDeleteCustomerImage.Visibility = Visibility.Hidden;

            }
        }

        private void cbEnterProduct_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterProductImage);
            if (cbEnterProductImage.Visibility == Visibility.Hidden)
            {
                cbAddProductImage.Visibility = Visibility.Hidden;
                cbEditProductImage.Visibility = Visibility.Hidden;
                cbDeleteProductImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbEnterInvoice_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterInvoiceImage);
            if (cbEnterInvoiceImage.Visibility == Visibility.Hidden)
            {
                cbAddInvoiceImage.Visibility = Visibility.Hidden;
                cbEditInvoiceImage.Visibility = Visibility.Hidden;
                cbDeleteInvoiceImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbEnterActivity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterActivityImage);
            if (cbEnterActivityImage.Visibility == Visibility.Hidden)
            {
                cbAddActivityImage.Visibility = Visibility.Hidden;
                cbEditActivityImage.Visibility = Visibility.Hidden;
                cbDeleteActivityImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbEnterReminder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterReminderImage);
            if (cbEnterReminderImage.Visibility == Visibility.Hidden)
            {
                cbAddReminderImage.Visibility = Visibility.Hidden;
                cbEditReminderImage.Visibility = Visibility.Hidden;
                cbDeleteReminderImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbEnterUser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterUserImage);
            if (cbEnterUserImage.Visibility == Visibility.Hidden)
            {
                cbAddUserImage.Visibility = Visibility.Hidden;
                cbEditUserImage.Visibility = Visibility.Hidden;
                cbDeleteUserImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbEnterMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterMessageImage);
            if (cbEnterMessageImage.Visibility == Visibility.Hidden)
            {
                cbAddMessageImage.Visibility = Visibility.Hidden;
                cbEditMessageImage.Visibility = Visibility.Hidden;
                cbDeleteProductImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbEnterReport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterReportImage);
            if (cbEnterReportImage.Visibility == Visibility.Hidden)
            {
                cbAddReportImage.Visibility = Visibility.Hidden;
                cbEditReportImage.Visibility = Visibility.Hidden;
                cbDeleteReportImage.Visibility = Visibility.Hidden;

            }

        }

        private void cbEnterSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEnterSettingImage);
            if (cbEnterSettingImage.Visibility == Visibility.Hidden)
            {
                cbAddSettingImage.Visibility = Visibility.Hidden;
                cbEditSettingImage.Visibility = Visibility.Hidden;
                cbDeleteSettingImage.Visibility = Visibility.Hidden;

            }

        }

        #endregion
        #region AddCheckboxes
        private void cbAddCustomer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddCustomerImage);
            ChangeVisibilityToVisible(cbEnterCustomerImage);
        }

        private void cbAddProduct_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddProductImage);
            ChangeVisibilityToVisible(cbEnterProductImage);
        }

        private void cbAddInvoice_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddInvoiceImage);
            ChangeVisibilityToVisible(cbEnterInvoiceImage);

        }

        private void cbAddActivity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddActivityImage);
            ChangeVisibilityToVisible(cbEnterActivityImage);

        }

        private void cbAddReminder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddReminderImage);
            ChangeVisibilityToVisible(cbEnterReminderImage);

        }

        private void cbAddUser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddUserImage);
            ChangeVisibilityToVisible(cbEnterUserImage);

        }

        private void cbAddMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddMessageImage);
            ChangeVisibilityToVisible(cbEnterMessageImage);


        }

        private void cbAddReport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddReportImage);
            ChangeVisibilityToVisible(cbEnterReportImage);

        }

        private void cbAddSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbAddSettingImage);
            ChangeVisibilityToVisible(cbEnterSettingImage);

        }

        #endregion
        #region EditCheckBoxes

        private void cbEditCustomer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditCustomerImage);
            ChangeVisibilityToVisible(cbEnterCustomerImage);
        }

        private void cbEditProduct_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditProductImage);
            ChangeVisibilityToVisible(cbEnterProductImage);

        }

        private void cbEditInvoice_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditInvoiceImage);
            ChangeVisibilityToVisible(cbEnterInvoiceImage);

        }

        private void cbEditActivity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditActivityImage);
            ChangeVisibilityToVisible(cbEnterActivityImage);

        }

        private void cbEditReminder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditReminderImage);
            ChangeVisibilityToVisible(cbEnterReminderImage);

        }

        private void cbEditUser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditUserImage);
            ChangeVisibilityToVisible(cbEnterUserImage);

        }

        private void cbEditMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditMessageImage);
            ChangeVisibilityToVisible(cbEnterMessageImage);
        }

        private void cbEditReport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditReportImage);
            ChangeVisibilityToVisible(cbEnterReportImage);
        }

        private void cbEditSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbEditSettingImage);
            ChangeVisibilityToVisible(cbEnterSettingImage);
        }


        #endregion
        #region DeleteCheckBoxes
        private void cbDeleteCustomer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteCustomerImage);
            ChangeVisibilityToVisible(cbEnterCustomerImage);
        }

        private void cbDeleteProduct_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteProductImage);
            ChangeVisibilityToVisible(cbEnterProductImage);

        }

        private void cbDeleteInvoice_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteInvoiceImage);
            ChangeVisibilityToVisible(cbEnterInvoiceImage);

        }

        private void cbDeleteActivity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteActivityImage);
            ChangeVisibilityToVisible(cbEnterActivityImage);

        }

        private void cbDeleteReminder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteReminderImage);
            ChangeVisibilityToVisible(cbEnterReminderImage);

        }

        private void cbDeleteUser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteUserImage);
            ChangeVisibilityToVisible(cbEnterUserImage);

        }

        private void cbDeleteMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteMessageImage);
            ChangeVisibilityToVisible(cbEnterMessageImage);
        }

        private void cbDeleteReport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteReportImage);
            ChangeVisibilityToVisible(cbEnterReportImage);


        }

        private void cbDeleteSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeVisibility(cbDeleteSettingImage);
            ChangeVisibilityToVisible(cbEnterSettingImage);

        }



        #endregion
        //cbEvents >>>>>
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

                u.Password = txtReapetPass.Text;
                u.RegDate = DateTime.Now;
                if (txtPass.Text.Length > 8 && txtPass.Text.Length < 32 && txtReapetPass.Text != "")
                {
                    if (txtPass.Text == txtReapetPass.Text)
                    {

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
                        MessageBox.Show("کلمه عبور با تکرار وارد شده آن مطابقت ندارد");
                    }

                }

                else
                {
                    if (btnAdd.Content.ToString() == "ویرایش")
                    {

                        u.UserGroup = ugUser;
                        u.Picture = SavePicUpdate(txtUsername.Text);
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

        private void txtPass_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPass.Text.Length > 8 && txtPass.Text.Length < 32)
            {
                lblPassWarn.Content = "";
            }
            else lblPassWarn.Content = "رمز باید بیشتر از 8 کاراکتر و کمتر از 32 کاراکتر باشد";
        }

        private void btnAddUserGroup_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserGroupName.Text != "" && btnAddUserGroup.Content.ToString() == "ایجاد گروه کاربری")
            {
                UserGroup ug = new UserGroup();
                ug.Title = txtUserGroupName.Text;
                //Customer >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblCustomersAccess.Content.ToString(), cbEnterCustomerImage, cbAddCustomerImage, cbEditCustomerImage, cbDeleteCustomerImage));
                //Product >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblProductsAccess.Content.ToString(), cbEnterProductImage, cbAddProductImage, cbEditProductImage, cbDeleteProductImage));
                //Setting >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblSettingAccess.Content.ToString(), cbEnterSettingImage, cbAddSettingImage, cbEditSettingImage, cbDeleteSettingImage));
                //Activity >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblActivitiesAccess.Content.ToString(), cbEnterActivityImage, cbAddActivityImage, cbEditActivityImage, cbDeleteActivityImage));
                //Invoice >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblInvoicesAccess.Content.ToString(), cbEnterInvoiceImage, cbAddInvoiceImage, cbEditInvoiceImage, cbDeleteInvoiceImage));
                //Message >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblMessagesAccess.Content.ToString(), cbEnterMessageImage, cbAddMessageImage, cbEditMessageImage, cbDeleteMessageImage));
                //Reminder >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblRemindersAccess.Content.ToString(), cbEnterReminderImage, cbAddReminderImage, cbEditReminderImage, cbDeleteReminderImage));
                //Report >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblReportsAccess.Content.ToString(), cbEnterReportImage, cbAddReportImage, cbEditReportImage, cbDeleteReportImage));
                //User >>>
                ug.UserAccessRoles.Add(FillAccessRole(lblUsersAccess.Content.ToString(), cbEnterUserImage, cbAddUserImage, cbEditUserImage, cbDeleteUserImage));

                MessageBox.Show(UGbll.Create(ug), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearCB();
                PublicMethods.dgvFiller(dgvUserGroups, UGbll.Read());
                txtUserGroupName.Focus();
            }
            else if (txtUserGroupName.Text != "" && btnAddUserGroup.Content.ToString() == "ویرایش")
            {
                foreach (var item in ugEdit.UserAccessRoles)
                {
                    if (item.Section == "بخش مشتریان")
                    {
                        FillAccessRoleForUpdate(item, cbEnterCustomerImage, cbAddCustomerImage, cbEditCustomerImage, cbDeleteCustomerImage);
                    }
                    else if (item.Section == "بخش کالاها")
                    {
                        FillAccessRoleForUpdate(item, cbEnterProductImage, cbAddProductImage, cbEditProductImage, cbDeleteProductImage);
                    }
                    else if (item.Section == "بخش فاکتورها")
                    {
                        FillAccessRoleForUpdate(item, cbEnterInvoiceImage, cbAddInvoiceImage, cbEditInvoiceImage, cbDeleteInvoiceImage);
                    }
                    else if (item.Section == "بخش فعالیت ها")
                    {
                        FillAccessRoleForUpdate(item, cbEnterActivityImage, cbAddActivityImage, cbEditActivityImage, cbDeleteActivityImage);
                    }
                    else if (item.Section == "بخش یادآور ها")
                    {
                        FillAccessRoleForUpdate(item, cbEnterReminderImage, cbAddReminderImage, cbEditReminderImage, cbDeleteReminderImage);
                    }
                    else if (item.Section == "بخش کاربران")
                    {
                        FillAccessRoleForUpdate(item, cbEnterUserImage, cbAddUserImage, cbEditUserImage, cbDeleteUserImage);
                    }
                    else if (item.Section == "پنل پیامکی")
                    {
                        FillAccessRoleForUpdate(item, cbEnterMessageImage, cbAddMessageImage, cbEditMessageImage, cbDeleteMessageImage);
                    }
                    else if (item.Section == "بخش گزارشات")
                    {
                        FillAccessRoleForUpdate(item, cbEnterReportImage, cbAddReportImage, cbEditReportImage, cbDeleteReportImage);
                    }
                    else if (item.Section == "بخش تنظیمات")
                    {
                        FillAccessRoleForUpdate(item, cbEnterSettingImage, cbAddSettingImage, cbEditSettingImage, cbDeleteSettingImage);
                    }

                }
                MessageBox.Show(UGbll.Update(ugEdit.Title, ugEdit.UserAccessRoles), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearCB();
                btnAddUserGroup.Content = "ایجاد گروه کاربری";
                btnAddUserGroup.IsEnabled = Add;
                txtUserGroupName.Text = "";
                txtUserGroupName.IsEnabled = true;
                txtUserGroupName.Focus();
            }
            else MessageBox.Show("لطفا نام گروه کاربری را خالی نگذارید");
        }

        private void miEditUserGroup_Click(object sender, RoutedEventArgs e)
        {
            txtUserGroupName.Text = ugEdit.Title;
            txtUserGroupName.IsEnabled = false;
            foreach (var item in ugEdit.UserAccessRoles)
            {
                if (item.Section == "بخش مشتریان")
                {
                    FillForUpdate(cbEnterCustomerImage, cbAddCustomerImage, cbEditCustomerImage, cbDeleteCustomerImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }
                else if (item.Section == "بخش کالاها")
                {
                    FillForUpdate(cbEnterProductImage, cbAddProductImage, cbEditProductImage, cbDeleteProductImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }
                else if (item.Section == "بخش فاکتورها")
                {
                    FillForUpdate(cbEnterInvoiceImage, cbAddInvoiceImage, cbEditInvoiceImage, cbDeleteInvoiceImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }
                else if (item.Section == "بخش فعالیت ها")
                {
                    FillForUpdate(cbEnterActivityImage, cbAddActivityImage, cbEditActivityImage, cbDeleteActivityImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }
                else if (item.Section == "بخش یادآور ها")
                {
                    FillForUpdate(cbEnterReminderImage, cbAddReminderImage, cbEditReminderImage, cbDeleteReminderImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }
                else if (item.Section == "بخش کاربران")
                {
                    FillForUpdate(cbEnterUserImage, cbAddUserImage, cbEditUserImage, cbDeleteUserImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }
                else if (item.Section == "پنل پیامکی")
                {
                    FillForUpdate(cbEnterMessageImage, cbAddMessageImage, cbEditMessageImage, cbDeleteMessageImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }
                else if (item.Section == "بخش گزارشات")
                {
                    FillForUpdate(cbEnterReportImage, cbAddReportImage, cbEditReportImage, cbDeleteReportImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }
                else if (item.Section == "بخش تنظیمات")
                {
                    FillForUpdate(cbEnterSettingImage, cbAddSettingImage, cbEditSettingImage, cbDeleteSettingImage, item.CanEnter, item.CanCreate, item.CanUpdate, item.CanDelete);
                }

            }

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
