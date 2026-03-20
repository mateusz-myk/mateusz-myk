using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Kalendarz.Converters
{
    // Konwertuje string (np. "#FFBEE6FD" lub nazwa koloru) na SolidColorBrush
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(s);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // fallback
                    return new SolidColorBrush(Colors.LightBlue);
                }
            }
            return new SolidColorBrush(Colors.LightBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
