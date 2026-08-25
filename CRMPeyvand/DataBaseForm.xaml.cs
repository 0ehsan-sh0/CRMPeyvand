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

    }
}
