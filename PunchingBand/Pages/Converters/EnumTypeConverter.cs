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
            enumType = value.GetType();
            return value.ToString().SplitCamelCase();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(enumType, value.ToString().Replace(" ", ""));
        }
    }
}
