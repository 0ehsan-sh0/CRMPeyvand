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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for ReminderForm.xaml
    /// </summary>
    public partial class ReminderForm : Window
    {
        public ReminderForm()
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
        }
        ReminderBLL Rbll = new ReminderBLL();
        UserBLL Ubll = new UserBLL();
        User cbUser = new User();
        Reminder EditReminder = new Reminder();
        User u = new User();
        bool Add;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;
            if (!AccessGuard.Can(u, Section.Reminders, Operation.Create))
            {
                btnAddReminder.IsEnabled = false;
                Add = false;
            }
            else
            {
                btnAddReminder.IsEnabled = true;
                Add = true;
            }
            if (!AccessGuard.Can(u, Section.Reminders, Operation.Edit))
            {
                miEdit.IsEnabled = false;
            }
            else
            {
                miEdit.IsEnabled = true;
            }
            if (!AccessGuard.Can(u, Section.Reminders, Operation.Delete))
            {
                miDelete.IsEnabled = false;
            }
            else
            {
                miDelete.IsEnabled = true;
            }

            PublicMethods.dgvFiller(dgvReminders, Rbll.Read());
            txtUser.ItemsSource = Ubll.ReadUserNamesList();
            lblCount.Content = Rbll.Count();
            PublicMethods.DGVAutoSizeColumnFill(dgvReminders);

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

        private void txtUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtUser.SelectedItem != null)
            {
                cbUser = Ubll.ReadByUserName(txtUser.SelectedItem.ToString());
            }
        }

        private void btnAddReminder_Click(object sender, RoutedEventArgs e)
        {
            if (txtTopic.Text != "" && cbUser != null && DatePicker.SelectedDate != null)
            {
                if (btnAddReminder.Content.ToString() == "ثبت یادآور")
                {
                    Reminder r = new Reminder();
                    r.Title = txtTopic.Text;
                    r.Info = txtReminderInfo.Text;
                    r.RegDate = DateTime.Now;
                    r.RemindDate = DatePicker.SelectedDate.Value;
                    r.User = cbUser;
                    MessageBox.Show(Rbll.Create(r, cbUser), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    PublicMethods.dgvFiller(dgvReminders, Rbll.Read());
                    lblCount.Content = Rbll.Count();
                    txtTopic.Clear();
                    txtReminderInfo.Clear();
                    txtUser.Text = "";
                    cbUser = null;
                    DatePicker.SelectedDate = DateTime.Now;
                    txtUser.Focus();
                }
                else
                {
                    Reminder r = new Reminder();
                    r.Title = txtTopic.Text;
                    r.Info = txtReminderInfo.Text;
                    r.RemindDate = DatePicker.SelectedDate.Value;
                    r.User = cbUser;
                    btnAddReminder.Content = "ثبت یادآور";
                    btnAddReminder.IsEnabled = Add;
                    MessageBox.Show(Rbll.Update(r, EditReminder.id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                    PublicMethods.dgvFiller(dgvReminders, Rbll.Read());
                    txtTopic.Clear();
                    txtReminderInfo.Clear();
                    txtUser.Text = "";
                    cbUser = null;
                    DatePicker.SelectedDate = DateTime.Now;
                    txtUser.Focus();
                }
            }
            else
            {
                MessageBox.Show("لطفا تمامی فیلد هارا پر کنید");
            }
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            txtTopic.Text = EditReminder.Title;
            txtUser.Text = EditReminder.User.UserName;
            cbUser = EditReminder.User;
            txtReminderInfo.Text = EditReminder.Info;
            DatePicker.SelectedDate = EditReminder.RemindDate;
            btnAddReminder.Content = "ویرایش یادآور";
            btnAddReminder.IsEnabled = true;
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult DeleteConfirmation = MessageBox.Show("آیا از عملیات حذف مطمعن هستید ؟", "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (DeleteConfirmation == MessageBoxResult.Yes)
            {
                Rbll.Delete(EditReminder.id);
                lblCount.Content = Rbll.Count();
                PublicMethods.dgvFiller(dgvReminders, Rbll.Read());
            }
        }

        private void dgvReminders_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (PublicMethods.ReadTheEntityCode(dgvReminders, 0) != null)
            {
                dgvReminders.ContextMenu.IsEnabled = true;
                int id = Convert.ToInt32(PublicMethods.ReadTheEntityCode(dgvReminders, 0));
                EditReminder = Rbll.ReadById(id);

            }
            else
            {
                dgvReminders.ContextMenu.IsEnabled = false;
            }

        }

        private void txtSearchReminder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchReminder.Text != String.Empty)
            {
                PublicMethods.dgvFiller(dgvReminders, Rbll.Search(txtSearchReminder.Text));
            }
            else PublicMethods.dgvFiller(dgvReminders, Rbll.Read());
        }

        private void miDone_Click(object sender, RoutedEventArgs e)
        {
            Rbll.Done(EditReminder.id);
            PublicMethods.dgvFiller(dgvReminders, Rbll.Read());
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
