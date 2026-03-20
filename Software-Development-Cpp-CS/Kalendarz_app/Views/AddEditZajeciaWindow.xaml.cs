using Kalendarz.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Kalendarz.Views
{
    public partial class AddEditZajeciaWindow : Window, INotifyPropertyChanged
    {
        private Zajecia _original;
        private Zajecia _editing;

        // Te właściwości są używane do wiązania w oknie dialogowym
        public string TitleText
        {
            get => _editing.Title;
            set { _editing.Title = value; OnPropertyChanged(); }
        }

        public DayOfWeek Day
        {
            get => _editing.Day;
            set { _editing.Day = value; OnPropertyChanged(); }
        }

        public string StartString
        {
            get => _editing.Start.ToString(@"hh\:mm");
            set
            {
                if (TimeSpan.TryParse(value, out var ts))
                {
                    _editing.Start = ts;
                    OnPropertyChanged();
                }
            }
        }

        public string EndString
        {
            get => _editing.End.ToString(@"hh\:mm");
            set
            {
                if (TimeSpan.TryParse(value, out var ts))
                {
                    _editing.End = ts;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _editing.Description;
            set { _editing.Description = value; OnPropertyChanged(); }
        }

        public string SerialNumberString
        {
            get => _editing.SerialNumber == 0 ? string.Empty : _editing.SerialNumber.ToString();
            set
            {
                if (int.TryParse(value, out var n))
                {
                    _editing.SerialNumber = n;
                    OnPropertyChanged();
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    _editing.SerialNumber = 0;
                    OnPropertyChanged();
                }
            }
        }

        public string Color
        {
            get => _editing.Color;
            set { _editing.Color = value; OnPropertyChanged(); }
        }

        public AddEditZajeciaWindow(Zajecia model)
        {
            _original = model ?? throw new ArgumentNullException(nameof(model));
            _editing = new Zajecia
            {
                Id = model.Id,
                Title = model.Title,
                Day = model.Day,
                Start = model.Start,
                End = model.End,
                Description = model.Description,
                Color = model.Color,
                SerialNumber = model.SerialNumber
            };

            // Set DataContext to the window itself so bindings target the window properties
            this.DataContext = this;

            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // Walidacja: start < end
            if (_editing.End <= _editing.Start)
            {
                MessageBox.Show(this, "Czas zakończenia musi być później niż czas rozpoczęcia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Przekopiuj zmiany do oryginału
            _original.Title = _editing.Title;
            _original.Day = _editing.Day;
            _original.Start = _editing.Start;
            _original.End = _editing.End;
            _original.Description = _editing.Description;
            _original.Color = _editing.Color;
            _original.SerialNumber = _editing.SerialNumber;

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

    }
}
