using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    public class ImageNote : INotifyPropertyChanged
    {
        private string _imagePath = string.Empty;
        private string _description = string.Empty;
        private double _left;
        private double _top;
        private double _width = 200;
        private double _height = 200;
        private string? _parentFolderId;
        private double _descriptionFontSize = 25;

        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public string? ParentFolderId
        {
            get => _parentFolderId;
            set => SetProperty(ref _parentFolderId, value);
        }

        public double DescriptionFontSize
        {
            get => _descriptionFontSize;
            set => SetProperty(ref _descriptionFontSize, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
