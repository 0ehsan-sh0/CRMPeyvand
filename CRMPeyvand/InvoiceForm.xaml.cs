using BE;
using Section = BE.Section;
using BLL;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Services;
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

        private const string InvoiceNumberColumn = "شماره فاکتور";

        /// <summary>
        /// The DAL's own name for the amount column, so the grid groups the column
        /// the query produced rather than a name repeated here. The line grid
        /// declares its own columns and already carries "N0".
        /// </summary>
        private const string AmountColumn = "هزینه پرداختی";

        /// <summary>
        /// Every fill has to name the amount column again: a grid generated from a
        /// DataTable throws its columns away and rebuilds them each time, and
        /// searching narrows the rows.
        /// </summary>
        private void FillInvoices(DataTable table)
        {
            PublicMethods.dgvFiller(dgvInvoices, table, AmountColumn);
        }

        /// <summary>
        /// Puts the three amounts back to nothing. Written once because saving,
        /// printing and discarding the draft each used to spell it out slightly
        /// differently, and discarding left the discount showing a figure that
        /// belonged to the lines just thrown away.
        /// </summary>
        private void ResetTotals()
        {
            lblTotalPrice.Content = Money.Display(0m);
            lblOff.Content = Money.Display(0m);
            lblFinalPrice.Content = Money.Display(0m);
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
                    lblOff.Content = Money.Display(discount);
                    lblFinalPrice.Content = Money.Display(Pricing.ComputePayable(total, discount));
                    lblOffError.Content = "";
                    return off.Code;
                }
                else
                {
                    lblOffError.Content = result;
                    lblOff.Content = Money.Display(0m);
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
            lstResult.Items.Add(cbProduct.Name + " به ارزش " + Money.DisplayWithCurrency(cbProduct.SalePrice) + " "
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
                lblTotalPrice.Content = Money.Display(sum);
                if (txtOff.Text != string.Empty)
                {
                    countOff();
                }
                else lblFinalPrice.Content = Money.Display(sum);
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

            if (!AccessGuard.Can(u, Section.Payments, Operation.Create))
            {
                miRecordPayment.IsEnabled = false;
            }
            else
            {
                miRecordPayment.IsEnabled = true;
            }

            txtCustomer.ItemsSource = Cbll.ReadPhoneNumbers();
            txtProduct.ItemsSource = Pbll.ReadNames();
            lblDate.Content = DateTime.Now.Date.ToString("yyyy/MM/dd");
            lblCount.Content = Ibll.CountInvoices();
            FillInvoices(Ibll.Read());
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
                    savedInvoice = Ibll.Create(invoice, cbCustomer.id, draftLines.ToList(), null);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
MessageBox.Show($"فاکتور با شماره {savedInvoice.id} ثبت شد", "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
FillInvoices(Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
                dgvProduts.ItemsSource = null;
                draftLines.Clear();
                lstResult.Items.Clear();
                ResetTotals();
                txtOff.Clear();
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
            ResetTotals();
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
                FillInvoices(Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
            }

        }

private void miRecordPayment_Click(object sender, RoutedEventArgs e)
        {
            if (!AccessGuard.Can(u, Section.Payments, Operation.Create))
            {
                MessageBox.Show("شما به ثبت وصولی دسترسی ندارید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (invoiceEdit != null && invoiceEdit.id != 0)
            {
                PaymentsForm form = new PaymentsForm(invoiceEdit.id);
                form.ShowDialog();
                FillInvoices(Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
            }
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

        private void btnInvoiceDetails_Click(object sender, RoutedEventArgs e)
        {
            if (!AccessGuard.Can(u, Section.Invoices, Operation.View))
            {
                return;
            }

            if (!((sender as FrameworkElement)?.DataContext is DataRowView row))
            {
                return;
            }

            int invoiceId = Convert.ToInt32(row.Row[InvoiceNumberColumn]);
            InvoiceDetailsForm details = new InvoiceDetailsForm(invoiceId);
            details.ShowDialog();
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
                    savedInvoice = Ibll.Create(invoice, cbCustomer.id, draftLines.ToList(), null);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
MessageBox.Show($"فاکتور با شماره {savedInvoice.id} ثبت شد", "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
FillInvoices(Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
                try
                {
                    string invNum = savedInvoice.id.ToString();
                    var reportModel = InvoiceReportModelFactory.FromInvoice(savedInvoice);

                    var doc = new InvoiceDocument(reportModel);
                    ReportViewerService.OpenReportPdf(doc, $"Invoice_{invNum}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطا در چاپ فاکتور:\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                dgvProduts.ItemsSource = null;
                draftLines.Clear();
                lstResult.Items.Clear();
                ResetTotals();
                txtOff.Clear();
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
                FillInvoices(Ibll.Search(txtSearch.Text));
            }
            else FillInvoices(Ibll.Read());  
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
