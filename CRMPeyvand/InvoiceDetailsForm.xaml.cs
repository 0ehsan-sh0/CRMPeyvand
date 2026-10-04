using BE;
using BLL;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using CRMPeyvand.Reports.Services;
using Section = BE.Section;
using System;
using System.Windows;
using System.Windows.Input;

namespace CRMPeyvand
{
    /// <summary>
    /// Read-only view of a single persisted invoice, with printing.
    /// </summary>
    public partial class InvoiceDetailsForm : Window
    {
        private readonly InvoiceBLL Ibll = new InvoiceBLL();
        private readonly int invoiceId;
        private Invoice invoice;
        private InvoiceReportModel model;

        public InvoiceDetailsForm(int invoiceId)
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
            this.invoiceId = invoiceId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            User currentUser = ((MainWindow)Application.Current.MainWindow).loggedInUser;
            if (!AccessGuard.Can(currentUser, Section.Invoices, Operation.View))
            {
                MessageBox.Show("شما به مشاهده فاکتور ها دسترسی ندارید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            invoice = Ibll.ReadDetails(invoiceId);
            if (invoice == null)
            {
                MessageBox.Show("فاکتور مورد نظر یافت نشد", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            model = InvoiceReportModelFactory.FromInvoice(invoice);
            ShowDetails();
        }

        private void ShowDetails()
        {
            lblNumber.Content = model.InvoiceNumber;
            lblRegDate.Content = model.IssueDatePersian;
            lblStatus.Content = invoice.IsCheckedout ? "پرداخت شده" : "پرداخت نشده";
            lblCheckoutDate.Content = invoice.CheckoutDate.HasValue
                ? PersianReportStyle.FormatPersianDate(invoice.CheckoutDate.Value)
                : "-";
            lblOffCode.Content = string.IsNullOrWhiteSpace(invoice.OffCode) ? "-" : invoice.OffCode;
            lblIssuedBy.Content = OrDefault(invoice.User?.Name ?? invoice.User?.UserName);
            lblCustomer.Content = OrDefault(model.CustomerName);
            lblPhone.Content = OrDefault(model.CustomerPhone);

            dgvLines.ItemsSource = model.Items;
            lblSubTotal.Content = model.SubTotal.ToString("N0");
            lblDiscountAmount.Content = model.DiscountAmount.ToString("N0");
            lblFinalTotal.Content = model.FinalTotal.ToString("N0");
            lblPaidAmount.Content = model.PaidAmount.ToString("N0");
            lblBalance.Content = model.RemainingBalance.ToString("N0");

            Title = $"جزئیات فاکتور {model.InvoiceNumber}";
        }

        private static string OrDefault(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReportViewerService.OpenReportPdf(new InvoiceDocument(model), $"Invoice_{model.InvoiceNumber}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در چاپ فاکتور:\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSavePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string savedPath = ReportViewerService.ExportReportPdf(new InvoiceDocument(model), $"Invoice_{model.InvoiceNumber}.pdf");
                if (savedPath != null)
                    MessageBox.Show("فایل PDF ذخیره شد:\n" + savedPath, "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره فاکتور:\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
            }
        }
    }
}
