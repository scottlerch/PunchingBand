using System;
using Windows.UI.Xaml.Data;

namespace PunchingBand.Pages.BindingConverters
{
    public class EnumTypeConverter : IValueConverter
    {
        private Type enumType;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            enumType = value.GetType();
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Enum.Parse(enumType, value.ToString());
        }
    }
}
