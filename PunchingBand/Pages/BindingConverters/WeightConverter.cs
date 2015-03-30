using System;
using Windows.UI.Xaml.Data;

namespace PunchingBand.Pages.BindingConverters
{
    public class WeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((WeightUnit)Enum.Parse(typeof(WeightUnit), parameter as string) == WeightUnit.Lbs)
            {
                value = (double) value*2.2;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((WeightUnit)Enum.Parse(typeof(WeightUnit), parameter as string) == WeightUnit.Lbs)
            {
                value = (double) value/2.2;
            }

            return value;
        }
    }
}
