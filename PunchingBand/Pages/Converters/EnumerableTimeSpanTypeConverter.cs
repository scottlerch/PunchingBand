using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace PunchingBand.Pages.BindingConverters
{
    public class EnumerableTimeSpanTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IEnumerable).Cast<TimeSpan>().Select(e => e.ToString()).ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
