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
    /// Interaction logic for ProductForm.xaml
    /// </summary>
    public partial class ProductForm : Window
    {
        public ProductForm()
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
        }
        CatalogItemBLL bll = new CatalogItemBLL();
        CatalogItem productEdit = new CatalogItem();
        string name;
        UserBLL Ubll = new UserBLL();
        User u = new User();
        bool Add;

        /// <summary>
        /// The DAL's own name for the price column, so that the grid groups the
        /// column the query produced rather than a name repeated here.
        /// </summary>
        private const string PriceColumn = "قیمت";
        private void IsService_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (IsServiceImage.Visibility == Visibility.Visible)
            {
                IsServiceImage.Visibility = Visibility.Hidden;
                txtTotal.IsEnabled = true;
            }
            else if (IsServiceImage.Visibility == Visibility.Hidden)
            {
                IsServiceImage.Visibility = Visibility.Visible;
                txtTotal.IsEnabled = false;
            }

        }
        private void IsService_MouseEnter(object sender, MouseEventArgs e)
        {
            IsService.BorderBrush = new SolidColorBrush(Color.FromRgb(68, 229, 231));
        }

        private void IsService_MouseLeave(object sender, MouseEventArgs e)
        {
            IsService.BorderBrush = new SolidColorBrush(Color.FromRgb(115, 251, 211));
        }
        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            ItemKind kind = ItemKind.Good;
            int total = 0;
            if (IsServiceImage.Visibility == Visibility.Visible)
            {
                kind = ItemKind.Service;
                total = 1;
            }
            else if (IsServiceImage.Visibility == Visibility.Hidden)
            {
                kind = ItemKind.Good;
                total = Convert.ToInt32(txtTotal.Text);
            }
            // What the field shows is grouped for reading, so the separators have
            // to come back off before it is a number again.
            int? price = Money.ParseWhole(txtPrice.Text);
            CatalogItem product = new CatalogItem()
            {
                Name = txtName.Text,
                Stock = total,
                Kind = kind
            };
            if (txtName.Text != "" && price.HasValue && (product.Kind == ItemKind.Service || txtTotal.Text != ""))
            {
                if (btnAddProduct.Content.ToString() == "ثبت کالا")
                {
                    product.SalePrice = Money.FromToman(price.Value);
                    MessageBox.Show(bll.Create(product), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtName.Clear();
                    txtPrice.Clear();
                    txtTotal.Clear();
                    FillProducts(bll.Read());
                    ProductCount.Content = bll.ProductsCount();
                    txtName.Focus();
                }
                else if (btnAddProduct.Content.ToString() == "ویرایش کالا")
                {
                    product.SalePrice = Money.FromToman(price.Value);
                    MessageBox.Show(bll.Update(product, productEdit.Id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtName.Clear();
                    txtPrice.Clear();
                    txtTotal.Clear();
                    txtTotal.IsEnabled = true;
                    IsServiceImage.Visibility = Visibility.Hidden;
                    FillProducts(bll.Read());
                    btnAddProduct.Content = "ثبت کالا";
                    btnAddProduct.IsEnabled = Add;
                    txtName.Focus();
                }
            }
            else MessageBox.Show("لطفا تمامی فیلد هارا پر کنید");
        }

        /// <summary>
        /// Every fill has to name the price column again: a grid generated from a
        /// DataTable throws its columns away and rebuilds them each time, and
        /// searching narrows the rows.
        /// </summary>
        private void FillProducts(DataTable table)
        {
            PublicMethods.dgvFiller(dgvProducts, table, PriceColumn);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!AccessGuard.Can(u, Section.CatalogItems, Operation.Create))
            {
                btnAddProduct.IsEnabled = false;
                Add = false;
            }
            else
            {
                btnAddProduct.IsEnabled = true;
                Add = true;
            }
            if (!AccessGuard.Can(u, Section.CatalogItems, Operation.Edit))
            {
                miEdit.IsEnabled = false;
            }
            else
            {
                miEdit.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.CatalogItems, Operation.Delete))
            {
                miDelete.IsEnabled = false;
            }
            else
            {
                miDelete.IsEnabled = true;
            }
            FillProducts(bll.Read());
            PublicMethods.DGVAutoSizeColumnFill(dgvProducts);
            ProductCount.Content = bll.ProductsCount();
        }

        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                bll.Delete(productEdit.Id);
                FillProducts(bll.Read());
                ProductCount.Content = bll.ProductsCount();
            }
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = productEdit.Name;
            txtPrice.Text = Money.Display(productEdit.SalePrice);
            txtTotal.Text = productEdit.Stock.ToString();
            if (productEdit.Kind == ItemKind.Service)
            {
                IsServiceImage.Visibility = Visibility.Visible;
                txtTotal.IsEnabled = false;
            }
            else if (productEdit.Kind == ItemKind.Good)
            {
                IsServiceImage.Visibility = Visibility.Hidden;
                txtTotal.IsEnabled = true;
            }
            btnAddProduct.Content = "ویرایش کالا";
            btnAddProduct.IsEnabled = true;
        }

        private void dgvProducts_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvProducts, 0) != null)
            {
                dgvProducts.ContextMenu.IsEnabled = true;
                name = PublicMethods.ReadTheEntityCode(dgvProducts, 0);
                productEdit = bll.ReadByName(name);
            }
            else
            {
                dgvProducts.ContextMenu.IsEnabled = false;
            }

        }

        private void txtTotal_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t.Text == "0")
            {
                t.Clear();
            }
        }

        private void txtSearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchProduct.Text != String.Empty)
            {
                FillProducts(bll.Search(txtSearchProduct.Text));
            }
            else FillProducts(bll.Read());
        }

        private void txtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Not FilterNumber, which would strip the separator it is meant to be
            // showing.
            PriceField.GroupAsTyped(sender as TextBox);
        }

        private void txtPrice_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (PriceField.Backspace(sender as TextBox, e.Key))
            {
                e.Handled = true;
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
