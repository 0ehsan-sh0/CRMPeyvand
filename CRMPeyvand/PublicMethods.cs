using BE;
using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace CRMPeyvand
{
    static class PublicMethods
    {
        public static void ChangeToPersianCulture()
        {
            PersianCulture culture = new PersianCulture();
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            ConfigHelper.Instance.SetLang(culture.IetfLanguageTag);
        }
        public static void DGVAutoSizeColumnFill(DataGrid d)
        {
            if (d.ItemsSource != null)
            {
                d.MinColumnWidth = (d.ActualWidth / d.Columns.Count) - 3;
                d.MaxColumnWidth = (d.ActualWidth / d.Columns.Count) - 3;
            }
        }
        public static void CheckBoxMouseEnter(Border border)
        {
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 211, 149));
        }
        public static void CheckBoxMouseLeave(Border border)
        {
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 239, 169));
        }
        public static void dgvFiller(DataGrid dg, DataTable dt)
        {
            dg.DataContext = null;
            dg.ItemsSource = dt.DefaultView;
            dg.AutoGenerateColumns = true;
            dg.CanUserAddRows = false;
            DGVAutoSizeColumnFill(dg);
        }

        public static string ReadTheEntityCode(DataGrid d, int index)
        {
            DataRowView row = (DataRowView)d.CurrentItem;
            if (row != null)
            {
                return row.Row.ItemArray[index].ToString();

            }
            else return null;
        }
        public static void FilterNumber(TextBox t)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(t.Text, "[^0-9]"))
            {
                t.Text = t.Text.Remove(t.Text.Length - 1);
            }
        }
        public static void FilterPersian(TextBox t)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(t.Text, "[^0-9]") || !t.Text.IsPersian())
            {
                t.Clear();
            }
        }

    }
}
