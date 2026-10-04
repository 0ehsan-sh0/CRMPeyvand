using BE;
using Section = BE.Section;
using BLL;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CRMPeyvand
{
    /// <summary>
    /// The «دریافت‌ها» ledger: record money received against an invoice, print the
    /// receipt, void a mistake.
    ///
    /// A payment is never edited. It may already have a printed receipt in the
    /// customer's hand, so a correction is a void plus a fresh payment.
    /// </summary>
    public partial class PaymentsForm : Window
    {
        /// <summary>The grid is the source of invoices, so 0 means "not chosen yet".</summary>
        private const int NoInvoice = 0;

        /// <summary>
        /// The DAL's own name for the money column, so the grid groups the column
        /// the query produced rather than a name repeated here. Every fill has to
        /// name it again: a grid generated from a DataTable throws its columns
        /// away and rebuilds them each time, and searching narrows the rows.
        /// </summary>
        private const string AmountColumn = "مبلغ";

        /// <summary>
        /// The two states the invoice label can be in. Kept as fields so the
        /// normal colour does not have to be rebuilt on every lookup.
        /// </summary>
        private static readonly Brush InfoBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0x8F, 0xE7));
        private static readonly Brush ProblemBrush = Brushes.DarkRed;

        private readonly int preselectedInvoiceId;
        private Invoice selectedInvoice;
        private Payment paymentEdit;
        private Payment lastRecorded;
        private User u = new User();

        private readonly PaymentBLL Pbll = new PaymentBLL();
        private readonly InvoiceBLL Ibll = new InvoiceBLL();

        public PaymentsForm() : this(NoInvoice)
        {
        }

        /// <summary>Opened from an unpaid invoice row, with that invoice filled in.</summary>
        public PaymentsForm(int invoiceId)
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
            this.preselectedInvoiceId = invoiceId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;

            btnAdd.IsEnabled = AccessGuard.Can(u, Section.Payments, Operation.Create);
            btnPrintReceipt.IsEnabled = AccessGuard.Can(u, Section.Payments, Operation.View);
            miPrint.IsEnabled = AccessGuard.Can(u, Section.Payments, Operation.View);
            miVoid.IsEnabled = AccessGuard.Can(u, Section.Payments, Operation.Delete);

            // Captions rather than enum values, because the caption is what the
            // employee reads. The order matches SelectedInstrument below.
            cbInstrument.ItemsSource = new[]
            {
                PaymentInstrumentTitles.Of(PaymentInstrument.Cash),
                PaymentInstrumentTitles.Of(PaymentInstrument.Card),
                PaymentInstrumentTitles.Of(PaymentInstrument.Transfer),
                PaymentInstrumentTitles.Of(PaymentInstrument.Cheque),
                PaymentInstrumentTitles.Of(PaymentInstrument.Other),
            };
            cbInstrument.SelectedIndex = 0;

            dpPaymentDate.SelectedDate = DateTime.Today;

            if (preselectedInvoiceId != NoInvoice)
            {
                txtInvoiceNumber.Text = preselectedInvoiceId.ToString();
                LoadInvoice();
            }

            FillPayments(Pbll.Read());
            lblCount.Content = Pbll.Count();
        }

        private void FillPayments(DataTable table)
        {
            PublicMethods.dgvFiller(dgvPayments, table, AmountColumn);
        }

        /// <summary>
        /// Resolves the typed invoice number and shows what is still owed on it, so
        /// the amount can default to the whole remainder instead of being typed
        /// blind.
        ///
        /// Called when the field loses focus or on Enter, never per keystroke. A
        /// lookup on every character told the user an invoice did not exist while
        /// they were still typing its number, one digit at a time — typing "12"
        /// produced two "not found" dialogs, and a query per character besides.
        ///
        /// The outcome goes in the label beside the field rather than a MessageBox,
        /// so a mistyped digit does not throw a modal at someone mid-entry, and the
        /// typed number is left alone so it can be corrected.
        /// </summary>
        private void LoadInvoice()
        {
            selectedInvoice = null;
            txtAmount.Clear();

            if (!int.TryParse(txtInvoiceNumber.Text, out int invoiceId))
            {
                // Empty or half-typed is not an error yet, so say nothing.
                lblInvoiceInfo.Content = "";
                return;
            }

            var invoice = Ibll.ReadDetails(invoiceId);
            if (invoice == null || invoice.DeleteStatus)
            {
                SetInvoiceProblem("فاکتور مورد نظر یافت نشد");
                return;
            }

            selectedInvoice = invoice;

            if (invoice.IsSettled)
            {
                SetInvoiceProblem("این فاکتور تسویه شده است");
                return;
            }

            lblInvoiceInfo.Foreground = InfoBrush;
            lblInvoiceInfo.Content =
                $"مشتری: {invoice.Customer?.Name} - مانده حساب: {Money.Display(invoice.Balance)}";

            txtAmount.Text = Money.Group(Money.ToToman(invoice.Balance).ToString("0"));
        }

        private void SetInvoiceProblem(string message)
        {
            lblInvoiceInfo.Foreground = ProblemBrush;
            lblInvoiceInfo.Content = message;
        }

        private void txtInvoiceNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            PublicMethods.FilterNumber(textBox);
        }

        private void txtInvoiceNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            LoadInvoice();
        }

        /// <summary>
        /// Enter resolves the invoice without leaving the field, so a cashier who
        /// types the number and reaches for the amount box is not made to tab away
        /// first to find out whether the number was any good.
        /// </summary>
        private void txtInvoiceNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadInvoice();
                txtAmount.Focus();
                e.Handled = true;
            }
        }

        private void txtAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            PriceField.GroupAsTyped(textBox);
        }

        private void txtAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (PriceField.Backspace(textBox, e.Key))
            {
                e.Handled = true;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!AccessGuard.Can(u, Section.Payments, Operation.Create))
            {
                return;
            }

            if (selectedInvoice == null)
            {
                // Clicking the button takes focus off the field, so LostFocus has
                // normally resolved the number already. This covers the paths where
                // it did not - notably Enter, which resolves without leaving.
                LoadInvoice();
            }

            if (selectedInvoice == null || selectedInvoice.IsSettled)
            {
                MessageBox.Show("لطفا شماره فاکتوری که هنوز تسویه نشده را وارد کنید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // The field is grouped for reading, so the separators come back off
            // before it is a number again.
            if (Money.IsTooLarge(txtAmount.Text))
            {
                MessageBox.Show("مبلغ وصولی بیش از حد مجاز است", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? amount = Money.ParseWhole(txtAmount.Text);
            if (!amount.HasValue)
            {
                MessageBox.Show("مبلغ وصولی را وارد کنید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var payment = new Payment
            {
                // The field is Toman because that is what the label says; the
                // database is Rial. Every other money figure in this form is
                // derived from the invoice and so is already in stored units.
                Amount = Money.FromToman(amount.Value),
                RegDate = dpPaymentDate.SelectedDate ?? DateTime.Today,
                Instrument = SelectedInstrument(),
                Reference = string.IsNullOrWhiteSpace(txtReference.Text) ? null : txtReference.Text.Trim(),
                User = u,
            };

            try
            {
                string result = Pbll.Create(payment, selectedInvoice.id);
                MessageBox.Show(result, "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);

                lastRecorded = Pbll.ReadById(payment.Id);
                ResetEntry();
                LoadInvoice();
                FillPayments(Pbll.Read());
                lblCount.Content = Pbll.Count();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private PaymentInstrument SelectedInstrument()
        {
            switch (cbInstrument.SelectedIndex)
            {
                case 1: return PaymentInstrument.Card;
                case 2: return PaymentInstrument.Transfer;
                case 3: return PaymentInstrument.Cheque;
                case 4: return PaymentInstrument.Other;
                default: return PaymentInstrument.Cash;
            }
        }

        private void ResetEntry()
        {
            txtAmount.Clear();
            txtReference.Clear();
            dpPaymentDate.SelectedDate = DateTime.Today;
            cbInstrument.SelectedIndex = 0;
            paymentEdit = null;
            txtInvoiceNumber.Focus();
        }

        private void btnPrintReceipt_Click(object sender, RoutedEventArgs e)
        {
            Payment target = lastRecorded ?? paymentEdit;
            if (target == null)
            {
                MessageBox.Show("ابتدا یک وصولی را از جدول انتخاب کنید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PrintReceipt(target);
        }

        private void PrintReceipt(Payment payment)
        {
            try
            {
                var model = PaymentReportModelFactory.FromPayment(payment);
                ReportViewerService.OpenReportPdf(new PaymentReceiptDocument(model), $"Receipt_{model.ReceiptNumber}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در چاپ رسید دریافت:\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text != string.Empty)
            {
                FillPayments(Pbll.Search(txtSearch.Text));
            }
            else FillPayments(Pbll.Read());
        }

        private void dgvPayments_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            string receipt = PublicMethods.ReadTheEntityCode(dgvPayments, 0);
            if (receipt != null)
            {
                dgvPayments.ContextMenu.IsEnabled = true;
                paymentEdit = Pbll.ReadById(Convert.ToInt32(receipt));
            }
            else
            {
                dgvPayments.ContextMenu.IsEnabled = false;
                paymentEdit = null;
            }
        }

        private void miPrint_Click(object sender, RoutedEventArgs e)
        {
            if (paymentEdit != null && AccessGuard.Can(u, Section.Payments, Operation.View))
            {
                PrintReceipt(paymentEdit);
            }
        }

        private void miVoid_Click(object sender, RoutedEventArgs e)
        {
            if (!AccessGuard.Can(u, Section.Payments, Operation.Delete) || paymentEdit == null)
            {
                return;
            }

            MessageBoxResult confirmation = MessageBox.Show(
                "آیا از ابطال این وصولی مطمئن هستید ؟", "هشدار",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (confirmation == MessageBoxResult.Yes)
            {
                MessageBox.Show(Pbll.Void(paymentEdit.Id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                paymentEdit = null;
                LoadInvoice();
                FillPayments(Pbll.Read());
                lblCount.Content = Pbll.Count();
            }
        }

        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
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