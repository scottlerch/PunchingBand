using System;
using Windows.UI.Xaml.Data;

namespace PunchingBand.Pages.BindingConverters
{
    public class TimeSpanTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return TimeSpan.Zero;
            return TimeSpan.Parse(value.ToString());
        }
    }
}
