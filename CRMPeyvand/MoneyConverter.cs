using System;
using System.Globalization;
using System.Windows.Data;

namespace CRMPeyvand
{
    /// <summary>
    /// Formats a money value for a grid cell.
    ///
    /// The grids that are generated from a DataTable have no StringFormat to
    /// hang off, and the ones that declare their own columns already carry
    /// "N0". This exists for the generated ones so that a price column reads the
    /// same either way.
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
                case decimal amount:
                    return Money.Display(amount);
                case int count:
                    return count.ToString("N0", CultureInfo.CurrentCulture);
                case long big:
                    return big.ToString("N0", CultureInfo.CurrentCulture);
                case double number:
                    return number.ToString("N0", CultureInfo.CurrentCulture);
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
