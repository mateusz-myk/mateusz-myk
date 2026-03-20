using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kalendarz.Converters
{
    public class BackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = BackgroundImagePath
            // values[1] = Color
            
            if (values.Length >= 2)
            {
                var imagePath = values[0] as string;
                var color = values[1] as string;

                // Jeśli jest ścieżka do obrazu i plik istnieje
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        
                        return new ImageBrush
                        {
                            ImageSource = bitmap,
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    catch
                    {
                        // Jeśli błąd, użyj koloru
                    }
                }

                // Jeśli nie ma obrazu, użyj koloru
                if (!string.IsNullOrEmpty(color))
                {
                    try
                    {
                        return new BrushConverter().ConvertFromString(color);
                    }
                    catch
                    {
                        return Brushes.White;
                    }
                }
            }

            return Brushes.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
