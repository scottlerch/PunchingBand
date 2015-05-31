using System;
using Windows.UI.Xaml.Data;

namespace PunchingBand.Pages.BindingConverters
{
    public class FistSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((FistSide) value) == FistSide.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((bool) value)? FistSide.Left : FistSide.Right;
        }
    }
}
