using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kalendarz.Models
{
    public class Student : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = string.Empty;
        private string _lastName = string.Empty;
        private string _lastTopic = string.Empty;
        private string _discordName = string.Empty;
        private string _className = string.Empty;
        private int _paidHours = 0;
        private decimal _hourlyRate = 0;
        private string _status = "AKTYWNY";
        private bool _isSelected = false;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); }
        }

        public string FullName => $"{Name} {LastName}";

        public string LastTopic
        {
            get => _lastTopic;
            set { _lastTopic = value; OnPropertyChanged(); }
        }

        public string DiscordName
        {
            get => _discordName;
            set { _discordName = value; OnPropertyChanged(); }
        }

        public string ClassName
        {
            get => _className;
            set { _className = value; OnPropertyChanged(); }
        }

        public int PaidHours
        {
            get => _paidHours;
            set { _paidHours = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingHours)); }
        }

        public decimal HourlyRate
        {
            get => _hourlyRate;
            set { _hourlyRate = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public List<StudentLesson> Lessons { get; set; } = new List<StudentLesson>();

        // Oblicz wykorzystane godziny
        public int UsedHours
        {
            get
            {
                var total = Lessons.Sum(l => l.Hours);
                return total;
            }
        }

        // Oblicz pozostałe godziny
        public int RemainingHours
        {
            get
            {
                var remaining = PaidHours - UsedHours;
                return remaining;
            }
        }

        // Metoda do odświeżania obliczeń po zmianach w lekcjach
        public void RefreshCalculations()
        {
            OnPropertyChanged(nameof(UsedHours));
            OnPropertyChanged(nameof(RemainingHours));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StudentLesson
    {
        public DateTime Date { get; set; }
        public int Hours { get; set; }
    }
}
