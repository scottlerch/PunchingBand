using PunchingBand.Utilities;
using System;
using System.Collections;
using System.Linq;
using Xamarin.Forms;
using System.Globalization;

namespace PunchingBand.Pages.BindingConverters
{
    public class EnumerableEnumTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IEnumerable).Cast<Enum>().Select(e => e.ToString().SplitCamelCase()).ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
