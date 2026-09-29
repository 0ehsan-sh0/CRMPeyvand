using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        /// <summary>
        /// Suppresses the dirty flag while the form is being populated from the
        /// saved configuration, so loading a form is not reported as an edit.
        /// </summary>
        private bool _loading;

        /// <summary>True when the fields hold something the user has not saved yet.</summary>
        private bool _dirty;

        private string BackUPDB(string userPath)
        {
            var path = userPath + @"/CRMPeyvand.bak";

            if (!Directory.Exists(userPath))
            {
                System.Windows.MessageBox.Show("مسیر مورد نظر برای ذخیره فایل پیدا نشد", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            return bll.BackUp(path);
        }

        private static void Say(string message, bool isError = false)
        {
            System.Windows.MessageBox.Show(
                message,
                isError ? "خطا" : "اطلاعیه",
                MessageBoxButton.OK,
                isError ? MessageBoxImage.Error : MessageBoxImage.Information);
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

        //----- پیکربندی پایگاه داده

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _loading = true;
            try
            {
                LoadSavedConnection();
            }
            finally
            {
                _loading = false;
            }

            UpdateConnectionStringPreview();
            RefreshStatus();
        }

        /// <summary>
        /// Fills the SQL Server fields from the connection the user has already
        /// saved, whether it is currently active or merely kept while the app
        /// runs on SQLite. Populating it is what makes "deactivate" reversible:
        /// the credentials are still on screen, so switching back is one click
        /// rather than retyping a server, a login and a password.
        ///
        /// The fields are deliberately not seeded from App.config. What the user
        /// saved from this screen is what they can see and change here; a stale
        /// configuration file is not.
        /// </summary>
        private void LoadSavedConnection()
        {
            var current = DataSource.Current;
            if (!current.IsSqlServerConfigured) return;

            try
            {
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(
                    current.SqlServerConnectionString);

                txtServer.Text = builder.DataSource;
                txtDatabase.Text = builder.InitialCatalog;
                chkWindowsAuth.IsChecked = builder.IntegratedSecurity;

                if (builder.IntegratedSecurity == false)
                {
                    txtUser.Text = builder.UserID;
                    txtPassword.Password = builder.Password;
                }
            }
            catch (Exception)
            {
                // A stored connection string we cannot parse is not a reason to
                // refuse to open the window. The user can retype it, and the
                // "آزمایش اتصال" button is there to prove whatever they type.
                txtServer.Clear();
                txtDatabase.Clear();
                txtUser.Clear();
                txtPassword.Clear();
                chkWindowsAuth.IsChecked = false;
            }
        }

        /// <summary>
        /// Shows which provider the app is on, where the SQLite file is and
        /// whether it exists yet, and words the toggle for what clicking it will
        /// actually do.
        /// </summary>
        private void RefreshStatus()
        {
            var current = DataSource.Current;

            lblActiveProvider.Text = current.Kind == DbProviderKind.SqlServer
                ? "SQL Server"
                : "SQLite";

            var path = DataSource.SqliteFilePath;
            txtSqlitePath.Text = path;
            lblSqliteExists.Text = File.Exists(path)
                ? "فایل موجود است"
                : "هنوز ساخته نشده است - با اولین اجرا ساخته می‌شود";

            btnToggleSqlServerText.Text = current.Kind == DbProviderKind.SqlServer
                ? "غیرفعال کردن اتصال SQL Server"
                : "فعال کردن اتصال SQL Server";
        }

        /// <summary>
        /// The connection string built from what is on screen right now, or a
        /// Persian error. Building can fail on its own - a blank server name is
        /// enough - and that must be reported rather than thrown out of a Click
        /// handler.
        /// </summary>
        private bool TryBuildFromForm(out DataSource candidate, out string error)
        {
            candidate = null;
            error = null;

            try
            {
                // System.Data.SqlClient, not Microsoft.Data.SqlClient: EF6's SQL
                // Server provider services only accept this one, and the string
                // is built here so it matches what they will be handed.
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder
                {
                    DataSource = txtServer.Text.Trim(),
                    InitialCatalog = txtDatabase.Text.Trim(),
                    IntegratedSecurity = chkWindowsAuth.IsChecked == true,
                };

                if (builder.IntegratedSecurity == false)
                {
                    builder.UserID = txtUser.Text.Trim();
                    builder.Password = txtPassword.Password;
                }

                // The app already relies on both of these: multiple active result
                // sets for the report queries, and a trusted certificate for a
                // local instance that uses a self-signed one.
                builder.TrustServerCertificate = true;
                builder.MultipleActiveResultSets = true;

                candidate = DataSource.ForSqlServer(builder.ToString());
                return true;
            }
            catch (Exception e)
            {
                error = "پیکربندی پایگاه داده نامعتبر است:\n" + e.Message;
                return false;
            }
        }

        /// <summary>
        /// Mirrors the composed connection string on screen, so the user can see
        /// exactly what will be stored - including the password when they chose
        /// SQL authentication. Showing it is the alternative to hiding a secret
        /// they cannot otherwise correct.
        /// </summary>
        private void UpdateConnectionStringPreview()
        {
            txtConnectionString.Text = TryBuildFromForm(out var candidate, out _)
                ? candidate.ConnectionString
                : string.Empty;
        }

        private void FieldsChanged()
        {
            if (_loading) return;

            UpdateConnectionStringPreview();

            var current = DataSource.Current;
            bool differs = !current.IsSqlServerConfigured
                || !TryBuildFromForm(out var candidate, out _)
                || !string.Equals(candidate.SqlServerConnectionString,
                                 current.SqlServerConnectionString,
                                 StringComparison.Ordinal);

            _dirty = differs;
            lblDirty.Text = differs ? "ذخیره نشده" : "ذخیره شده";
            lblDirty.Foreground = differs
                ? System.Windows.Media.Brushes.Firebrick
                : System.Windows.Media.Brushes.SeaGreen;
        }

        // TextBox and PasswordBox raise different event types, so each needs its
        // own handler with the exact delegate signature; both do the same thing.
        private void TextFieldChanged(object sender, TextChangedEventArgs e)
        {
            FieldsChanged();
        }

        private void PasswordFieldChanged(object sender, RoutedEventArgs e)
        {
            FieldsChanged();
        }

        private void chkWindowsAuth_Changed(object sender, RoutedEventArgs e)
        {
            // The two fields are meaningless under Windows authentication.
            bool sqlLogin = chkWindowsAuth.IsChecked == false;
            txtUser.IsEnabled = sqlLogin;
            txtPassword.IsEnabled = sqlLogin;

            FieldsChanged();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildFromForm(out var candidate, out var buildError))
            {
                Say(buildError, true);
                return;
            }

            var message = DataSource.Test(candidate);
            if (message == null)
            {
                Say("اتصال با موفقیت برقرار شد");
            }
            else
            {
                Say(message, true);
            }
        }

        /// <summary>
        /// Saves the connection and activates it, but only after it has been
        /// proven to work. A saved connection that cannot be opened is a worse
        /// state than the one the user is leaving, so the test gate is the whole
        /// point of this handler. It covers the first save and every later edit.
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildFromForm(out var candidate, out var buildError))
            {
                Say(buildError, true);
                return;
            }

            var message = DataSource.Test(candidate);
            if (message != null)
            {
                Say(message, true);
                return;
            }

            if (!TryUse(candidate)) return;

            _dirty = false;
            lblDirty.Text = "ذخیره شده";
            lblDirty.Foreground = System.Windows.Media.Brushes.SeaGreen;
            RefreshStatus();

            Say("پیکربندی ذخیره شد و اتصال SQL Server فعال شد.\nلطفا برنامه را دوباره اجرا کنید.");
        }

        /// <summary>
        /// Turns the SQL Server connection off, or back on.
        ///
        /// Off never fails - SQLite needs nothing installed, and the saved
        /// connection is kept so the switch can be thrown back. On goes through
        /// the same test as saving, because activating credentials that no longer
        /// work would leave the app unable to start.
        /// </summary>
        private void btnToggleSqlServer_Click(object sender, RoutedEventArgs e)
        {
            var current = DataSource.Current;

            if (current.Kind == DbProviderKind.SqlServer)
            {
                if (!TryUse(current.DeactivateSqlServer())) return;

                RefreshStatus();
                Say("اتصال SQL Server غیرفعال شد.\nاطلاعات اتصال ذخیره شده باقی ماند و برنامه از SQLite استفاده می‌کند.");
                return;
            }

            if (!current.IsSqlServerConfigured)
            {
                Say("ابتدا اطلاعات اتصال SQL Server را وارد کرده و ذخیره کنید.", true);
                return;
            }

            if (_dirty)
            {
                Say("تغییرات ذخیره نشده است. ابتدا دکمه «ذخیره و فعال سازی» را بزنید.", true);
                return;
            }

            DataSource candidate;
            try
            {
                candidate = current.ActivateSqlServer();
            }
            catch (InvalidOperationException ex)
            {
                Say(ex.Message, true);
                return;
            }

            var message = DataSource.Test(candidate);
            if (message != null)
            {
                Say(message, true);
                return;
            }

            if (!TryUse(candidate)) return;

            RefreshStatus();
            Say("اتصال SQL Server فعال شد.\nلطفا برنامه را دوباره اجرا کنید.");
        }

        /// <summary>
        /// Persists a data source, reporting the failure rather than letting it
        /// escape: Use writes provider.json, so an unwritable or locked data
        /// folder is a real outcome the user has to be told about.
        /// </summary>
        private bool TryUse(DataSource candidate)
        {
            try
            {
                DataSource.Use(candidate);
                return true;
            }
            catch (Exception ex)
            {
                Say("پیکربندی پایگاه داده ذخیره نشد:\n" + ex.Message, true);
                return false;
            }
        }
        //----- پیکربندی پایگاه داده
    }
}
