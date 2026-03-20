using Kalendarz.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace Kalendarz.Views.Dialogs
{
    public partial class EditShortcutDialog : Window
    {
        public PlanShortcut Shortcut { get; }
        private string _selectedIcon = "";

        public EditShortcutDialog(PlanShortcut shortcut)
        {
            InitializeComponent();
            Shortcut = shortcut;

            // Załaduj dane skrótu
            PlanNameTextBox.Text = shortcut.PlanName;
            _selectedIcon = shortcut.Icon;
            WidthTextBox.Text = shortcut.Width.ToString();
            HeightTextBox.Text = shortcut.Height.ToString();
            NameFontSizeTextBox.Text = shortcut.NameFontSize.ToString();

            // Ustaw kolory na podglądach (Border)
            BackgroundColorPreview.Background = ParseColor(shortcut.Color);
            IconColorPreview.Background = ParseColor(shortcut.IconColor);
            NameColorPreview.Background = ParseColor(shortcut.NameColor);
        }

        private void BackgroundColorButton_Click(object sender, RoutedEventArgs e)
        {
            var currentColor = ColorToHex(((SolidColorBrush)BackgroundColorPreview.Background).Color);
            var dialog = new ColorPickerDialog(currentColor);
            if (dialog.ShowDialog() == true)
            {
                BackgroundColorPreview.Background = ParseColor(dialog.SelectedColor);
            }
        }

        private void IconColorButton_Click(object sender, RoutedEventArgs e)
        {
            var currentColor = ColorToHex(((SolidColorBrush)IconColorPreview.Background).Color);
            var dialog = new ColorPickerDialog(currentColor);
            if (dialog.ShowDialog() == true)
            {
                IconColorPreview.Background = ParseColor(dialog.SelectedColor);
            }
        }

        private void NameColorButton_Click(object sender, RoutedEventArgs e)
        {
            var currentColor = ColorToHex(((SolidColorBrush)NameColorPreview.Background).Color);
            var dialog = new ColorPickerDialog(currentColor);
            if (dialog.ShowDialog() == true)
            {
                NameColorPreview.Background = ParseColor(dialog.SelectedColor);
            }
        }

        private void SelectIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string icon)
            {
                _selectedIcon = icon;
            }
        }

        private void DecreaseWidth_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(WidthTextBox.Text, out double width) && width > 50)
            {
                WidthTextBox.Text = (width - 10).ToString();
            }
        }

        private void IncreaseWidth_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(WidthTextBox.Text, out double width) && width < 400)
            {
                WidthTextBox.Text = (width + 10).ToString();
            }
        }

        private void DecreaseHeight_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(HeightTextBox.Text, out double height) && height > 50)
            {
                HeightTextBox.Text = (height - 10).ToString();
            }
        }

        private void IncreaseHeight_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(HeightTextBox.Text, out double height) && height < 400)
            {
                HeightTextBox.Text = (height + 10).ToString();
            }
        }

        private void SizeTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Opcjonalnie możemy dodać walidację lub podgląd w czasie rzeczywistym
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Walidacja
            if (string.IsNullOrWhiteSpace(_selectedIcon))
            {
                MessageBox.Show("Wybierz ikonę.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(WidthTextBox.Text, out double width) || width <= 0)
            {
                MessageBox.Show("Szerokość musi być liczbą większą od 0.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(HeightTextBox.Text, out double height) || height <= 0)
            {
                MessageBox.Show("Wysokość musi być liczbą większą od 0.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(NameFontSizeTextBox.Text, out double nameFontSize) || nameFontSize <= 0)
            {
                MessageBox.Show("Rozmiar czcionki musi być liczbą większą od 0.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Zapisz zmiany do obiektu
            Shortcut.Icon = _selectedIcon;

            if (BackgroundColorPreview.Background is SolidColorBrush bgBrush)
                Shortcut.Color = ColorToHex(bgBrush.Color);

            if (IconColorPreview.Background is SolidColorBrush iconBrush)
                Shortcut.IconColor = ColorToHex(iconBrush.Color);

            if (NameColorPreview.Background is SolidColorBrush nameBrush)
                Shortcut.NameColor = ColorToHex(nameBrush.Color);

            Shortcut.Width = width;
            Shortcut.Height = height;
            Shortcut.NameFontSize = nameFontSize;

            DialogResult = true;
            Close();
        }

        private SolidColorBrush ParseColor(string colorString)
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }

        private string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
