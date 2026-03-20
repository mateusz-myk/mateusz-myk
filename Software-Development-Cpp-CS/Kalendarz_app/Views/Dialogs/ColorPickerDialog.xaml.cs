using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kalendarz.Views.Dialogs
{
    public partial class ColorPickerDialog : Window
    {
        public string SelectedColor { get; private set; }

        private static readonly string[] Colors = new[]
        {
            "#FFFFFF99", "#FFFFE599", "#FFFFD699", "#FFFFC799",
            "#FFFFB399", "#FFFFA099", "#FFFF9999", "#FFFF99A9",
            "#FFFF99C7", "#FFFF99E5", "#FFF999FF", "#FFE599FF",
            "#FFD699FF", "#FFC799FF", "#FFB399FF", "#FFA099FF",
            "#FF9999FF", "#FF99A9FF", "#FF99C7FF", "#FF99E5FF",
            "#FF99FFFF", "#FF99FFE5", "#FF99FFC7", "#FF99FFA9",
            "#FF99FF99", "#FFA9FF99", "#FFC7FF99", "#FFE5FF99",
            "#FFFFFF", "#FFE0E0E0", "#FFC0C0C0", "#FFA0A0A0",
            "#FF87CEEB", "#FF90EE90", "#FFFFB6C1", "#FFFFFFE0",
            "#FF000000"
        };

        public ColorPickerDialog(string currentColor)
        {
            InitializeComponent();
            SelectedColor = currentColor;
            PopulateColors();
        }

        private void PopulateColors()
        {
            foreach (var color in Colors)
            {
                var border = new Border
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(4),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Background = (Brush)new BrushConverter().ConvertFromString(color)!,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = color
                };

                border.MouseLeftButtonDown += (s, e) =>
                {
                    SelectedColor = (s as Border)?.Tag as string ?? SelectedColor;
                    DialogResult = true;
                    Close();
                };

                ColorPanel.Children.Add(border);
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
