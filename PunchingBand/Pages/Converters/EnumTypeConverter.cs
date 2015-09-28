using PunchingBand.Utilities;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PunchingBand.Pages.BindingConverters
{
    public class EnumTypeConverter : IValueConverter
    {
        private Type enumType;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.ToObject(targetType, (int)value);
        }
    }
}
