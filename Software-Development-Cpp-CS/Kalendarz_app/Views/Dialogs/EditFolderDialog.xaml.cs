using Kalendarz.Models;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;

namespace Kalendarz.Views.Dialogs
{
    public partial class EditFolderDialog : Window
    {
        private FolderItem _folder;
        private string _selectedColor;
        private string _selectedNameColor;
        private string _selectedIconColor;

        public EditFolderDialog(FolderItem folder)
        {
            InitializeComponent();
            _folder = folder;
            _selectedColor = folder.Color;
            _selectedNameColor = folder.NameColor;
            _selectedIconColor = folder.IconColor;

            NameTextBox.Text = folder.Name;
            NameFontSizeTextBox.Text = folder.NameFontSize.ToString();
            ImagePathTextBox.Text = folder.BackgroundImagePath ?? "";
            CanvasImagePathTextBox.Text = folder.CanvasBackgroundImagePath ?? "";
            UpdateColorPreview();
            UpdateNameColorPreview();
            UpdateIconColorPreview();
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerDialog(_selectedColor);
            if (dialog.ShowDialog() == true)
            {
                _selectedColor = dialog.SelectedColor;
                UpdateColorPreview();
            }
        }

        private void SelectNameColor_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerDialog(_selectedNameColor);
            if (dialog.ShowDialog() == true)
            {
                _selectedNameColor = dialog.SelectedColor;
                UpdateNameColorPreview();
            }
        }

        private void SelectIconColor_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerDialog(_selectedIconColor);
            if (dialog.ShowDialog() == true)
            {
                _selectedIconColor = dialog.SelectedColor;
                UpdateIconColorPreview();
            }
        }

        private void SelectIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string icon)
            {
                _folder.Icon = icon;
            }
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Pliki obrazów|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Wszystkie pliki|*.*",
                Title = "Wybierz obraz tła"
            };

            if (dialog.ShowDialog() == true)
            {
                ImagePathTextBox.Text = dialog.FileName;
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            ImagePathTextBox.Text = "";
        }

        private void SelectCanvasImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Pliki obrazów|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Wszystkie pliki|*.*",
                Title = "Wybierz obraz tła pulpitu"
            };

            if (dialog.ShowDialog() == true)
            {
                CanvasImagePathTextBox.Text = dialog.FileName;
            }
        }

        private void RemoveCanvasImage_Click(object sender, RoutedEventArgs e)
        {
            CanvasImagePathTextBox.Text = "";
        }

        private void UpdateColorPreview()
        {
            try
            {
                ColorPreview.Background = (Brush)new BrushConverter().ConvertFromString(_selectedColor)!;
            }
            catch
            {
                ColorPreview.Background = Brushes.White;
            }
        }

        private void UpdateNameColorPreview()
        {
            try
            {
                NameColorPreview.Background = (Brush)new BrushConverter().ConvertFromString(_selectedNameColor)!;
            }
            catch
            {
                NameColorPreview.Background = Brushes.Black;
            }
        }

        private void UpdateIconColorPreview()
        {
            try
            {
                IconColorPreview.Background = (Brush)new BrushConverter().ConvertFromString(_selectedIconColor)!;
            }
            catch
            {
                IconColorPreview.Background = Brushes.Black;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            _folder.Name = NameTextBox.Text;
            _folder.Color = _selectedColor;
            _folder.NameColor = _selectedNameColor;
            _folder.IconColor = _selectedIconColor;
            _folder.BackgroundImagePath = string.IsNullOrWhiteSpace(ImagePathTextBox.Text) ? null : ImagePathTextBox.Text;
            _folder.CanvasBackgroundImagePath = string.IsNullOrWhiteSpace(CanvasImagePathTextBox.Text) ? null : CanvasImagePathTextBox.Text;

            if (double.TryParse(NameFontSizeTextBox.Text, out double fontSize) && fontSize > 0)
            {
                _folder.NameFontSize = fontSize;
            }

            DialogResult = true;
            Close();
        }
    }
}
