using System;
using System.Globalization;
using System.Windows.Data;
using BE;

namespace CRMPeyvand
{
    public class PersianKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ItemKind)value == ItemKind.Good ? "محصول" : "خدمات";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
