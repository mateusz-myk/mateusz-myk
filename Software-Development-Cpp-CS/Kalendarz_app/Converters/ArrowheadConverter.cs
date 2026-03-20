using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Kalendarz.Converters
{
    public class ArrowheadConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 4 && 
                values[0] is double startX && 
                values[1] is double startY && 
                values[2] is double endX && 
                values[3] is double endY)
            {
                // Oblicz kąt strzałki w stopniach
                double angleRad = Math.Atan2(endY - startY, endX - startX);
                double angleDeg = angleRad * 180 / Math.PI;

                double arrowSize = 12;

                // Punkt końcowy strzałki
                Point tip = new Point(endX, endY);

                // Oblicz punkty grotu
                Point left = new Point(
                    endX - arrowSize * Math.Cos(angleRad - Math.PI / 6),
                    endY - arrowSize * Math.Sin(angleRad - Math.PI / 6)
                );

                Point right = new Point(
                    endX - arrowSize * Math.Cos(angleRad + Math.PI / 6),
                    endY - arrowSize * Math.Sin(angleRad + Math.PI / 6)
                );

                // Jeśli mamy piąty parametr, zwróć pojedynczy punkt lub kąt
                if (values.Length == 5 && values[4] != null)
                {
                    string? pointIndex = values[4].ToString();
                    return pointIndex switch
                    {
                        "angle" => angleDeg,
                        "0" => tip,
                        "1" => left,
                        "2" => right,
                        _ => tip
                    };
                }

                // Domyślnie zwróć PointCollection
                return new PointCollection { tip, left, right };
            }

            if (targetType == typeof(Point))
                return new Point(0, 0);
            if (targetType == typeof(double))
                return 0.0;

            return new PointCollection();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
