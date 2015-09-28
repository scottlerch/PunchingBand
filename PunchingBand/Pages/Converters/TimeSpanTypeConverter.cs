using System;
using System.Globalization;
using Xamarin.Forms;

namespace PunchingBand.Pages.BindingConverters
{
    public class TimeSpanTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return TimeSpan.Zero;
            return TimeSpan.Parse(value.ToString());
        }
    }
}
