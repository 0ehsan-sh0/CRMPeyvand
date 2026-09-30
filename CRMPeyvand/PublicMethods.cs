using BE;
using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CRMPeyvand
{
    static class PublicMethods
    {
        /// <summary>
        /// Trailing-slash path of the folder holding employee photos. Kept out of the
        /// Program Files install folder, which is read-only for standard users.
        ///
        /// %ProgramData% is preferred so that every employee shares one folder and an
        /// administrator can see everyone's photo. That folder is created by whichever
        /// user signs in first and is then writable only by them, so on a shared
        /// machine later users fall back to their own %LocalAppData%: they can still
        /// save and see their own photo, only cross-user photos go missing.
        /// </summary>
        public static string UserPicturesDirectory
        {
            get
            {
                return PictureFolderUnder(Environment.SpecialFolder.CommonApplicationData)
                    ?? PictureFolderUnder(Environment.SpecialFolder.LocalApplicationData);
            }
        }

        private static string PictureFolderUnder(Environment.SpecialFolder root)
        {
            try
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(root), "CRMPeyvand", "UserPisc");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                // Prove writability now rather than letting File.Copy fail later:
                // creating the folder succeeds even when we may not write into it.
                // Unique name so concurrent app instances cannot trip over each other.
                string probe = Path.Combine(path, ".write-test-" + Guid.NewGuid().ToString("N"));
                try
                {
                    using (File.Create(probe))
                    {
                    }
                }
                finally
                {
                    File.Delete(probe);
                }
                return path + Path.DirectorySeparatorChar;
            }
            catch (Exception)
            {
                return null;
            }
        }

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

        /// <summary>
        /// Fills a grid whose columns are generated from a DataTable, grouping the
        /// named ones.
        ///
        /// A generated column is the only kind here that has no StringFormat to
        /// hang a separator off, and the columns are thrown away and rebuilt
        /// every time the grid is refilled - searching narrows the rows - so this
        /// has to be asked for on each fill rather than once at startup.
        /// </summary>
        public static void dgvFiller(DataGrid dg, DataTable dt, params string[] moneyColumns)
        {
            dgvFiller(dg, dt);

            if (moneyColumns == null || moneyColumns.Length == 0)
            {
                return;
            }

            foreach (DataGridColumn column in dg.Columns)
            {
                if (column is not DataGridBoundColumn bound)
                {
                    continue;
                }

                // The generated header and the path it binds to are both the
                // DataTable's own column name, but a caption is allowed to differ
                // from it, so either one naming the column will do.
                string name = NameOf(bound);
                if (name == null || Array.IndexOf(moneyColumns, name) < 0)
                {
                    continue;
                }

                bound.Binding = new Binding(name)
                {
                    Converter = new MoneyConverter(),
                    Mode = BindingMode.OneWay,
                };
            }
        }

        private static string NameOf(DataGridBoundColumn column)
        {
            foreach (string candidate in new[] { column.SortMemberPath, column.Header?.ToString() })
            {
                if (!string.IsNullOrEmpty(candidate))
                {
                    return candidate;
                }
            }
            return null;
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
