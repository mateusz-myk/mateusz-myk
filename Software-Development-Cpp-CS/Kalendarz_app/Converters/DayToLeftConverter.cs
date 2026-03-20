using System;
using System.Globalization;
using System.Windows.Data;

namespace Kalendarz.Converters
{
    // Konwertuje DayOfWeek oraz szerokość canvasa na pozycję Left
    // Oczekuje MultiBinding: [0] = DayOfWeek, [1] = double canvasActualWidth
    public class DayToLeftConverter : IMultiValueConverter
    {
        public int VisibleDays { get; set; } = 5; // Poniedziałek - Piątek

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is DayOfWeek day && values[1] is double totalWidth)
            {
                // Ustal indeks dnia: poniedziałek -> 0 ... niedziela -> 6
                int index = DayIndex(day);
                if (index < 0) index = 0;
                double columnWidth = totalWidth / VisibleDays;
                return index * columnWidth + 4; // +4 padding
            }
            return 0.0;
        }

        private int DayIndex(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => 0,
                DayOfWeek.Tuesday => 1,
                DayOfWeek.Wednesday => 2,
                DayOfWeek.Thursday => 3,
                DayOfWeek.Friday => 4,
                DayOfWeek.Saturday => 5,
                DayOfWeek.Sunday => 6,
                _ => 7,
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
