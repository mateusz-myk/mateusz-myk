using System;
using System.Globalization;
using System.Windows.Data;

namespace Kalendarz.Converters
{
    // Konwertuje TimeSpan (Start) na pozycję Y na Canvasie
    public class TimeToYConverter : IValueConverter
    {
        // Początkowa godzina siatki
        public double StartHour { get; set; } = 8.0;
        // Piksele na godzinę
        public double PixelsPerHour { get; set; } = 60.0;
        // Dodatkowy offset od góry (np. miejsce na nagłówki)
        public double TopOffset { get; set; } = 0.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                double hours = ts.TotalHours;
                return TopOffset + (hours - StartHour) * PixelsPerHour;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
