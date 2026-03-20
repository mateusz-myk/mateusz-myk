using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    // Reprezentuje pojedyncze zajęcia / wydarzenie
    public class Zajecia : INotifyPropertyChanged
    {
        private Guid _id = Guid.NewGuid();
        private string _title = string.Empty;
        private DayOfWeek _day = DayOfWeek.Monday;
        private TimeSpan _start = TimeSpan.FromHours(8);
        private TimeSpan _end = TimeSpan.FromHours(9);
        private string _description = string.Empty;
        private int _serialNumber;
        private string _color = "#FFBEE6FD";

        public Guid Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
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

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public int SerialNumber
        {
            get => _serialNumber;
            set { _serialNumber = value; OnPropertyChanged(); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        // Formatted strings for UI (read/write to support TwoWay bindings)
        public string StartString
        {
            get => _start.ToString(@"hh\:mm");
            set
            {
                if (TimeSpan.TryParse(value, out var ts))
                {
                    // Assign via Start to trigger existing notifications
                    Start = ts;
                }
            }
        }

        public string EndString
        {
            get => _end.ToString(@"hh\:mm");
            set
            {
                if (TimeSpan.TryParse(value, out var ts))
                {
                    End = ts;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
