using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    public class PlanShortcut : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _planName = "";
        public string PlanName
        {
            get => _planName;
            set => SetProperty(ref _planName, value);
        }

        private string _icon = "📁";
        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private string _color = "#FFE0E0E0";
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

        private double _width = 200;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height = 200;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private double _nameFontSize = 30;
        public double NameFontSize
        {
            get => _nameFontSize;
            set => SetProperty(ref _nameFontSize, value);
        }

        private string _nameColor = "#FF000000";
        public string NameColor
        {
            get => _nameColor;
            set => SetProperty(ref _nameColor, value);
        }

        private string _iconColor = "#FF000000";
        public string IconColor
        {
            get => _iconColor;
            set => SetProperty(ref _iconColor, value);
        }

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
