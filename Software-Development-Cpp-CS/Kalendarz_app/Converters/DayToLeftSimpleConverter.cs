using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Kalendarz.Converters
{
    // Prosty konwerter: DayOfWeek -> Left na Canvasie przy stałej szerokości kolumny
    public class DayToLeftSimpleConverter : IValueConverter
    {
        // szerokość jednej kolumny (dzień) - dopasowane do WeekView (150)
        public double ColumnWidth { get; set; } = 150.0;
        // padding wewnątrz kolumny
        public double Padding { get; set; } = 6.0;
        // ile pustych kolumn dodać z lewej (np. 1 oznacza rezerwację miejsca na kolumnę 'Godzina')
        public int ColumnOffsetCount { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DayOfWeek day)
            {
                int index = day switch
                {
                    DayOfWeek.Monday => 0,
                    DayOfWeek.Tuesday => 1,
                    DayOfWeek.Wednesday => 2,
                    DayOfWeek.Thursday => 3,
                    DayOfWeek.Friday => 4,
                    DayOfWeek.Saturday => 5,
                    DayOfWeek.Sunday => 6,
                    _ => 0,
                };
                return (index + ColumnOffsetCount) * ColumnWidth + Padding;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
