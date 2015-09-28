using System;
using System.Globalization;
using Xamarin.Forms;

namespace PunchingBand.Pages.BindingConverters
{
    public class WeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((WeightUnit)Enum.Parse(typeof(WeightUnit), parameter as string) == WeightUnit.Lbs)
            {
                value = Math.Round((double) value*2.2);
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = double.Parse((string)value);

            if ((WeightUnit)Enum.Parse(typeof(WeightUnit), parameter as string) == WeightUnit.Lbs)
            {
                value = number / 2.2;
            }

            return value;
        }
    }
}
