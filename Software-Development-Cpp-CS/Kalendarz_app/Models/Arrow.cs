using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    public class Arrow : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private double _startX;
        public double StartX
        {
            get => _startX;
            set => SetProperty(ref _startX, value);
        }

        private double _startY;
        public double StartY
        {
            get => _startY;
            set => SetProperty(ref _startY, value);
        }

        private double _endX;
        public double EndX
        {
            get => _endX;
            set => SetProperty(ref _endX, value);
        }

        private double _endY;
        public double EndY
        {
            get => _endY;
            set => SetProperty(ref _endY, value);
        }

        private string _color = "#FF000000";
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        private double _thickness = 2;
        public double Thickness
        {
            get => _thickness;
            set => SetProperty(ref _thickness, value);
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
