using Kalendarz.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Kalendarz.Views.Dialogs
{
    public partial class EditImageDialog : Window
    {
        private ImageNote _image;
        private string _selectedImagePath = string.Empty;
        private BitmapImage? _originalImage;

        public EditImageDialog(ImageNote image)
        {
            InitializeComponent();
            _image = image;
            _selectedImagePath = image.ImagePath;

            ImagePathTextBox.Text = image.ImagePath;
            DescriptionTextBox.Text = image.Description;
            DescriptionFontSizeTextBox.Text = image.DescriptionFontSize.ToString();
            WidthTextBox.Text = image.Width.ToString();
            HeightTextBox.Text = image.Height.ToString();

            LoadImagePreview();
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Pliki obrazów|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Wszystkie pliki|*.*",
                Title = "Wybierz zdjęcie"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                ImagePathTextBox.Text = _selectedImagePath;
                LoadImagePreview();
                
                // Automatycznie ustaw proporcjonalny rozmiar
                AutoScaleImage();
            }
        }

        private void LoadImagePreview()
        {
            try
            {
                if (!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
                {
                    _originalImage = new BitmapImage();
                    _originalImage.BeginInit();
                    _originalImage.CacheOption = BitmapCacheOption.OnLoad;
                    _originalImage.UriSource = new Uri(_selectedImagePath, UriKind.Absolute);
                    _originalImage.EndInit();
                    
                    ImagePreview.Source = _originalImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania obrazu: {ex.Message}", "Błąd", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AutoScaleImage()
        {
            if (_originalImage == null)
                return;

            double originalWidth = _originalImage.PixelWidth;
            double originalHeight = _originalImage.PixelHeight;

            if (originalWidth == 0 || originalHeight == 0)
                return;

            // Ustaw na oryginalny rozmiar
            WidthTextBox.Text = originalWidth.ToString();
            HeightTextBox.Text = originalHeight.ToString();
        }

        private void NormalizeSize_Click(object sender, RoutedEventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("Najpierw wybierz zdjęcie.", "Informacja", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            double originalWidth = _originalImage.PixelWidth;
            double originalHeight = _originalImage.PixelHeight;

            if (originalWidth == 0 || originalHeight == 0)
                return;

            // Normalizacja: mniejszy wymiar = 200px, większy = proporcjonalnie
            double baseSize = 200.0;
            double newWidth, newHeight;

            if (originalWidth <= originalHeight)
            {
                // Szerokość jest mniejsza lub równa - ustaw szerokość na 200
                newWidth = baseSize;
                newHeight = baseSize * (originalHeight / originalWidth);
            }
            else
            {
                // Wysokość jest mniejsza - ustaw wysokość na 200
                newHeight = baseSize;
                newWidth = baseSize * (originalWidth / originalHeight);
            }

            WidthTextBox.Text = Math.Round(newWidth).ToString();
            HeightTextBox.Text = Math.Round(newHeight).ToString();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedImagePath))
            {
                MessageBox.Show("Wybierz zdjęcie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _image.ImagePath = _selectedImagePath;
            _image.Description = DescriptionTextBox.Text;

            if (double.TryParse(DescriptionFontSizeTextBox.Text, out double fontSize) && fontSize > 0)
            {
                _image.DescriptionFontSize = fontSize;
            }

            if (double.TryParse(WidthTextBox.Text, out double width) && width > 10)
            {
                _image.Width = width;
            }

            if (double.TryParse(HeightTextBox.Text, out double height) && height > 10)
            {
                _image.Height = height;
            }

            DialogResult = true;
            Close();
        }
    }
}
