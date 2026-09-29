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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BE;
using BLL;
using DAL;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for DataBaseForm.xaml
    /// </summary>
    public partial class DataBaseForm : Window
    {
        public DataBaseForm()
        {
            InitializeComponent();
        }
        SettingBLL bll = new SettingBLL();
        Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
        string BackUPDB(string userPath)
        {

            string path = userPath + @"/CRMPeyvand.bak";

            if (!Directory.Exists(userPath))
            {
                System.Windows.MessageBox.Show("مسیر مورد نظر برای ذخیره فایل پیدا نشد", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            else
            {
                return bll.BackUp(path);
            }

        }
        //BackUp -----
        private void btnSMSActivation_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.Cancel)
            {
                if (folderBrowserDialog1.SelectedPath != null)
                {
                    System.Windows.MessageBox.Show(BackUPDB(folderBrowserDialog1.SelectedPath), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

        }
        //----- BackUp
        private void BackToSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        //Restore -----
        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {

        }
        //----- Restore
        private void btnDeleteDB_Click(object sender, RoutedEventArgs e)
        {

        }

        //پیکربندی پایگاه داده -----

        private const int SqlServerIndex = 1;

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
            }
        }

        /// <summary>
        /// Fills the form from whatever the app is actually using right now.
        ///
        /// A SQL Server installation shows its own settings so they can be
        /// corrected; anything else shows SQLite with the SQL Server fields
        /// blank, because there is nothing meaningful to pre-fill. The fields
        /// are deliberately not seeded from App.config: the point of this
        /// screen is that what the user types here wins, and a stale
        /// configuration file is not a thing they can see or edit.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var current = DataSource.Current;
            if (current.Kind == DbProviderKind.SqlServer)
            {
                cmbProvider.SelectedIndex = SqlServerIndex;
                try
                {
                    var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
                        current.ConnectionString);
                    txtServer.Text = builder.DataSource;
                    txtDatabase.Text = builder.InitialCatalog;
                    txtUser.Text = builder.UserID;
                    chkWindowsAuth.IsChecked = builder.IntegratedSecurity;
                }
                catch (Exception)
                {
                    // A stored connection string we cannot parse is not a reason
                    // to refuse to open the window; the user can retype it.
                    cmbProvider.SelectedIndex = SqlServerIndex;
                }
            }
            else
            {
                cmbProvider.SelectedIndex = 0;
            }

            ProviderChanged();
        }

        private void cmbProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProviderChanged();
        }

        private void chkWindowsAuth_Changed(object sender, RoutedEventArgs e)
        {
            ProviderChanged();
        }

        /// <summary>
        /// Greys out whatever the chosen provider does not use, so it is obvious
        /// which fields are actually in play.
        /// </summary>
        private void ProviderChanged()
        {
            bool sqlServer = cmbProvider.SelectedIndex == SqlServerIndex;
            bool sqlLogin = sqlServer && chkWindowsAuth.IsChecked == false;
            txtServer.IsEnabled = sqlServer;
            txtDatabase.IsEnabled = sqlServer;
            txtUser.IsEnabled = sqlLogin;
            txtPassword.IsEnabled = sqlLogin;
        }

        /// <summary>
        /// The DataSource the form currently describes, or an error instead:
        /// building the connection string can fail on its own (a blank server
        /// name is enough), and that must be reported rather than thrown out of
        /// a Click handler.
        /// </summary>
        private bool TryBuildFromForm(out DataSource candidate, out string error)
        {
            candidate = null;
            error = null;

            try
            {
                if (cmbProvider.SelectedIndex != SqlServerIndex)
                {
                    candidate = DataSource.DefaultSqlite();
                    return true;
                }

                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
                {
                    DataSource = txtServer.Text,
                    InitialCatalog = txtDatabase.Text,
                    IntegratedSecurity = chkWindowsAuth.IsChecked == true,
                };
                if (builder.IntegratedSecurity == false)
                {
                    builder.UserID = txtUser.Text;
                    builder.Password = txtPassword.Password;
                }
                builder.TrustServerCertificate = true;
                builder.MultipleActiveResultSets = true;

                candidate = new DataSource
                {
                    Kind = DbProviderKind.SqlServer,
                    ConnectionString = builder.ToString(),
                };
                return true;
            }
            catch (Exception e)
            {
                error = "پیکربندی پایگاه داده نامعتبر است:\n" + e.Message;
                return false;
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildFromForm(out var candidate, out var buildError))
            {
                System.Windows.MessageBox.Show(buildError, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var message = DataSource.Test(candidate);
            System.Windows.MessageBox.Show(
                message ?? "اتصال با موفقیت برقرار شد",
                message == null ? "اطلاعیه" : "خطا", MessageBoxButton.OK,
                message == null ? MessageBoxImage.Information : MessageBoxImage.Error);
        }

        /// <summary>
        /// Saves only what has been proven to work. A DataSource that cannot be
        /// opened is a worse state than the one the user is leaving, so the
        /// test gate is the whole point of this handler.
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildFromForm(out var candidate, out var buildError))
            {
                System.Windows.MessageBox.Show(buildError, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var message = DataSource.Test(candidate);
            if (message != null)
            {
                System.Windows.MessageBox.Show(
                    message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // DataSource.Use writes provider.json, so it can fail on a locked or
            // unwritable data folder. That is a real outcome the user has to be
            // told about, not a reason to take the window down with it.
            try
            {
                DataSource.Use(candidate);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "پیکربندی پایگاه داده ذخیره نشد:\n" + ex.Message,
                    "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Windows.MessageBox.Show(
                "پیکربندی پایگاه داده ذخیره شد لطفا برنامه را دوباره اجرا کنید",
                "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        //----- پیکربندی پایگاه داده

    }
}
