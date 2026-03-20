using Kalendarz.Models;
using Kalendarz.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kalendarz.Views
{
    public partial class CalendarView : UserControl
    {
        private DispatcherTimer? _currentTimeTimer;

        public CalendarView()
        {
            InitializeComponent();
            this.Loaded += CalendarView_Loaded;
            this.Unloaded += CalendarView_Unloaded;

            // Ustaw focus aby UserControl mógł odbierać zdarzenia klawiatury
            this.Loaded += (s, e) => this.Focus();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is CalendarViewModel vm && vm.BackCommand.CanExecute(null))
                {
                    vm.BackCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void CalendarView_Loaded(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Ustaw dzisiejszą datę
                if (TodayLabel != null)
                    TodayLabel.Text = DateTime.Now.ToString("dddd, d MMMM yyyy", new System.Globalization.CultureInfo("pl-PL"));

                // Ustaw aktualną godzinę
                UpdateCurrentTime();

                // Uruchom timer do odświeżania godziny
                StartCurrentTimeTimer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CalendarView_Loaded: {ex.Message}");
            }
        }

        private void CalendarView_Unloaded(object? sender, RoutedEventArgs e)
        {
            // Zatrzymaj timer gdy widok jest zamykany
            StopCurrentTimeTimer();
        }

        private void StartCurrentTimeTimer()
        {
            if (_currentTimeTimer != null)
                return;

            _currentTimeTimer = new DispatcherTimer();
            _currentTimeTimer.Interval = TimeSpan.FromMinutes(1);
            _currentTimeTimer.Tick += (s, e) => UpdateCurrentTime();
            _currentTimeTimer.Start();
        }

        private void UpdateCurrentTime()
        {
            try
            {
                if (CurrentTimeLabel != null)
                    CurrentTimeLabel.Text = DateTime.Now.ToString("HH:mm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateCurrentTime: {ex.Message}");
            }
        }

        private void StopCurrentTimeTimer()
        {
            if (_currentTimeTimer != null)
            {
                _currentTimeTimer.Stop();
                _currentTimeTimer = null;
            }
        }

        private void EditDay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.DataContext is CalendarDay day)
            {
                if (DataContext is CalendarViewModel viewModel)
                {
                    var window = new EditDayDialog(day);
                    if (window.ShowDialog() == true)
                    {
                        day.Description = window.EventDescription;
                        day.BackgroundColor = window.SelectedColor;
                        viewModel.SaveDayDataPublic(day);
                    }
                }
            }
        }

        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.DataContext is CalendarDay day)
            {
                if (DataContext is CalendarViewModel viewModel)
                {
                    day.Description = string.Empty;
                    day.BackgroundColor = "#FFFFFFFF";
                    viewModel.SaveDayDataPublic(day);
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer && DataContext is CalendarViewModel viewModel)
            {
                // Jeśli użytkownik przewinął do 60% wysokości, załaduj więcej dni
                var scrollPercentage = scrollViewer.VerticalOffset / scrollViewer.ScrollableHeight;
                if (scrollPercentage > 0.6)
                {
                    viewModel.LoadMoreDays();
                }
            }
        }
    }
}
