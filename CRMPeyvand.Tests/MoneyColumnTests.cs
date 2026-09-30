using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// A grid built from a DataTable has its columns thrown away and rebuilt on
    /// every fill, so a price column can only be grouped by saying so on each
    /// fill. These pin that the name is matched, that the money column is
    /// formatted, and that the columns nobody asked about are left as they were -
    /// the stock count and the invoice number are not money.
    /// </summary>
    public class MoneyColumnTests
    {
        private const string NameColumn = "نام";
        private const string PriceColumn = "قیمت";
        private const string StockColumn = "موجودی";

        private static T OnStaThread<T>(Func<T> body)
        {
            T result = default;
            Exception failure = null;

            var thread = new Thread(() =>
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                try
                {
                    result = body();
                }
                catch (Exception e)
                {
                    failure = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (failure != null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }
            return result;
        }

        private static DataTable CatalogTable()
        {
            var table = new DataTable();
            table.Columns.Add(NameColumn, typeof(string));
            table.Columns.Add(PriceColumn, typeof(decimal));
            table.Columns.Add(StockColumn, typeof(int));
            table.Rows.Add("کالا", 1234567m, 12);
            return table;
        }

        private static DataGridBoundColumn ColumnNamed(DataGrid grid, string name)
        {
            return grid.Columns
                .OfType<DataGridBoundColumn>()
                .SingleOrDefault(c => string.Equals(c.Header?.ToString(), name, StringComparison.Ordinal));
        }

        /// <summary>
        /// What the column will format its cell with, or null when the column is
        /// showing the value as it stands. A generated column binds through a
        /// BindingBase, so the converter is only reachable through the Binding.
        /// </summary>
        private static IValueConverter ConverterOf(DataGridBoundColumn column)
        {
            return (column?.Binding as Binding)?.Converter;
        }

        /// <summary>
        /// A grid that has been through a layout pass, which is the state a grid
        /// is in when a form fills it: its columns exist and it has a width.
        /// An unlaid-out grid has neither, and dgvFiller works the column widths
        /// out by dividing one by the other.
        /// </summary>
        private static DataGrid LaidOutGrid(DataTable table)
        {
            var grid = new DataGrid { Width = 420, Height = 300 };
            grid.ItemsSource = table.DefaultView;
            grid.Measure(new Size(420, 300));
            grid.Arrange(new Rect(0, 0, 420, 300));
            return grid;
        }

        [Fact]
        public void A_named_column_is_given_the_money_formatter()
        {
            IValueConverter converter = OnStaThread(() =>
            {
                var grid = LaidOutGrid(CatalogTable());
                PublicMethods.dgvFiller(grid, CatalogTable(), PriceColumn);
                return ConverterOf(ColumnNamed(grid, PriceColumn));
            });

            Assert.NotNull(converter);
            Assert.Equal("1,234,567", converter.Convert(1234567m, typeof(string), null, CultureInfo.CurrentCulture));
        }

        [Fact]
        public void A_column_nobody_named_is_left_alone()
        {
            IValueConverter converter = OnStaThread(() =>
            {
                var grid = LaidOutGrid(CatalogTable());
                PublicMethods.dgvFiller(grid, CatalogTable(), PriceColumn);
                return ConverterOf(ColumnNamed(grid, StockColumn));
            });

            // A stock count is a count. Formatting it as money would read as a price.
            Assert.Null(converter);
        }

        /// <summary>
        /// The value the grid reads is the one the query produced. A converter
        /// that reformatted the underlying row instead would break the context
        /// menu, which reads columns back out of the DataTable by index.
        /// </summary>
        [Fact]
        public void The_row_under_the_grid_keeps_the_raw_number()
        {
            decimal stored = OnStaThread(() =>
            {
                var table = CatalogTable();
                PublicMethods.dgvFiller(LaidOutGrid(table), table, PriceColumn);
                return (decimal)table.Rows[0][PriceColumn];
            });

            Assert.Equal(1234567m, stored);
        }

        [Fact]
        public void A_column_is_matched_again_after_the_grid_is_refilled()
        {
            IValueConverter converter = OnStaThread(() =>
            {
                var grid = LaidOutGrid(CatalogTable());
                PublicMethods.dgvFiller(grid, CatalogTable(), PriceColumn);

                // A search narrows the rows, which rebuilds the columns.
                PublicMethods.dgvFiller(grid, CatalogTable(), PriceColumn);

                return ConverterOf(ColumnNamed(grid, PriceColumn));
            });

            Assert.NotNull(converter);
        }

        [Fact]
        public void Asking_for_no_columns_at_all_is_the_same_as_the_plain_fill()
        {
            int columns = OnStaThread(() =>
            {
                var grid = LaidOutGrid(CatalogTable());
                PublicMethods.dgvFiller(grid, CatalogTable());
                return grid.Columns.Count;
            });

            Assert.Equal(3, columns);
        }

        [Fact]
        public void An_unknown_column_name_is_simply_not_found()
        {
            IValueConverter converter = OnStaThread(() =>
            {
                var grid = LaidOutGrid(CatalogTable());
                PublicMethods.dgvFiller(grid, CatalogTable(), "ستونی که وجود ندارد");
                return ConverterOf(ColumnNamed(grid, PriceColumn));
            });

            Assert.Null(converter);
        }
    }
}
