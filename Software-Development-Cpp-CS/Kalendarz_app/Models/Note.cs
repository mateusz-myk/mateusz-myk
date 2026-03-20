using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    public class Note : INotifyPropertyChanged
    {
        private Guid _id = Guid.NewGuid();
        private int _serialNumber;
        private string _text = string.Empty;
        private DayOfWeek _day = DayOfWeek.Monday;
        private TimeSpan _start = TimeSpan.FromHours(8);
        private TimeSpan _end = TimeSpan.FromHours(9);

        public Guid Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public int SerialNumber
        {
            get => _serialNumber;
            set { _serialNumber = value; OnPropertyChanged(); }
        }

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public DayOfWeek Day
        {
            get => _day;
            set { _day = value; OnPropertyChanged(); }
        }

        public TimeSpan Start
        {
            get => _start;
            set { _start = value; OnPropertyChanged(); OnPropertyChanged(nameof(StartString)); }
        }

        public TimeSpan End
        {
            get => _end;
            set { _end = value; OnPropertyChanged(); OnPropertyChanged(nameof(EndString)); }
        }

        public string StartString => _start.ToString(@"hh\:mm");
        public string EndString => _end.ToString(@"hh\:mm");

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Czy notatka jest w trybie edycji (używane przez widok do pokazania edytora in-place)
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }
    }
}
