using BE;
using Section = BE.Section;
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
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for ActivitiesForm.xaml
    /// </summary>
    public partial class ActivitiesForm : Window
    {
        public ActivitiesForm()
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
        }
        ActivityBLL Abll = new ActivityBLL();
        UserBLL Ubll = new UserBLL();
        ActivityCategoryBLL ACbll = new ActivityCategoryBLL();
        CustomerBLL Cbll = new CustomerBLL();
        ReminderBLL Rbll = new ReminderBLL();
        User cbUser = new User();
        Customer cbCustomer = new Customer();
        ActivityCategory cbActivityCategory = new ActivityCategory();
        Activity activityEdit = new Activity();
        User u = new User();
        bool Add = false;
        void AddEdit()
        {
            Activity activity = new Activity();
            activity.Title = txtActivityTopic.Text;
            activity.Info = txtActivityInfo.Text;
            activity.User = cbUser;
            activity.Customer = cbCustomer;
            activity.ActivityCategory = cbActivityCategory;
            if (txtActivityTopic.Text != "" && cbUser != null && cbCustomer != null && cbActivityCategory != null)
            {
                if (btnAdd.Content.ToString() == "ثبت یادآور")
                {
                    activity.RegDate = DateTime.Now;
                    MessageBox.Show(Abll.Create(activity), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    PublicMethods.dgvFiller(dgvActivities, Abll.Read());
                    if (IsChecked4Image.Visibility == Visibility.Visible)
                    {
                        Reminder reminder = new Reminder();
                        reminder.Title = txtActivityTopic.Text;
                        reminder.Info = txtActivityInfo.Text;
                        reminder.RegDate = DateTime.Now;
                        reminder.RemindDate = DatePicker.SelectedDate.Value;
                        reminder.User = cbUser;
                        MessageBox.Show(Rbll.Create(r: reminder, u: reminder.User), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                        IsChecked4Image.Visibility = Visibility.Hidden;
                        DatePicker.SelectedDate = DateTime.Now;
                        DatePicker.IsEnabled = false;
                    }
                    lblCount.Content = Abll.Count();
                    txtActivityTopic.Clear();
                    txtActivityInfo.Clear();
                    txtUser.Text = "";
                    txtCustomer.Text = "";
                    txtCategory.Text = "";
                    cbUser = null;
                    cbCustomer = null;
                    cbActivityCategory = null;
                    txtCustomer.Focus();

                }
                else
                {
                    MessageBox.Show(Abll.Update(activity, activityEdit.id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (IsChecked4Image.Visibility == Visibility.Visible)
                    {
                        Reminder reminder = new Reminder();
                        reminder.Title = txtActivityTopic.Text;
                        reminder.Info = txtActivityInfo.Text;
                        reminder.RegDate = DateTime.Now;
                        reminder.RemindDate = DatePicker.SelectedDate.Value;
                        reminder.User = cbUser;
                        MessageBox.Show(Rbll.Create(r: reminder, u: reminder.User), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                        IsChecked4Image.Visibility = Visibility.Hidden;
                        DatePicker.SelectedDate = DateTime.Now;
                        DatePicker.IsEnabled = false;
                    }
                    btnAdd.Content = "ثبت یادآور";
                    btnAdd.IsEnabled = Add;
                    txtActivityTopic.Clear();
                    txtActivityInfo.Clear();
                    txtUser.Text = "";
                    txtCustomer.Text = "";
                    txtCategory.Text = "";
                    cbUser = null;
                    cbCustomer = null;
                    cbActivityCategory = null;
                    txtCustomer.Focus();
                    PublicMethods.dgvFiller(dgvActivities, Abll.Read());
                }
            }
            else MessageBox.Show("لطفا تمامی فیلد هارا پر کنید");

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
        private void IsChecked4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsChecked4Image.Visibility == Visibility.Visible)
            {
                IsChecked4Image.Visibility = Visibility.Hidden;
                DatePicker.IsEnabled = false;
            }
            else if (IsChecked4Image.Visibility == Visibility.Hidden)
            {
                IsChecked4Image.Visibility = Visibility.Visible;
                DatePicker.IsEnabled = true;
            }

        }

        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!AccessGuard.Can(u, Section.Activities, Operation.Create))
            {
                btnAdd.IsEnabled = false;
                Add = false;
            }
            else
            {
                btnAdd.IsEnabled = true;
                Add = true;
            }
            if (!AccessGuard.Can(u, Section.Activities, Operation.Edit))
            {
                miEdit.IsEnabled = false;
            }
            else
            {
                miEdit.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Activities, Operation.Delete))
            {
                miDelete.IsEnabled = false;
            }
            else
            {
                miDelete.IsEnabled = true;
            }
            PublicMethods.dgvFiller(dgvActivities, Abll.Read());
            txtUser.ItemsSource = Ubll.ReadUserNamesList();
            txtCustomer.ItemsSource = Cbll.ReadPhoneNumbers();
            txtCategory.ItemsSource = ACbll.ActivityCategoryReadNames();
            lblCount.Content = Abll.Count();
            DatePicker.IsEnabled = false;
            PublicMethods.DGVAutoSizeColumnFill(dgvActivities);

        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                Abll.Delete(activityEdit.id);
                lblCount.Content = Abll.Count();
                PublicMethods.dgvFiller(dgvActivities, Abll.Read());
            }

        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            txtActivityTopic.Text = activityEdit.Title;
            txtActivityInfo.Text = activityEdit.Info;
            txtUser.Text = activityEdit.User.UserName;
            cbUser = activityEdit.User;
            txtCustomer.Text = activityEdit.Customer.Phone;
            cbCustomer = activityEdit.Customer;
            txtCategory.Text = activityEdit.ActivityCategory.CategoryName;
            cbActivityCategory = activityEdit.ActivityCategory;
            btnAdd.Content = "ویرایش";
            btnAdd.IsEnabled = true;
        }

        private void dgvActivities_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvActivities, 0) != null)
            {
                dgvActivities.ContextMenu.IsEnabled = true;
                int id = Convert.ToInt32(PublicMethods.ReadTheEntityCode(dgvActivities, 0));
                activityEdit = Abll.ReadById(id);
            }
            else
            {
                dgvActivities.ContextMenu.IsEnabled = false;
            }

        }

        private void txtCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtCustomer.SelectedItem != null)
            {
                cbCustomer = Cbll.GetCustomer(txtCustomer.SelectedItem.ToString());
            }

        }

        private void txtUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtUser.SelectedItem != null)
            {
                cbUser = Ubll.ReadByUserName(txtUser.SelectedItem.ToString());
            }

        }

        private void txtCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtCategory.SelectedItem != null)
            {
                cbActivityCategory = ACbll.ReadByName(txtCategory.SelectedItem.ToString());
            }

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEdit();
        }

        private void txtSearchActivities_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchActivities.Text != String.Empty)
            {
                PublicMethods.dgvFiller(dgvActivities, Abll.Search(txtSearchActivities.Text));
            }
            else PublicMethods.dgvFiller(dgvActivities, Abll.Read());

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
                case Key.Enter:
                    AddEdit();
                    break;
            }
        }
    }
}
