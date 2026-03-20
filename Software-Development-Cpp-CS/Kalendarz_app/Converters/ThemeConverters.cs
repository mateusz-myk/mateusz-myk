using Kalendarz.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Kalendarz.Converters
{
    public class ThemeBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDark)
            {
                return isDark ? new SolidColorBrush(Color.FromRgb(30, 30, 30)) : Brushes.White;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThemeForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDark)
            {
                return isDark ? Brushes.White : Brushes.Black;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThemeBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDark)
            {
                return isDark ? new SolidColorBrush(Color.FromRgb(60, 60, 60)) : new SolidColorBrush(Color.FromRgb(200, 200, 200));
            }
            return new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThemeButtonBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDark)
            {
                return isDark ? new SolidColorBrush(Color.FromRgb(50, 50, 50)) : new SolidColorBrush(Color.FromRgb(240, 240, 240));
            }
            return new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThemeSecondaryBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDark)
            {
                return isDark ? new SolidColorBrush(Color.FromRgb(45, 45, 45)) : new SolidColorBrush(Color.FromRgb(250, 250, 250));
            }
            return new SolidColorBrush(Color.FromRgb(250, 250, 250));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
