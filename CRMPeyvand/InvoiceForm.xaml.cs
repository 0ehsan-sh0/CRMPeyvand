using BE;
using Section = BE.Section;
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
    public class InvoiceItemReportDto
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Count { get; set; }
    }

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
        CatalogItemBLL Pbll = new CatalogItemBLL();
        InvoiceBLL Ibll = new InvoiceBLL();
        Customer cbCustomer = new Customer();
        List<InvoiceLine> draftLines = new List<InvoiceLine>();
        OffCodeBLL Obll = new OffCodeBLL();
        CatalogItem cbProduct = new CatalogItem();
        Invoice invoiceEdit = new Invoice();
        UserBLL Ubll = new UserBLL();
        User u = new User();
        OffCode off = new OffCode();

        private static string GetReportPath(string mrtFileName)
        {
            string path = System.IO.Path.Combine(AppContext.BaseDirectory, "Reports", mrtFileName);
            if (System.IO.File.Exists(path))
                return path;
            string devPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\Reports", mrtFileName));
            if (System.IO.File.Exists(devPath))
                return devPath;
            return path;
        }

        string countOff()
        {
            if (cbCustomer != null)
            {
                string result = Obll.CanUse(txtOff.Text, cbCustomer);
                if (double.TryParse(result, out double offcode))
                {
                    off = Obll.GetOffCode(txtOff.Text);
                    decimal total = draftLines.Sum(l => l.LineTotal);
                    decimal discount = Pricing.ComputeDiscount(off, total);
                    lblOff.Content = discount.ToString("N0");
                    lblFinalPrice.Content = Pricing.ComputePayable(total, discount).ToString("N0");
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
            int qty = cbProduct.Kind == ItemKind.Service ? 1 : Convert.ToInt32(txtCount.Text);
            if (cbProduct.Kind == ItemKind.Good && cbProduct.Stock < qty)
            {
                MessageBox.Show("موجودی انبار کافی نمیباشد");
                return;
            }
            var line = new InvoiceLine { CatalogItemId = cbProduct.Id, CatalogItem = cbProduct, UnitPrice = cbProduct.SalePrice, Quantity = qty };
            draftLines.Add(line);
            dgvProduts.ItemsSource = null;
            dgvProduts.ItemsSource = draftLines;
            lstResult.Items.Add(cbProduct.Name + " به ارزش " + cbProduct.SalePrice.ToString("N0") + " تومان "
                + (cbProduct.Kind == ItemKind.Good ? "به تعداد " + qty : ""));
            cbProduct = null;
            txtProduct.Text = "";
            txtCount.Text = "";
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

            if ((cbProduct != null && txtCount.Text != "" && txtCount.Text != "0" && cbProduct.Kind == ItemKind.Good) || (cbProduct != null && cbProduct.Kind == ItemKind.Service))
            {
                FillDataGrid();
                decimal sum = draftLines.Sum(l => l.LineTotal);
                lblTotalPrice.Content = sum.ToString("N0");
                if (txtOff.Text != string.Empty)
                {
                    countOff();
                }
                else lblFinalPrice.Content = sum.ToString("N0");
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!AccessGuard.Can(u, Section.Invoices, Operation.Create))
            {
                btnAdd.IsEnabled = false;
                Print.IsEnabled = false;
            }
            else
            {
                btnAdd.IsEnabled = true;
                Print.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Invoices, Operation.Edit))
            {
                miDone.IsEnabled = false;
            }
            else
            {
                miDone.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Invoices, Operation.Delete))
            {
                miDelete.IsEnabled = false;
                miDeleteInvoice.IsEnabled = false;
            }
            else
            {
                miDelete.IsEnabled = true;
                miDeleteInvoice.IsEnabled = true;
            }

            txtCustomer.ItemsSource = Cbll.ReadPhoneNumbers();
            txtProduct.ItemsSource = Pbll.ReadNames();
            lblDate.Content = DateTime.Now.Date.ToString("yyyy/MM/dd");
            lblCount.Content = Ibll.CountInvoices();
            PublicMethods.dgvFiller(dgvInvoices, Ibll.Read());
        }

        private void txtProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtProduct.SelectedItem != null)
            {
                cbProduct = Pbll.ReadByName(txtProduct.SelectedItem.ToString());
                if (cbProduct.Kind == ItemKind.Service)
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
                invoice.User = u;
                Invoice savedInvoice;
                try
                {
                    savedInvoice = Ibll.Create(invoice, cbCustomer.id, draftLines.ToList());
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show($"فاکتور با شماره {savedInvoice.id} ثبت شد", "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                if (IsCheckedOutImage.Visibility == Visibility.Visible)
                {
                    Ibll.Done(savedInvoice.id);
                }
                PublicMethods.dgvFiller(dgvInvoices, Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
                dgvProduts.ItemsSource = null;
                draftLines.Clear();
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
            draftLines.Clear();
            lstResult.Items.Clear();
            lblFinalPrice.Content = "0";
            lblTotalPrice.Content = "0";
        }

        private void miDeleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (!AccessGuard.Can(u, Section.Invoices, Operation.Delete))
            {
                return;
            }
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                Ibll.Delete(invoiceEdit.id);
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
                
                invoiceEdit = Ibll.ReadById(Convert.ToInt32(PublicMethods.ReadTheEntityCode(dgvInvoices, 0)));
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
                invoice.User = u;
                Invoice savedInvoice;
                try
                {
                    savedInvoice = Ibll.Create(invoice, cbCustomer.id, draftLines.ToList());
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show($"فاکتور با شماره {savedInvoice.id} ثبت شد", "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                if (IsCheckedOutImage.Visibility == Visibility.Visible)
                {
                    Ibll.Done(savedInvoice.id);
                }
                PublicMethods.dgvFiller(dgvInvoices, Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
                StiReport sti = new StiReport();
                sti.Load(GetReportPath("InvoicePrint.mrt"));
                sti.CalculationMode = StiCalculationMode.Interpretation;

                if (sti.Dictionary.Variables.Contains("InvoiceNum"))
                    sti.Dictionary.Variables["InvoiceNum"].Value = savedInvoice.id.ToString();
                if (sti.Dictionary.Variables.Contains("Date"))
                    sti.Dictionary.Variables["Date"].Value = lblDate.Content?.ToString() ?? DateTime.Now.Date.ToString("yyyy/MM/dd");
                if (sti.Dictionary.Variables.Contains("CustomerName"))
                    sti.Dictionary.Variables["CustomerName"].Value = lblName.Content?.ToString() ?? "";
                if (sti.Dictionary.Variables.Contains("CustomerPhone"))
                    sti.Dictionary.Variables["CustomerPhone"].Value = lblPhone.Content?.ToString() ?? "";
                if (sti.Dictionary.Variables.Contains("TotalPrice"))
                    sti.Dictionary.Variables["TotalPrice"].Value = lblTotalPrice.Content?.ToString() ?? "0";
                if (sti.Dictionary.Variables.Contains("FinalPrice"))
                    sti.Dictionary.Variables["FinalPrice"].Value = lblFinalPrice.Content?.ToString() ?? "0";

                var reportItems = draftLines.Select(l => new InvoiceItemReportDto
                {
                    Name = l.CatalogItem?.Name ?? "",
                    Price = (double)l.UnitPrice,
                    Count = l.Quantity
                }).ToList();

                sti.RegBusinessObject("Product", "Product", reportItems);
                sti.Dictionary.Synchronize();
                sti.Render(false);
                sti.Show();
                dgvProduts.ItemsSource = null;
                draftLines.Clear();
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
