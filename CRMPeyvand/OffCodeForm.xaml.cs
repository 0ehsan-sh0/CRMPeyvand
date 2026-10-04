using BE;
using Section = BE.Section;
using System;
using System.Collections.Generic;
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
using BLL;
using HandyControl.Tools.Extension;
using System.Xml.Linq;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for OffCodeForm.xaml
    /// </summary>
    public partial class OffCodeForm : Window
    {
        public OffCodeForm()
        {
            InitializeComponent();
        }
        OffCodeBLL bll = new OffCodeBLL();
        UserBLL Ubll = new UserBLL();
        User u = new User();
        bool isPrice1 = false;
        OffCode offcodeEdit = new OffCode();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!AccessGuard.Can(u, Section.Discounts, Operation.Create))
            {
                btnAdd.IsEnabled = false;
            }
            else
            {
                btnAdd.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Discounts, Operation.Edit))
            {
                miEdit.IsEnabled = false;
            }
            else
            {
                miEdit.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Discounts, Operation.Delete))
            {
                miDelete.IsEnabled = false;
            }
            else
            {
                miDelete.IsEnabled = true;
            }
            IsCheckedImage.Visibility = Visibility.Visible;
            txtPrice.IsEnabled = false;
            DatePicker.SelectedDate = null;
            PublicMethods.dgvFiller(dgvOffCodes, bll.Read());
            lblCount.Content = bll.OffCodeCount();
        }
        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void IsChecked_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            PublicMethods.CheckBoxMouseEnter(border);
        }

        private void IsChecked_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            PublicMethods.CheckBoxMouseLeave(border);
        }

        private void IsChecked_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsCheckedImage.Visibility == Visibility.Visible)
            {
                IsCheckedImage.Visibility = Visibility.Hidden;
                txtPrice.IsEnabled = true;
                nudPercent.IsEnabled = false;
                nudPercent.Value = 0;
                isPrice1 = true;

            }
            else if (IsCheckedImage.Visibility == Visibility.Hidden)
            {
                IsCheckedImage.Visibility = Visibility.Visible;
                txtPrice.IsEnabled = false;
                nudPercent.IsEnabled = true;
                txtPrice.Clear();
                isPrice1 = false;
            }
        }

        private void txtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            PublicMethods.FilterNumber(textBox);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Read once, here, rather than four times further down inside the
            // branches. The field can be blank - a percentage code leaves it blank on
            // purpose - and Convert.ToDecimal used to be asked about that blank field
            // and threw, so saving a percentage code landed in the global
            // "something went wrong" box instead of saving. A missing amount means
            // zero; only an amount too wide to store is refused.
            long? price = Money.ParseWhole(txtPrice.Text);
            if (Money.IsTooLarge(txtPrice.Text))
            {
                MessageBox.Show("مبلغ تخفیف بیش از حد مجاز است", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OffCode offCode = new OffCode()
            {
                RegDate = DateTime.Now,
                IsPrice = isPrice1
            };
            if (txtCode.Text != String.Empty && (nudPercent.Value != 0 || txtPrice.Text != String.Empty) && btnAdd.Content.ToString() == "ثبت کد تخفیف")
            {
                if (DatePicker.SelectedDate == null && txtLimit.Text == String.Empty)
                {
                    MessageBoxResult WithoutLimit = MessageBox.Show("آیا مطمعن هستید که میخواهید هیچ محدودیتی اعمال نکنید؟\nپیشنهاد میشود یکی از محدودیت های اختیاری را اعمال کنید", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (WithoutLimit == MessageBoxResult.Yes)
                    {
                        offCode.Code = txtCode.Text;
                        if (offCode.IsPrice)
                        {
                            offCode.Price = Money.FromToman(price ?? 0);
                        }
                        else
                        {
                            offCode.Percent = (int)nudPercent.Value;
                        }
                        MessageBox.Show(bll.Create(offCode), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                        PublicMethods.dgvFiller(dgvOffCodes, bll.Read());
                        lblCount.Content = bll.OffCodeCount();
                        txtCode.Clear();
                        txtPrice.Clear();
                        nudPercent.Value = 0;
                        txtCode.Focus();

                    }
                }
                else
                {
                    offCode.Code = txtCode.Text;
                    if (offCode.IsPrice)
                    {
                        offCode.Price = Money.FromToman(price ?? 0);
                    }
                    else
                    {
                        offCode.Percent = (int)nudPercent.Value;
                    }
                    if (DatePicker.SelectedDate != null)
                    {
                        offCode.ExpireDate = DatePicker.SelectedDate;
                    }
                    if (txtLimit.Text != String.Empty)
                    {
                        offCode.LimitCount = Convert.ToInt32(txtLimit.Text);
                    }
                    MessageBox.Show(bll.Create(offCode), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    PublicMethods.dgvFiller(dgvOffCodes, bll.Read());
                    lblCount.Content = bll.OffCodeCount();
                    txtCode.Clear();
                    txtPrice.Clear();
                    nudPercent.Value = 0;
                    txtLimit.Clear();
                    DatePicker.SelectedDate = null;
                    txtCode.Focus();
                }
            }
            else if (txtCode.Text != String.Empty && (nudPercent.Value != 0 || txtPrice.Text != String.Empty) && btnAdd.Content.ToString() == "ویرایش")
            {
                if (DatePicker.SelectedDate == null && txtLimit.Text == String.Empty)
                {
                    MessageBoxResult WithoutLimit = MessageBox.Show("آیا مطمعن هستید که میخواهید هیچ محدودیتی اعمال نکنید؟\nپیشنهاد میشود یکی از محدودیت های اختیاری را اعمال کنید", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (WithoutLimit == MessageBoxResult.Yes)
                    {
                        offCode.Code = txtCode.Text;
                        if (offCode.IsPrice)
                        {
                            offCode.Price = Money.FromToman(price ?? 0);
                        }
                        else
                        {
                            offCode.Percent = (int)nudPercent.Value;
                        }
                        MessageBox.Show(bll.Update(offCode,offcodeEdit.Code), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                        PublicMethods.dgvFiller(dgvOffCodes, bll.Read());
                        lblCount.Content = bll.OffCodeCount();
                        txtCode.Clear();
                        txtPrice.Clear();
                        nudPercent.Value = 0;
                        btnAdd.Content = "ثبت کد تخفیف";
                        txtCode.IsEnabled = true;
                        txtCode.Focus();

                    }

                }
                else
                {
                    offCode.Code = txtCode.Text;
                    if (offCode.IsPrice)
                    {
                        offCode.Price = Money.FromToman(price ?? 0);
                    }
                    else
                    {
                        offCode.Percent = (int)nudPercent.Value;
                    }
                    if (DatePicker.SelectedDate != null)
                    {
                        offCode.ExpireDate = DatePicker.SelectedDate;
                    }
                    if (txtLimit.Text != String.Empty)
                    {
                        offCode.LimitCount = Convert.ToInt32(txtLimit.Text);
                    }
                    MessageBox.Show(bll.Update(offCode, offcodeEdit.Code), "اطلاعیه",MessageBoxButton.OK,MessageBoxImage.Information);
                    PublicMethods.dgvFiller(dgvOffCodes, bll.Read());
                    lblCount.Content = bll.OffCodeCount();
                    txtCode.Clear();
                    txtPrice.Clear();
                    nudPercent.Value = 0;
                    txtLimit.Clear();
                    DatePicker.SelectedDate = null;
                    btnAdd.Content = "ثبت کد تخفیف";
                    txtCode.IsEnabled = true;
                    txtCode.Focus();

                }
            }
            else
            {
                MessageBox.Show("لطفا تمامی فیلد های ضروری را پر کنید", "اطلاعیه");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text != String.Empty)
            {
                PublicMethods.dgvFiller(dgvOffCodes, bll.Search(txtSearch.Text));
            }
            else PublicMethods.dgvFiller(dgvOffCodes, bll.Read());
        }

        private void dgvOffCodes_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvOffCodes, 0) != null)
            {
                dgvOffCodes.ContextMenu.IsEnabled = true;
                string code = PublicMethods.ReadTheEntityCode(dgvOffCodes, 0);
                offcodeEdit = bll.GetOffCode(code);
            }
            else
            {
                dgvOffCodes.ContextMenu.IsEnabled = false;
            }
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            txtCode.Text = offcodeEdit.Code;
            txtCode.IsEnabled = false;
            if (offcodeEdit.IsPrice)
            {
                IsCheckedImage.Visibility = Visibility.Hidden;
                txtPrice.IsEnabled = true;
                nudPercent.IsEnabled = false;
                txtPrice.Text = Money.Display(offcodeEdit.Price.Value);
            }
            else
            {
                IsCheckedImage.Visibility = Visibility.Visible;
                txtPrice.IsEnabled = false;
                nudPercent.IsEnabled = true;
                nudPercent.Value = (double)offcodeEdit.Percent;
            }
            if (offcodeEdit.LimitCount != null)
            {
                txtLimit.Text = offcodeEdit.LimitCount.ToString();
            }
            if (offcodeEdit.ExpireDate != null)
            {
                DatePicker.SelectedDate = offcodeEdit.ExpireDate;
            }
            btnAdd.Content = "ویرایش";
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                bll.Delete(offcodeEdit.Code);
                lblCount.Content = bll.OffCodeCount();
                PublicMethods.dgvFiller(dgvOffCodes, bll.Read());
            }
        }

        private void miCopy_Click(object sender, RoutedEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvOffCodes, 0) != null)
            {
                dgvOffCodes.ContextMenu.IsEnabled = true;
                string code = PublicMethods.ReadTheEntityCode(dgvOffCodes, 0);
                Clipboard.SetText(code);
                MessageBox.Show("کد کپی شد");
            }
            else
            {
                dgvOffCodes.ContextMenu.IsEnabled = false;
            }
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
