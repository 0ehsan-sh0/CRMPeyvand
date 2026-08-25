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
    /// Interaction logic for ActivityCategoryForm.xaml
    /// </summary>
    public partial class ActivityCategoryForm : Window
    {
        public ActivityCategoryForm()
        {
            InitializeComponent();
        }
        ActivityCategoryBLL bll = new ActivityCategoryBLL();
        ActivityCategory acEdit = new ActivityCategory();
        void AutoSizeColumn()
        {
            if (dgvAC.ItemsSource != null)
            {
                dgvAC.MinColumnWidth = (dgvAC.ActualWidth / (dgvAC.Columns.Count - 1)) - 3;
                dgvAC.MaxColumnWidth = (dgvAC.ActualWidth / (dgvAC.Columns.Count - 1)) - 3;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ActivityCategory a = new ActivityCategory();
            a.CategoryName = txtCategory.Text;

            if (txtCategory.Text != "")
            {
                if (btnAdd.Content.ToString() == "ثبت")
                {
                    MessageBox.Show(bll.Create(a), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    PublicMethods.dgvFiller(dgvAC, bll.Read());
                    dgvAC.Columns[0].Visibility = Visibility.Hidden;
                    AutoSizeColumn();
                    txtCategory.Text = "";
                    txtCategory.Focus();

                }
                else
                {
                    MessageBox.Show(bll.Update(a, acEdit.id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    PublicMethods.dgvFiller(dgvAC, bll.Read());
                    dgvAC.Columns[0].Visibility = Visibility.Hidden;
                    AutoSizeColumn();
                    btnAdd.Content = "ثبت";
                    txtCategory.Text = "";
                    txtCategory.Focus();
                }
            }

        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PublicMethods.dgvFiller(dgvAC, bll.Read());
            dgvAC.Columns[0].Visibility = Visibility.Hidden;
            AutoSizeColumn();
        }

        private void EditMI_Click(object sender, RoutedEventArgs e)
        {
            txtCategory.Text = acEdit.CategoryName;
            btnAdd.Content = "ویرایش";
        }

        private void DeleteMI_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                bll.Delete(acEdit.id);
                PublicMethods.dgvFiller(dgvAC, bll.Read());
                dgvAC.Columns[0].Visibility = Visibility.Hidden;
                AutoSizeColumn();
            }
        }

        private void dgvAC_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvAC, 0) != null)
            {
                dgvAC.ContextMenu.IsEnabled = true;
                int id = Convert.ToInt32(PublicMethods.ReadTheEntityCode(dgvAC, 0));
                acEdit = bll.ReadById(id);
            }
            else
            {
                dgvAC.ContextMenu.IsEnabled = false;
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
