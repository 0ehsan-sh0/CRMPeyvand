using BE;
using BLL;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
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
using System.Xml.Linq;
using static HandyControl.Tools.Interop.InteropValues;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for InvoiceForm.xaml
    /// </summary>
    public partial class InvoiceForm : Window
    {
        public InvoiceForm()
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
        }

        CustomerBLL Cbll = new CustomerBLL();
        ProductBLL Pbll = new ProductBLL();
        InvoiceBLL Ibll = new InvoiceBLL();
        Customer cbCustomer = new Customer();
        List<Product> products = new List<Product>();
        OffCodeBLL Obll = new OffCodeBLL();
        Product cbProduct = new Product();
        Invoice invoiceEdit = new Invoice();
        UserBLL Ubll = new UserBLL();
        User u = new User();
        OffCode off = new OffCode();
        string countOff()
        {
            if (cbCustomer != null)
            {
                string result = Obll.CanUse(txtOff.Text, cbCustomer);
                if (double.TryParse(result, out double offcode))
                {
                    off = Obll.GetOffCode(txtOff.Text);
                    if (off.IsPrice)
                    {
                        double total = Convert.ToDouble(lblTotalPrice.Content);
                        lblOff.Content = (Convert.ToDouble(off.Price)).ToString("N0");
                        if (total >= Convert.ToDouble(lblOff.Content))
                        {
                            lblFinalPrice.Content = Convert.ToDouble(total - Convert.ToDouble(lblOff.Content)).ToString("N0");
                        }
                    }
                    else if (lblTotalPrice.Content.ToString() != "0")
                    {
                        double total = Convert.ToDouble(lblTotalPrice.Content);
                        lblOff.Content = Convert.ToDouble((total * off.Percent / 100)).ToString("N0");
                        lblFinalPrice.Content = Convert.ToDouble(total - Convert.ToDouble(lblOff.Content)).ToString("N0");
                    }
                    lblOffError.Content = "";
                    return off.Code;
                }
                else
                {
                    lblOffError.Content = result;
                    lblOff.Content = 0;
                    return null;
                }
            }
            else return null;

        }
        void FillDataGrid()
        {
            if (cbProduct.Type == "محصول")
            {
                if (cbProduct.Total >= Convert.ToInt32(txtCount.Text))
                {

                    cbProduct.Count = Convert.ToInt32(txtCount.Text);
                    products.Add(cbProduct);
                    dgvProduts.ItemsSource = null;
                    dgvProduts.ItemsSource = products;
                    string s = cbProduct.Name + " به ارزش " + cbProduct.Price.ToString("N0") + " تومان " + "به تعداد " + cbProduct.Count;
                    lstResult.Items.Add(s);
                    dgvProduts.Columns[0].Visibility = Visibility.Hidden;
                    dgvProduts.Columns[4].Visibility = Visibility.Hidden;
                    dgvProduts.Columns[5].Visibility = Visibility.Hidden;
                    dgvProduts.Columns[7].Visibility = Visibility.Hidden;
                    dgvProduts.Columns[1].Header = "نام کالا";
                    dgvProduts.Columns[2].Header = "قیمت";
                    dgvProduts.Columns[3].Header = "نوع کالا";
                    dgvProduts.Columns[6].Header = "تعداد";
                    if (dgvProduts.ItemsSource != null)
                    {
                        dgvProduts.MinColumnWidth = (dgvProduts.ActualWidth / (dgvProduts.Columns.Count - 4)) - 3;
                        dgvProduts.MaxColumnWidth = (dgvProduts.ActualWidth / (dgvProduts.Columns.Count - 4)) - 3;
                    }
                    cbProduct = null;
                    txtProduct.Text = "";
                    txtCount.Text = "";

                }
                else
                {
                    MessageBox.Show("موجودی انبار کافی نمیباشد");
                }
            }
            else
            {
                cbProduct.Count = 1;
                products.Add(cbProduct);
                dgvProduts.ItemsSource = null;
                dgvProduts.ItemsSource = products;
                string s = cbProduct.Name + " به ارزش " + cbProduct.Price.ToString("N0") + " تومان ";
                lstResult.Items.Add(s);

                dgvProduts.Columns[0].Visibility = Visibility.Hidden;
                dgvProduts.Columns[4].Visibility = Visibility.Hidden;
                dgvProduts.Columns[5].Visibility = Visibility.Hidden;
                dgvProduts.Columns[7].Visibility = Visibility.Hidden;
                dgvProduts.Columns[1].Header = "نام کالا";
                dgvProduts.Columns[2].Header = "قیمت";
                dgvProduts.Columns[3].Header = "نوع کالا";
                dgvProduts.Columns[6].Header = "تعداد";
                if (dgvProduts.ItemsSource != null)
                {
                    dgvProduts.MinColumnWidth = (dgvProduts.ActualWidth / (dgvProduts.Columns.Count - 4)) - 3;
                    dgvProduts.MaxColumnWidth = (dgvProduts.ActualWidth / (dgvProduts.Columns.Count - 4)) - 3;
                }
                cbProduct = null;
                txtProduct.Text = "";
                txtCount.Text = "";
            }

        }
        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void txtCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtCustomer.SelectedItem != null)
            {
                cbCustomer = Cbll.GetCustomer(txtCustomer.SelectedItem.ToString());
                lblName.Content = cbCustomer.Name;
                lblPhone.Content = cbCustomer.Phone;
                countOff();
            }
        }

        private void AddProductToList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if ((cbProduct != null && txtCount.Text != "" && txtCount.Text != "0" && cbProduct.Type == "محصول") || (cbProduct != null && cbProduct.Type == "خدمات"))
            {
                bool exist = false;
                foreach (var item in products)
                {
                    if (item.id == cbProduct.id)
                    {
                        exist = true;
                    }
                }
                if (!exist)
                {
                    FillDataGrid();
                    double sum = 0;
                    foreach (var item in dgvProduts.Items)
                    {
                        Product p = new Product();
                        p = item as Product;
                        sum += p.Price * p.Count;
                    }
                    lblTotalPrice.Content = sum.ToString("N0");
                    if (sum >= Convert.ToDouble(lblOff.Content))
                    {
                        if (txtOff.Text != string.Empty)
                        {
                            countOff();
                        }
                        else lblFinalPrice.Content = sum.ToString("N0");
                    }
                    else lblFinalPrice.Content = sum.ToString("N0");
                }
                else MessageBox.Show("شما در حال حاضر این کالا را در سبد خرید دارید");
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!Ubll.Access(u, "بخش فعالیت ها", 2))
            {
                btnAdd.IsEnabled = false;
                Print.IsEnabled = false;
            }
            else
            {
                btnAdd.IsEnabled = true;
                Print.IsEnabled = true;
            }
            if (!Ubll.Access(u, "بخش فعالیت ها", 4))
            {
                miDelete.IsEnabled = false;
            }
            else
            {
                miDelete.IsEnabled = true;
            }

            txtCustomer.ItemsSource = Cbll.ReadPhoneNumbers();
            txtProduct.ItemsSource = Pbll.ReadNames();
            lblDate.Content = DateTime.Now.Date.ToString("yyyy/MM/dd");
            lblCount.Content = Ibll.CountInvoices();
            dgvProduts.AutoGenerateColumns = true;
            dgvProduts.CanUserAddRows = false;
            PublicMethods.dgvFiller(dgvInvoices, Ibll.Read());
        }

        private void txtProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtProduct.SelectedItem != null)
            {
                cbProduct = Pbll.ReadByName(txtProduct.SelectedItem.ToString());
                if (cbProduct.Type == "خدمات")
                {
                    txtCount.IsEnabled = false;
                }
                else
                {
                    txtCount.IsEnabled = true;
                    txtCount.Text = "";
                }
            }
        }

        private void txtCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            PublicMethods.FilterNumber(t);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtCustomer.SelectedItem != null && dgvProduts.Items.Count != 0)
            {
                Invoice invoice = new Invoice();
                invoice.RegDate = DateTime.Now;
                invoice.OffCode = countOff();
                invoice.TotalPrice = Convert.ToDouble(lblFinalPrice.Content);
                invoice.User = u;
                
                foreach (var item in dgvProduts.Items)
                {
                    Product p = new Product();
                    p = item as Product;
                    invoice.TotalCount += p.Count;
                }
                MessageBox.Show(Ibll.Create(invoice, cbCustomer, products), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                if (IsCheckedOutImage.Visibility == Visibility.Visible)
                {
                    int id = Ibll.ReadInvoiceLastID();
                    Ibll.Done(id);
                }
                PublicMethods.dgvFiller(dgvInvoices, Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
                dgvProduts.ItemsSource = null;
                products.Clear();
                lstResult.Items.Clear();
                lblFinalPrice.Content = "0";
                lblTotalPrice.Content = "0";
                txtOff.Clear();
                lblOff.Content = "0";
                txtCustomer.Focus();
            }
            else MessageBox.Show("لطفا تمامی فیلد های مورد نیاز را پر کنید");
        }

        private void IsCheckedOut_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsCheckedOutImage.Visibility == Visibility.Visible)
            {
                IsCheckedOutImage.Visibility = Visibility.Hidden;
            }
            else if (IsCheckedOutImage.Visibility == Visibility.Hidden)
            {
                IsCheckedOutImage.Visibility = Visibility.Visible;
            }

        }
        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            dgvProduts.ItemsSource = null;
            products.Clear();
            lstResult.Items.Clear();
            lblFinalPrice.Content = "0";
            lblTotalPrice.Content = "0";
        }

        private void miDeleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                Ibll.Delete(invoiceEdit.InvoiceNumber);
                PublicMethods.dgvFiller(dgvInvoices, Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
            }

        }

        private void miDone_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Ibll.Done(invoiceEdit.id));
        }

        private void dgvInvoices_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvInvoices, 0) != null)
            {
                dgvInvoices.ContextMenu.IsEnabled = true;
                
                invoiceEdit = Ibll.Read(PublicMethods.ReadTheEntityCode(dgvInvoices, 0));
            }
            else
            {
                dgvInvoices.ContextMenu.IsEnabled = false;
            }
        }

        private void Print_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txtCustomer.SelectedItem != null && dgvProduts.Items.Count != 0)
            {
                Invoice invoice = new Invoice();
                invoice.RegDate = DateTime.Now;
                invoice.OffCode = countOff();
                invoice.TotalPrice = Convert.ToDouble(lblFinalPrice.Content);
                invoice.User = u;
                foreach (var item in dgvProduts.Items)
                {
                    Product p = new Product();
                    p = item as Product;
                    invoice.TotalCount += p.Count;
                }
                
                MessageBox.Show(Ibll.Create(invoice, cbCustomer, products), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                if (IsCheckedOutImage.Visibility == Visibility.Visible)
                {
                    int id = Ibll.ReadInvoiceLastID();
                    Ibll.Done(id);
                }
                PublicMethods.dgvFiller(dgvInvoices, Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
                StiReport sti = new StiReport();
                sti.Load(System.IO.Path.GetFullPath(System.IO.Path.GetFullPath(Directory.GetParent(Directory.GetParent(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString()).ToString()) + @"\Reports\InvoicePrint.mrt")));
                sti.Dictionary.Variables["InvoiceNum"].Value = Ibll.ReadInvoiceNumIsReport();
                sti.Dictionary.Variables["Date"].Value = lblDate.Content.ToString();
                sti.Dictionary.Variables["CustomerName"].Value = lblName.Content.ToString();
                sti.Dictionary.Variables["CustomerPhone"].Value = lblPhone.Content.ToString();
                sti.Dictionary.Variables["TotalPrice"].Value = lblTotalPrice.Content.ToString();
                sti.Dictionary.Variables["FinalPrice"].Value = lblFinalPrice.Content.ToString();
                sti.RegBusinessObject("Product", products);
                sti.Render();
                sti.Show();
                dgvProduts.ItemsSource = null;
                products.Clear();
                lstResult.Items.Clear();
                lblFinalPrice.Content = "0";
                lblTotalPrice.Content = "0";
                txtOff.Clear();
                lblOff.Content = "0";
                txtCustomer.Focus();
            }
            else MessageBox.Show("لطفا تمامی فیلد های مورد نیاز را پر کنید");
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text != String.Empty)
            {
                TextBox textBox = sender as TextBox;
                PublicMethods.FilterNumber(textBox);
                PublicMethods.dgvFiller(dgvInvoices, Ibll.Search(txtSearch.Text));
            }
            else PublicMethods.dgvFiller(dgvInvoices, Ibll.Read());  
        }

        private void txtOff_LostFocus(object sender, RoutedEventArgs e)
        {
            countOff();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
            }
        }
    }
}
