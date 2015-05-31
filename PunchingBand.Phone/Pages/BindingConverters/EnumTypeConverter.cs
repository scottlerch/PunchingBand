using System;
using Windows.UI.Xaml.Data;

namespace PunchingBand.Pages.BindingConverters
{
    public class EnumTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Enum.Parse(targetType, value.ToString());
        }
    }
}
