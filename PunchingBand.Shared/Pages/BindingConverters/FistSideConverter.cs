using System;
using Windows.UI.Xaml.Data;

namespace PunchingBand.Pages.BindingConverters
{
    public class FistSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((FistSides) value).HasFlag(FistSides.Right);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((bool) value)? FistSides.Left : FistSides.Right;
        }
    }
}
