using Kalendarz.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kalendarz.Views.Dialogs
{
    public partial class EditNoteDialog : Window
    {
        private StickyNote _note;
        private string _selectedColor;

        public EditNoteDialog(StickyNote note)
        {
            InitializeComponent();
            _note = note;
            _selectedColor = note.Color;

            TitleTextBox.Text = note.Title;
            DescriptionTextBox.Text = note.Description;
            WidthTextBox.Text = note.Width.ToString();
            HeightTextBox.Text = note.Height.ToString();

            // Font settings
            FontFamilyComboBox.SelectedItem = FontFamilyComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == note.FontFamily) ?? FontFamilyComboBox.Items[0];

            TitleFontSizeTextBox.Text = note.TitleFontSize.ToString();
            DescFontSizeTextBox.Text = note.DescriptionFontSize.ToString();
            TitleBoldCheckBox.IsChecked = note.IsTitleBold;
            DescBoldCheckBox.IsChecked = note.IsDescriptionBold;

            UpdateColorPreview();
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

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            _note.Title = TitleTextBox.Text;
            _note.Description = DescriptionTextBox.Text;
            _note.Color = _selectedColor;

            // Font settings
            if (FontFamilyComboBox.SelectedItem is ComboBoxItem selectedFont)
            {
                _note.FontFamily = selectedFont.Content.ToString();
            }

            if (double.TryParse(TitleFontSizeTextBox.Text, out double titleSize) && titleSize > 0)
            {
                _note.TitleFontSize = titleSize;
            }

            if (double.TryParse(DescFontSizeTextBox.Text, out double descSize) && descSize > 0)
            {
                _note.DescriptionFontSize = descSize;
            }

            _note.IsTitleBold = TitleBoldCheckBox.IsChecked ?? false;
            _note.IsDescriptionBold = DescBoldCheckBox.IsChecked ?? false;

            // Size
            if (double.TryParse(WidthTextBox.Text, out double width) && width > 50)
            {
                _note.Width = width;
            }

            if (double.TryParse(HeightTextBox.Text, out double height) && height > 20)
            {
                _note.Height = height;
            }

            DialogResult = true;
            Close();
        }
    }
}
