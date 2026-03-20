using System;
using System.Globalization;
using System.Windows.Data;

namespace Kalendarz.Converters
{
    // Konwertuje Start i End (TimeSpan) na wysokość bloku w pikselach
    public class DurationToHeightConverter : IMultiValueConverter
    {
        public double PixelsPerHour { get; set; } = 60.0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is TimeSpan start && values[1] is TimeSpan end)
            {
                var duration = (end - start).TotalHours;
                if (duration < 0) duration = 0;
                return duration * PixelsPerHour;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
