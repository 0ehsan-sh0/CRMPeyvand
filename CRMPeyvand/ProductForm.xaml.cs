using BE;
using BLL;
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
        ProductBLL bll = new ProductBLL();
        Product productEdit = new Product();
        string name;
        UserBLL Ubll = new UserBLL();
        User u = new User();
        bool Add;
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
            string ProductType = "";
            int total = 0;
            if (IsServiceImage.Visibility == Visibility.Visible)
            {
                ProductType = "خدمات";
                total = 1;
            }
            else if (IsServiceImage.Visibility == Visibility.Hidden)
            {
                ProductType = "محصول";
                total = Convert.ToInt32(txtTotal.Text);
            }
            Product product = new Product()
            {
                Name = txtName.Text,
                Total = total,
                Type = ProductType
            };
            if ((txtName.Text != "" && txtTotal.Text != "" && txtPrice.Text != "" && product.Type == "محصول") || (txtName.Text != "" && txtPrice.Text != "" && product.Type == "خدمات"))
            {
                if (btnAddProduct.Content.ToString() == "ثبت کالا")
                {
                    product.Price = Convert.ToInt32(txtPrice.Text);
                    MessageBox.Show(bll.Create(product), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtName.Clear();
                    txtPrice.Clear();
                    txtTotal.Clear();
                    PublicMethods.dgvFiller(dgvProducts, bll.Read());
                    ProductCount.Content = bll.ProductsCount();
                    txtName.Focus();
                }
                else if (btnAddProduct.Content.ToString() == "ویرایش کالا")
                {
                    product.Price = Convert.ToInt32(txtPrice.Text);
                    MessageBox.Show(bll.Update(product, productEdit.id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtName.Clear();
                    txtPrice.Clear();
                    txtTotal.Clear();
                    txtTotal.IsEnabled = true;
                    IsServiceImage.Visibility = Visibility.Hidden;
                    PublicMethods.dgvFiller(dgvProducts, bll.Read());
                    btnAddProduct.Content = "ثبت کالا";
                    btnAddProduct.IsEnabled = Add;
                    txtName.Focus();
                }
            }
            else MessageBox.Show("لطفا تمامی فیلد هارا پر کنید");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!Ubll.Access(u, "بخش کالاها", 2))
            {
                btnAddProduct.IsEnabled = false;
                Add = false;
            }
            else
            {
                btnAddProduct.IsEnabled = true;
                Add = true;
            }
            if (!Ubll.Access(u, "بخش کالاها", 3))
            {
                miEdit.IsEnabled = false;
            }
            else
            {
                miEdit.IsEnabled = true;
            }
            if (!Ubll.Access(u, "بخش کالاها", 4))
            {
                miDelete.IsEnabled = false;
            }
            else
            {
                miDelete.IsEnabled = true;
            }
            PublicMethods.dgvFiller(dgvProducts, bll.Read());
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
                bll.Delete(productEdit.id);
                PublicMethods.dgvFiller(dgvProducts, bll.Read());
                ProductCount.Content = bll.ProductsCount();
            }
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = productEdit.Name;
            txtPrice.Text = productEdit.Price.ToString();
            txtTotal.Text = productEdit.Total.ToString();
            if (productEdit.Type == "خدمات")
            {
                IsServiceImage.Visibility = Visibility.Visible;
                txtTotal.IsEnabled = false;
            }
            else if (productEdit.Type == "محصول")
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
                PublicMethods.dgvFiller(dgvProducts, bll.Search(txtSearchProduct.Text));
            }
            else PublicMethods.dgvFiller(dgvProducts, bll.Read());
        }

        private void txtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            PublicMethods.FilterNumber(t);
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
