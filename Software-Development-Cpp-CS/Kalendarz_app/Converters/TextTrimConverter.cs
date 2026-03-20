using System;
using System.Globalization;
using System.Windows.Data;

namespace Kalendarz.Converters
{
    public class TextTrimConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && parameter is string maxLengthStr)
            {
                if (int.TryParse(maxLengthStr, out int maxLength))
                {
                    if (text.Length > maxLength)
                    {
                        return text.Substring(0, maxLength) + "...";
                    }
                    return text;
                }
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
