using Kalendarz.Helpers;
using Kalendarz.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Kalendarz.ViewModels
{
    public class CalendarViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly Plan _plan;

        public Plan Plan => _plan;

        public ObservableCollection<CalendarDay> Days { get; }

        private CalendarDay? _selectedDay;
        public CalendarDay? SelectedDay
        {
            get => _selectedDay;
            set => SetProperty(ref _selectedDay, value);
        }

        public ICommand BackCommand { get; }
        public ICommand AddEventCommand { get; }
        public ICommand EditDayCommand { get; }
        public ICommand DeleteEventCommand { get; }

        // Lazy loading - śledź ile już załadowano
        private DateTime _startDate;
        private DateTime _lastLoadedDate;
        private bool _isLoading = false;
        private const int INITIAL_DAYS = 70; // ~11 wierszy (widocznych na ekranie)
        private const int LOAD_MORE_DAYS = 60; // ~2 miesiące na raz

        public CalendarViewModel(MainViewModel main, Plan plan)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));

            Days = new ObservableCollection<CalendarDay>();

            // Inicjalizacja kalendarza - załaduj początkową część
            InitializeCalendar();

            BackCommand = new RelayCommand(_ =>
            {
                // Wróć do Pulpitu
                var pulpitPlan = _main.Plans.FirstOrDefault(p => p.Name == "Pulpit");
                if (pulpitPlan != null)
                {
                    _main.ShowFileManagerView(pulpitPlan);
                }
            });
            AddEventCommand = new RelayCommand(p => AddEvent(p as CalendarDay), p => p is CalendarDay);
            EditDayCommand = new RelayCommand(p => EditDay(p as CalendarDay), p => p is CalendarDay);
            DeleteEventCommand = new RelayCommand(p => DeleteEvent(p as CalendarDay), p => p is CalendarDay);
        }

        public void LoadMoreDays()
        {
            if (_isLoading) return;

            var today = DateTime.Today;
            var oneYearAhead = today.AddYears(1);

            // Jeśli już załadowano rok do przodu, nie ładuj więcej
            if (_lastLoadedDate >= oneYearAhead)
                return;

            _isLoading = true;

            // Załaduj kolejne LOAD_MORE_DAYS dni
            LoadDaysRange(_lastLoadedDate.AddDays(1), LOAD_MORE_DAYS);

            _isLoading = false;
        }

        private void InitializeCalendar()
        {
            // Znajdź dzisiejszą datę
            var today = DateTime.Today;

            // Znajdź poniedziałek bieżącego tygodnia
            var currentWeekMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                currentWeekMonday = currentWeekMonday.AddDays(-7);
            }

            // Cofnij się o 2 tygodnie (14 dni)
            _startDate = currentWeekMonday.AddDays(-14);
            _lastLoadedDate = _startDate.AddDays(-1);

            // Załaduj początkową część (11 wierszy widocznych na ekranie)
            LoadDaysRange(_startDate, INITIAL_DAYS);
        }

        private void LoadDaysRange(DateTime startDate, int daysToLoad)
        {
            var currentDate = startDate;
            int currentMonth = startDate.Month;
            int currentYear = startDate.Year;
            var allDays = new System.Collections.Generic.List<CalendarDay>();

            // Jeśli to nie pierwszy load, musimy sprawdzić czy poprzedni miesiąc się zakończył
            if (Days.Count > 0)
            {
                var lastDay = Days.LastOrDefault(d => !d.IsEmpty);
                if (lastDay != null)
                {
                    currentMonth = lastDay.Date.Month;
                    currentYear = lastDay.Date.Year;
                }
            }

            int realDaysAdded = 0;

            while (realDaysAdded < daysToLoad)
            {
                // Sprawdź czy to nowy miesiąc
                if ((currentDate.Month != currentMonth || currentDate.Year != currentYear))
                {
                    // Wypełnij resztę bieżącego tygodnia pustymi dniami
                    int totalCount = Days.Count + allDays.Count;
                    int daysInCurrentWeek = totalCount % 7;
                    while (daysInCurrentWeek > 0 && daysInCurrentWeek < 7)
                    {
                        var emptyDay = new CalendarDay
                        {
                            Date = DateTime.MinValue,
                            Description = string.Empty,
                            BackgroundColor = "Transparent",
                            IsEmpty = true
                        };
                        allDays.Add(emptyDay);
                        daysInCurrentWeek = (Days.Count + allDays.Count) % 7;
                    }

                    // Dodaj cały pusty wiersz (7 dni)
                    for (int i = 0; i < 7; i++)
                    {
                        var emptyDay = new CalendarDay
                        {
                            Date = DateTime.MinValue,
                            Description = string.Empty,
                            BackgroundColor = "Transparent",
                            IsEmpty = true
                        };
                        allDays.Add(emptyDay);
                    }

                    // Dodaj puste dni na początku tygodnia
                    var firstDayOfMonth = currentDate;
                    var dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
                    int emptyDaysToAdd = dayOfWeek == 0 ? 6 : dayOfWeek - 1;

                    for (int i = 0; i < emptyDaysToAdd; i++)
                    {
                        var emptyDay = new CalendarDay
                        {
                            Date = DateTime.MinValue,
                            Description = string.Empty,
                            BackgroundColor = "Transparent",
                            IsEmpty = true
                        };
                        allDays.Add(emptyDay);
                    }

                    currentMonth = currentDate.Month;
                    currentYear = currentDate.Year;
                }

                // Dodaj normalny dzień
                var day = new CalendarDay
                {
                    Date = currentDate.Date,
                    Description = string.Empty,
                    BackgroundColor = "#FFFFFFFF"
                };

                LoadDayData(day);
                allDays.Add(day);
                realDaysAdded++;

                currentDate = currentDate.AddDays(1);
            }

            // Dodaj wszystkie dni na raz
            foreach (var day in allDays)
            {
                Days.Add(day);
            }

            // Zapisz ostatnią załadowaną datę
            var lastReal = allDays.LastOrDefault(d => !d.IsEmpty);
            if (lastReal != null)
            {
                _lastLoadedDate = lastReal.Date;
            }

            Debug.WriteLine($"Loaded {realDaysAdded} more days. Total: {Days.Count} items ({Days.Count(d => !d.IsEmpty)} real days), last date: {_lastLoadedDate:yyyy-MM-dd}");
        }

        private void LoadDayData(CalendarDay day)
        {
            var dateKey = day.Date.ToString("yyyy-MM-dd");
            if (_plan.CalendarDays.TryGetValue(dateKey, out var data))
            {
                day.Description = data.Description;
                day.BackgroundColor = data.BackgroundColor;
            }
        }

        private void SaveDayData(CalendarDay day)
        {
            var dateKey = day.Date.ToString("yyyy-MM-dd");
            _plan.CalendarDays[dateKey] = new CalendarDayData
            {
                Date = dateKey,
                Description = day.Description,
                BackgroundColor = day.BackgroundColor
            };
        }

        public void SaveDayDataPublic(CalendarDay day)
        {
            SaveDayData(day);
            _main.SavePlans();
        }

        private void AddEvent(CalendarDay? day)
        {
            if (day == null) return;

            var window = new Views.EditDayDialog(day);
            if (window.ShowDialog() == true)
            {
                day.Description = window.EventDescription;
                day.BackgroundColor = window.SelectedColor;
                SaveDayData(day);
                _main.SavePlans();
            }
        }

        private void EditDay(CalendarDay? day)
        {
            if (day == null) return;

            var window = new Views.EditDayDialog(day);
            if (window.ShowDialog() == true)
            {
                day.Description = window.EventDescription;
                day.BackgroundColor = window.SelectedColor;
                SaveDayData(day);
                _main.SavePlans();
            }
        }

        private void DeleteEvent(CalendarDay? day)
        {
            if (day == null) return;

            day.Description = string.Empty;
            day.BackgroundColor = "#FFFFFFFF";
            SaveDayData(day);
            _main.SavePlans();
        }
    }
}
