using System;
using Windows.UI.Xaml.Data;
using PunchingBand.Models.Enums;

namespace PunchingBand.Pages.BindingConverters
{
    public class WeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((WeightUnit)Enum.Parse(typeof(WeightUnit), parameter as string) == WeightUnit.Lbs)
            {
                value = Math.Round((double) value*2.2);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
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
