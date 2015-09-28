using System;
using System.Globalization;
using Xamarin.Forms;

namespace PunchingBand.Pages.BindingConverters
{
    public class FistSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fistSide = (FistSides) value;

            if (fistSide == FistSides.Unknown)
            {
                return "Please select primary fist from the Punching Band tile on your Band.";
            }

            return fistSide.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (FistSides) Enum.Parse(typeof (FistSides), (string) value);
        }
    }
}
