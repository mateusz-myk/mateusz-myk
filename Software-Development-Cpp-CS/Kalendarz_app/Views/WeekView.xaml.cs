using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using Kalendarz.ViewModels;
using Kalendarz.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;
using Kalendarz.Helpers;

namespace Kalendarz.Views
{
    public partial class WeekView : UserControl
    {
        private const double PixelsPerHour = 48.0;
        private const double ColumnWidth = 150.0;
        private const int StartHour = 7;  // plan zaczyna się od 7:00
        private const int HoursCount = 15; // 7..22 -> 22-7=15
        private const double HeaderHeight = 28.0;

        private DispatcherTimer? _currentTimeTimer;
        private Line? _currentTimeLine;

        public WeekView()
        {
            InitializeComponent();
            this.Loaded += WeekView_Loaded;
            this.Unloaded += WeekView_Unloaded;

            // Ustaw focus aby UserControl mógł odbierać zdarzenia klawiatury
            this.Loaded += (s, e) => this.Focus();

            // Nasłuchuj zmian motywu i przerysuj siatkę
            ThemeManager.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ThemeManager.IsDarkTheme))
                {
                    DrawGrid();
                }
            };
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is WeekViewModel vm && vm.BackCommand.CanExecute(null))
                {
                    vm.BackCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void WeekView_Loaded(object? sender, RoutedEventArgs e)
        {
            try
            {
                DrawGrid();
                HookNotesEditing();

                // Ustaw dzisiejszą datę
                if (TodayLabel != null)
                    TodayLabel.Text = DateTime.Now.ToString("dddd, d MMMM yyyy", new System.Globalization.CultureInfo("pl-PL"));

                // Ustaw aktualną godzinę
                UpdateCurrentTime();

                // Uruchom timer do odświeżania czerwonej linii i godziny
                StartCurrentTimeTimer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in WeekView_Loaded: {ex.Message}");
                MessageBox.Show($"Błąd podczas ładowania widoku: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WeekView_Unloaded(object? sender, RoutedEventArgs e)
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
            _currentTimeTimer.Tick += (s, e) => 
            {
                DrawCurrentTimeLine();
                UpdateCurrentTime();
            };
            _currentTimeTimer.Start();

            // Narysuj linię od razu
            DrawCurrentTimeLine();
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

            // Usuń linię z canvas
            if (_currentTimeLine != null && CurrentTimeCanvas != null)
            {
                CurrentTimeCanvas.Children.Remove(_currentTimeLine);
                _currentTimeLine = null;
            }
        }

        private void DrawCurrentTimeLine()
        {
            if (CurrentTimeCanvas == null)
                return;

            var now = DateTime.Now;
            var currentHour = now.Hour + now.Minute / 60.0;

            // Sprawdź czy aktualna godzina jest w zakresie wyświetlanym (7:00-22:00)
            if (currentHour < StartHour || currentHour > (StartHour + HoursCount))
            {
                // Ukryj linię jeśli jest poza zakresem
                if (_currentTimeLine != null)
                    _currentTimeLine.Visibility = Visibility.Collapsed;
                return;
            }

            // Określ indeks dnia tygodnia (Poniedziałek=0, Wtorek=1, ..., Niedziela=6)
            var dayIndex = (int)now.DayOfWeek;
            // DayOfWeek.Sunday = 0, więc musimy przekonwertować na poniedziałek=0
            dayIndex = dayIndex == 0 ? 6 : dayIndex - 1;

            // Oblicz pozycję Y dla aktualnej godziny
            var y = HeaderHeight + (currentHour - StartHour) * PixelsPerHour;

            // Oblicz pozycję X dla konkretnego dnia
            var x1 = dayIndex * ColumnWidth;
            var x2 = (dayIndex + 1) * ColumnWidth;

            if (_currentTimeLine == null)
            {
                // Utwórz nową linię
                _currentTimeLine = new Line
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 3,
                    X1 = x1,
                    X2 = x2,
                    Y1 = y,
                    Y2 = y
                };
                CurrentTimeCanvas.Children.Add(_currentTimeLine);
            }
            else
            {
                // Aktualizuj pozycję istniejącej linii
                _currentTimeLine.X1 = x1;
                _currentTimeLine.X2 = x2;
                _currentTimeLine.Y1 = y;
                _currentTimeLine.Y2 = y;
                _currentTimeLine.Visibility = Visibility.Visible;
            }
        }

        private void HookNotesEditing()
        {
            if (this.DataContext is WeekViewModel vm)
            {
                foreach (var n in vm.Notes)
                    AttachNoteHandler(n);
                vm.Notes.CollectionChanged += Notes_CollectionChanged;
            }
        }

        private void Notes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var ni in e.NewItems)
                    if (ni is Note n) AttachNoteHandler(n);
        }

        private void AttachNoteHandler(Note n)
        {
            n.PropertyChanged -= Note_PropertyChanged;
            n.PropertyChanged += Note_PropertyChanged;
        }

        private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEditing" && sender is Note n && n.IsEditing)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    try
                    {
                        if (NotesListBox == null) return;
                        var lbi = NotesListBox.ItemContainerGenerator.ContainerFromItem(n) as ListBoxItem;
                        if (lbi == null) return;
                        var tb = FindVisualChild<System.Windows.Controls.TextBox>(lbi);
                        if (tb != null) { tb.Focus(); tb.SelectAll(); }
                    }
                    catch { }
                }));
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void DrawGrid()
        {
            if (ScheduleCanvas == null) return;

            ScheduleCanvas.Children.Clear();
            ScheduleCanvas.Width = ColumnWidth * 7;
            ScheduleCanvas.Height = HeaderHeight + PixelsPerHour * HoursCount;

            // Ustaw kolor tła Canvasu w zależności od motywu
            ScheduleCanvas.Background = ThemeManager.Instance.IsDarkTheme 
                ? new SolidColorBrush(Color.FromRgb(30, 30, 30)) 
                : Brushes.White;

            if (HourGrid != null)
            {
                HourGrid.Width = 55;
                HourGrid.Height = ScheduleCanvas.Height;
                HourGrid.RowDefinitions.Clear();
                HourGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HeaderHeight) });
                for (int i = 0; i < HoursCount; i++)
                    HourGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(PixelsPerHour) });
            }

            // tła weekendowe (sobota i niedziela)
            var weekendBrush = ThemeManager.Instance.IsDarkTheme 
                ? new SolidColorBrush(Color.FromArgb(0xFF, 0x28, 0x30, 0x38)) 
                : new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF8, 0xFF));

            // Sobota (kolumna 5)
            var saturdayRect = new Rectangle
            {
                Width = ColumnWidth,
                Height = ScheduleCanvas.Height - HeaderHeight,
                Fill = weekendBrush
            };
            Canvas.SetLeft(saturdayRect, 5 * ColumnWidth);
            Canvas.SetTop(saturdayRect, HeaderHeight);
            ScheduleCanvas.Children.Add(saturdayRect);

            // Niedziela (kolumna 6)
            var sundayRect = new Rectangle
            {
                Width = ColumnWidth,
                Height = ScheduleCanvas.Height - HeaderHeight,
                Fill = weekendBrush
            };
            Canvas.SetLeft(sundayRect, 6 * ColumnWidth);
            Canvas.SetTop(sundayRect, HeaderHeight);
            ScheduleCanvas.Children.Add(sundayRect);

            // pionowe linie kolumn
            var verticalLineColor = ThemeManager.Instance.IsDarkTheme 
                ? Color.FromRgb(60, 60, 60) 
                : Color.FromRgb(220, 220, 220);

            for (int i = 0; i <= 7; i++)
            {
                var line = new Line
                {
                    X1 = i * ColumnWidth, X2 = i * ColumnWidth,
                    Y1 = HeaderHeight, Y2 = ScheduleCanvas.Height,
                    Stroke = new SolidColorBrush(verticalLineColor),
                    StrokeThickness = 1
                };
                ScheduleCanvas.Children.Add(line);
            }

            // uaktualnij konwerter TimeToY przez kod (nadpisuje wartość z XAML)
            try
            {
                if (ZajeciaItemsControl?.Resources.Contains("TimeToY") == true)
                {
                    if (ZajeciaItemsControl.Resources["TimeToY"] is Kalendarz.Converters.TimeToYConverter conv)
                    {
                        conv.StartHour = StartHour;
                        conv.TopOffset = HeaderHeight;
                        conv.PixelsPerHour = PixelsPerHour;
                    }
                }
            }
            catch { }

            // poziome linie godzinowe
            var horizontalLineColor = ThemeManager.Instance.IsDarkTheme 
                ? Color.FromRgb(50, 50, 50) 
                : Color.FromRgb(230, 230, 230);

            for (int h = 0; h <= HoursCount; h++)
            {
                var y = HeaderHeight + h * PixelsPerHour;
                var line = new Line
                {
                    X1 = 0, X2 = ScheduleCanvas.Width,
                    Y1 = y, Y2 = y,
                    Stroke = new SolidColorBrush(horizontalLineColor),
                    StrokeThickness = 1
                };
                ScheduleCanvas.Children.Add(line);
            }

            // etykiety godzin w HourGrid
            if (HourGrid != null)
            {
                HourGrid.Children.Clear();
                var overlay = new Canvas { Width = 55, Height = HourGrid.Height, IsHitTestVisible = false };
                HourGrid.Children.Add(overlay);

                var textColor = ThemeManager.Instance.IsDarkTheme ? Brushes.White : Brushes.Black;

                for (int i = 0; i <= HoursCount; i++)
                {
                    var tb = new TextBlock 
                    { 
                        Text = $"{StartHour + i}:00", 
                        FontSize = 11,
                        Foreground = textColor
                    };
                    double y = HeaderHeight + i * PixelsPerHour;
                    Canvas.SetLeft(tb, 2);
                    Canvas.SetTop(tb, y - 7); // centruj tekst na linii poziomej
                    overlay.Children.Add(tb);
                }
            }

            // nagłówki dni
            string[] days = { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota", "Niedziela" };
            var headerTextColor = ThemeManager.Instance.IsDarkTheme ? Brushes.White : Brushes.Black;

            for (int i = 0; i < 7; i++)
            {
                var tb = new TextBlock 
                { 
                    Text = days[i], 
                    FontWeight = FontWeights.SemiBold,
                    Foreground = headerTextColor
                };
                Canvas.SetLeft(tb, i * ColumnWidth + 6);
                Canvas.SetTop(tb, 4);
                ScheduleCanvas.Children.Add(tb);
            }
        }

        private void ScheduleCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ScheduleCanvas == null) return;

            // jeśli kliknięto na blok zajęć — pozwól jego kontekstowemu menu się otworzyć
            var original = e.OriginalSource as DependencyObject;
            for (var cur = original; cur != null; cur = System.Windows.Media.VisualTreeHelper.GetParent(cur))
                if (cur is Border b && b.DataContext is Zajecia) return;

            var pos = e.GetPosition(ScheduleCanvas);

            int dayIndex = (int)System.Math.Floor(pos.X / ColumnWidth);
            dayIndex = System.Math.Clamp(dayIndex, 0, 6);
            var day = (DayOfWeek)(((int)DayOfWeek.Monday + dayIndex) % 7);

            double yRelative = Math.Max(0, pos.Y - HeaderHeight);
            var minutesOffset = (int)(Math.Round(yRelative / PixelsPerHour * 60.0 / 15.0) * 15);
            var start = TimeSpan.FromHours(StartHour).Add(TimeSpan.FromMinutes(minutesOffset));

            var cm = new ContextMenu();
            var miAdd = new MenuItem { Header = "Dodaj zajęcia" };
            miAdd.Click += (_, __) =>
            {
                if (this.DataContext is WeekViewModel vm)
                    vm.AddZajecieAt(day, start);
            };
            cm.Items.Add(miAdd);
            cm.PlacementTarget = ScheduleCanvas;
            cm.IsOpen = true;
            e.Handled = true;
        }

        private void Block_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement fe && fe.DataContext is Zajecia z)
                if (this.DataContext is WeekViewModel vm)
                    vm.EditZajecieCommand.Execute(z);
        }

        private void ContextMenu_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Parent is ContextMenu cm &&
                cm.PlacementTarget is FrameworkElement fe && fe.DataContext is Zajecia z)
                if (this.DataContext is WeekViewModel vm && vm.EditZajecieCommand.CanExecute(z))
                    vm.EditZajecieCommand.Execute(z);
        }

        private void ContextMenu_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Parent is ContextMenu cm &&
                cm.PlacementTarget is FrameworkElement fe && fe.DataContext is Zajecia z)
                if (this.DataContext is WeekViewModel vm && vm.DeleteZajecieCommand.CanExecute(z))
                    vm.DeleteZajecieCommand.Execute(z);
        }

        private void NotesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;
            while (element != null && !(element is ListBoxItem))
                element = VisualTreeHelper.GetParent(element);

            if (element is ListBoxItem lbi && lbi.DataContext is Note note)
            {
                if (this.DataContext is WeekViewModel vm)
                {
                    foreach (var n in vm.Notes) n.IsEditing = false;
                    vm.SelectedNote = note;
                    note.IsEditing = true;
                }
            }
        }

        private void ToggleNotesButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotesPanel != null && ToggleNotesButton != null)
            {
                if (NotesPanel.Visibility == Visibility.Collapsed)
                {
                    // Rozwiń panel notatek
                    NotesPanel.Visibility = Visibility.Visible;
                    ToggleNotesButton.Content = "▶";
                    ToggleNotesButton.ToolTip = "Zwiń notatki";
                }
                else
                {
                    // Zwiń panel notatek
                    NotesPanel.Visibility = Visibility.Collapsed;
                    ToggleNotesButton.Content = "◀";
                    ToggleNotesButton.ToolTip = "Rozwiń notatki";
                }
            }
        }
    }
}
