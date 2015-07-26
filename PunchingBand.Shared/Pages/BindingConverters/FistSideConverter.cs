using System;
using Windows.UI.Xaml.Data;

namespace PunchingBand.Pages.BindingConverters
{
    public class FistSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var fistSide = (FistSides) value;

            if (fistSide == FistSides.Unknown)
            {
                return "Please select primary fist from the Punching Band tile on your Band.";
            }

            return fistSide.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (FistSides) Enum.Parse(typeof (FistSides), (string) value);
        }
    }
}
