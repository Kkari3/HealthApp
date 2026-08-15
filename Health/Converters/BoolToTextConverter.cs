using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Health.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCalories && parameter is string param)
            {
                var parts = param.Split('|');
                return isCalories ? parts[0] : parts[1];
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
