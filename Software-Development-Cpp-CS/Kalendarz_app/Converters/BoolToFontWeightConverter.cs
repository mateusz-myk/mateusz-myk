using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kalendarz.Converters
{
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBold && isBold)
            {
                return FontWeights.Bold;
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontWeight fontWeight)
            {
                return fontWeight == FontWeights.Bold;
            }
            return false;
        }
    }
}
