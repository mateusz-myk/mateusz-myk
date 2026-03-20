using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    public class CalendarDay : INotifyPropertyChanged
    {
        private string _backgroundColor = "#FFFFFFFF";
        private string _description = string.Empty;
        private bool _hasEvent;

        public DateTime Date { get; set; }

        public int DayNumber => Date.Day;

        public bool IsToday => Date != DateTime.MinValue && Date.Date == DateTime.Today;

        public bool IsFirstDayOfMonth => Date != DateTime.MinValue && Date.Day == 1;

        // Czy ten dzień jest weekendem (sobota lub niedziela)
        public bool IsWeekend => Date != DateTime.MinValue && (Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday);

        // Czy ten dzień jest pusty (placeholder)
        public bool IsEmpty { get; set; }

        // Czy ten dzień znajduje się w pierwszym tygodniu nowego miesiąca (dni 1-7)
        public bool IsInFirstWeekOfNewMonth => Date.Day >= 1 && Date.Day <= 7 && Date.Day == 1;

        public string MonthYearLabel => Date.ToString("MMMM yyyy", new System.Globalization.CultureInfo("pl-PL"));

        public string BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                    HasEvent = !string.IsNullOrWhiteSpace(value);
                }
            }
        }

        public bool HasEvent
        {
            get => _hasEvent;
            set
            {
                if (_hasEvent != value)
                {
                    _hasEvent = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CalendarEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
