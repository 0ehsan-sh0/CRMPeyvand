using System;
using System.Globalization;
using System.Windows.Data;

namespace CRMPeyvand
{
    /// <summary>
    /// Formats a money value for a grid cell, converting stored Rial into the
    /// Toman a grid shows.
    ///
    /// The grids that are generated from a DataTable have no StringFormat to
    /// hang off, and the ones that declare their own columns already carry
    /// "N0". This exists for the generated ones so that a price column reads the
    /// same either way.
    ///
    /// Every numeric cell it meets is treated as money and converted. That is
    /// safe because PublicMethods.dgvFiller only ever attaches this converter to
    /// the columns the caller named as money columns - the DataTable holds the
    /// stored Rial either way, so quantities and counts are untouched, which is
    /// what "Stock Counts Left Alone" means.
    /// </summary>
    public class MoneyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case DBNull:
                    return string.Empty;
                case decimal rial:
                    return Money.Display(rial);
                case int rial:
                    return Money.Display(rial);
                case long rial:
                    return Money.Display(rial);
                case double rial:
                    return Money.Display((decimal)rial);
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
