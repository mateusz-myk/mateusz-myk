using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    public class StickyNote : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _title = "Nowa notatka";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        // Zachowane dla kompatybilności wstecznej
        private string _text = "Nowa notatka";
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private string _color = "#FFFFFF99";
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        private double _left;
        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        private double _top;
        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        private double _width = 250;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height = 90;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        // Font formatting
        private string _fontFamily = "Segoe UI";
        public string FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        private double _titleFontSize = 30;
        public double TitleFontSize
        {
            get => _titleFontSize;
            set => SetProperty(ref _titleFontSize, value);
        }

        private double _descriptionFontSize = 25;
        public double DescriptionFontSize
        {
            get => _descriptionFontSize;
            set => SetProperty(ref _descriptionFontSize, value);
        }

        private bool _isTitleBold = true;
        public bool IsTitleBold
        {
            get => _isTitleBold;
            set => SetProperty(ref _isTitleBold, value);
        }

        private bool _isDescriptionBold = false;
        public bool IsDescriptionBold
        {
            get => _isDescriptionBold;
            set => SetProperty(ref _isDescriptionBold, value);
        }

        public string? ParentFolderId { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
