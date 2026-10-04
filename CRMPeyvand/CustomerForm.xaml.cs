using BE;
using Section = BE.Section;
using BLL;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for CustomerForm.xaml
    /// </summary>
    public partial class CustomerForm : Window
    {
        public CustomerForm()
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
        }
        CustomerBLL bll = new CustomerBLL();
        string phone;
        Customer customer = new Customer();
        UserBLL Ubll = new UserBLL();
        User u = new User();

        /// <summary>
        /// The DAL's own name for the balance column, so the grid groups the
        /// column the query produced rather than a name repeated here. Every fill
        /// has to name it again: a grid generated from a DataTable throws its
        /// columns away and rebuilds them each time, and searching narrows the
        /// rows.
        /// </summary>
        private const string BalanceColumn = "مانده حساب";

        private void FillCustomers(DataTable table)
        {
            PublicMethods.dgvFiller(dgvCustomer, table, BalanceColumn);
        }

        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!AccessGuard.Can(u, Section.Customers, Operation.Create))
            {
                btnAddProduct.IsEnabled = false;
            }
            else
            {
                btnAddProduct.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Customers, Operation.Edit))
            {
                EditCustomerMI.IsEnabled = false;
            }
            else
            {
                EditCustomerMI.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Customers, Operation.Delete))
            {
                DeleteCustomer.IsEnabled = false;
            }
            else
            {
                DeleteCustomer.IsEnabled = true;
            }
            FillCustomers(bll.Read());
            Count.Content = bll.CustomerCount();
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            Customer c = new Customer()
            {
                Name = txtName.Text,
                Phone = txtPhone.Text,
                RegDate = DateTime.Now
            };
            if (txtName.Text != "" && txtPhone.Text.Length == 11)
            {
                if (btnAddProduct.Content.ToString() == "ثبت مشتری")
                {
                    MessageBox.Show(bll.Create(c), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    FillCustomers(bll.Read());
                    txtName.Clear();
                    txtPhone.Clear();
                    Count.Content = bll.CustomerCount();
                    txtName.Focus();
                }
                else if (btnAddProduct.Content.ToString() == "ویرایش مشتری")
                {
                    MessageBox.Show(bll.Update(c, customer.id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    FillCustomers(bll.Read());
                    btnAddProduct.Content = "ثبت مشتری";
                    if (!AccessGuard.Can(u, Section.Customers, Operation.Create))
                    {
                        btnAddProduct.IsEnabled = false;
                    }
                    else
                    {
                        btnAddProduct.IsEnabled = true;
                    }
                    txtName.Clear();
                    txtPhone.Clear();
                    txtName.Focus();

                }
            }
            else
            {
                MessageBox.Show("لطفا تمامی فیلد هارا پر کنید و شماره تلفن را یازده رقمی وارد کنید", "اطلاعیه");
            }
        }

        private void txtSearchCustomer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchCustomer.Text != String.Empty)
            {
                FillCustomers(bll.Search(txtSearchCustomer.Text));
            }
            else FillCustomers(bll.Read());
        }

        private void EditCustomerMI_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = customer.Name;
            txtPhone.Text = customer.Phone;
            btnAddProduct.Content = "ویرایش مشتری";
            btnAddProduct.IsEnabled = true;
        }

        private void dgvCustomer_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvCustomer, 1) != null)
            {
                dgvCustomer.ContextMenu.IsEnabled = true;
                phone = PublicMethods.ReadTheEntityCode(dgvCustomer, 1);
                customer = bll.GetCustomer(phone);
            }
            else
            {
                dgvCustomer.ContextMenu.IsEnabled = false;
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                bll.Delete(customer.id);
                Count.Content = bll.CustomerCount();
                FillCustomers(bll.Read());
            }

        }

        private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            PublicMethods.FilterNumber(t);
            lblPhoneHint.Visibility = t.Text == string.Empty ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            PublicMethods.FilterPersian(t);
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

