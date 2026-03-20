using Kalendarz.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Kalendarz.Views
{
    public partial class EditDayDialog : Window
    {
        public CalendarDay SelectedDay { get; }
        public string EventDescription { get; private set; } = string.Empty;
        public string SelectedColor { get; private set; } = "#FFFFFFFF";

        public EditDayDialog(CalendarDay day)
        {
            InitializeComponent();
            SelectedDay = day;

            // Ustaw datę w formacie polskim
            DateTextBlock.Text = day.Date.ToString("dddd, d MMMM yyyy", new CultureInfo("pl-PL"));

            // Załaduj istniejące dane
            DescriptionTextBox.Text = day.Description;
            SelectedColor = day.BackgroundColor;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string color)
            {
                SelectedColor = color;

                // Podświetl wybrany przycisk
                ResetButtonBorders();
                button.BorderBrush = System.Windows.Media.Brushes.Black;
                button.BorderThickness = new Thickness(3);
            }
        }

        private void ResetButtonBorders()
        {
            BtnWhite.BorderBrush = System.Windows.Media.Brushes.Gray;
            BtnWhite.BorderThickness = new Thickness(1);
            BtnYellow.BorderBrush = System.Windows.Media.Brushes.Gray;
            BtnYellow.BorderThickness = new Thickness(1);
            BtnGreen.BorderBrush = System.Windows.Media.Brushes.Gray;
            BtnGreen.BorderThickness = new Thickness(1);
            BtnBlue.BorderBrush = System.Windows.Media.Brushes.Gray;
            BtnBlue.BorderThickness = new Thickness(1);
            BtnPink.BorderBrush = System.Windows.Media.Brushes.Gray;
            BtnPink.BorderThickness = new Thickness(1);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            EventDescription = DescriptionTextBox.Text;
            DialogResult = true;
            Close();
        }
    }
}
