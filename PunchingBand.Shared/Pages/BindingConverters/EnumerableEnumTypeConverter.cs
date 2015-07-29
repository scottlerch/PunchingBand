using PunchingBand.Utilities;
using System;
using System.Collections;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace PunchingBand.Pages.BindingConverters
{
    public class EnumerableEnumTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as IEnumerable).Cast<Enum>().Select(e => e.ToString().SplitCamelCase()).ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
