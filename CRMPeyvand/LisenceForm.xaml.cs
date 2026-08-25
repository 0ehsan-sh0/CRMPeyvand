using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FoxLearn.License;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for LisenceForm.xaml
    /// </summary>
    public partial class LisenceForm : UserControl
    {
        public LisenceForm()
        {
            InitializeComponent();
        }
        void Swithchpanels()
        {
            lblDetails.Visibility = Visibility.Hidden;
            lblWelcome.Visibility = Visibility.Hidden;
            btnBorder.Visibility = Visibility.Hidden;
            borderLisence.Visibility = Visibility.Hidden;
            borderName.Visibility = Visibility.Hidden;
            img.Visibility = Visibility.Hidden;
            rcCopy.IsEnabled = false; 
            rcCopy.Visibility = Visibility.Hidden;
            RegisterUC ruc = new RegisterUC();
            Grid.SetRow(ruc, 0);
            Grid.SetColumn(ruc, 0);
            Grid.SetColumnSpan(ruc, 6);
            Grid.SetRowSpan(ruc, 6);
            MainGrid.Children.Add(ruc);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Text = ComputerInfo.GetComputerId();
        }

        private void btnLicense_Click(object sender, RoutedEventArgs e)
        {
            KeyManager km = new KeyManager(txtName.Text);
            string productKey = txtLisence.Text;
            if (km.ValidKey(ref productKey))
            {
                KeyValuesClass kv = new KeyValuesClass();
                if (km.DisassembleKey(productKey, ref kv))
                {
                    LicenseInfo lic = new LicenseInfo();
                    lic.ProductKey = productKey;
                    lic.FullName = "Personal accounting";
                    if (kv.Type == LicenseType.TRIAL)
                    {
                        lic.Day = kv.Expiration.Day;
                        lic.Month = kv.Expiration.Month;
                        lic.Year = kv.Expiration.Year;
                    }

                    km.SaveSuretyFile(string.Format(@"{0}\Key.lic", Application.Current.StartupUri), lic);
                    MessageBox.Show("!!تبریک نرم افزار با موفقیت فعال شد");
                    Swithchpanels();
                }
            }
            else
            {
                MessageBox.Show("لایسنس وارد شده صحیح نمیباشد", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLisence.Clear();
            }            
        }

        private void txtName_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txtName.Text != String.Empty)
            {
                Clipboard.SetText(txtName.Text);
                MessageBox.Show("کد کپی شد");
            }
        }
    }
}
