using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    public class FolderItem : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name = "Nowy folder";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _color = "#FF87CEEB";
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        private string? _backgroundImagePath;
        public string? BackgroundImagePath
        {
            get => _backgroundImagePath;
            set => SetProperty(ref _backgroundImagePath, value);
        }

        private string? _canvasBackgroundImagePath;
        public string? CanvasBackgroundImagePath
        {
            get => _canvasBackgroundImagePath;
            set => SetProperty(ref _canvasBackgroundImagePath, value);
        }

        private string _icon = "📁";
        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
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

        private double _width = 240;
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

        public string? ParentId { get; set; }

        public ObservableCollection<FolderItem> SubFolders { get; set; } = new ObservableCollection<FolderItem>();
        public ObservableCollection<StickyNote> Notes { get; set; } = new ObservableCollection<StickyNote>();
        public ObservableCollection<Arrow> Arrows { get; set; } = new ObservableCollection<Arrow>();
        public ObservableCollection<ImageNote> Images { get; set; } = new ObservableCollection<ImageNote>();

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
