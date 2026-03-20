using Kalendarz.Models;
using Kalendarz.ViewModels;
using Kalendarz.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Kalendarz.Views
{
    public partial class FileManagerView : UserControl
    {
        private Point? _dragStart;
        private object? _draggedItem;
        private bool _isDragging;
        private bool _isArrowMode;
        private Point? _arrowStart;
        private FolderItem? _selectedFolder;
        private DispatcherTimer? _currentTimeTimer;

        // Panning
        private bool _isPanning;
        private Point _panningStart;
        private Point _canvasContextMenuPosition;
        private Point _lastMousePosition;

        private bool _isSavingNote = false;
        private double _savedScrollX = 0;
        private double _savedScrollY = 0;

        // Zoom
        private double _currentZoom = 1.0;
        private const double MaxZoom = 5.0;
        private const double ZoomSpeed = 0.1;
        private bool _isClampingScroll = false; // Zapobiega rekurencji w ClampScrollOffset

        public FileManagerView()
        {
            InitializeComponent();
            this.Loaded += FileManagerView_Loaded;
            this.Unloaded += FileManagerView_Unloaded;
            this.DataContextChanged += FileManagerView_DataContextChanged;

            // Ustaw focus aby UserControl mógł odbierać zdarzenia klawiatury
            this.Loaded += (s, e) => this.Focus();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is FileManagerViewModel vm && vm.BackCommand.CanExecute(null))
                {
                    vm.BackCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void FileManagerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Odpięcie starego ViewModelu
            if (e.OldValue is FileManagerViewModel oldVm)
            {
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;
            }

            // Podpięcie nowego ViewModelu
            if (e.NewValue is FileManagerViewModel newVm)
            {
                newVm.PropertyChanged += ViewModel_PropertyChanged;
                // Wycentruj przy pierwszym załadowaniu
                CenterScrollViewer();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Wycentruj ScrollViewer tylko gdy zmienia się folder (ale nie podczas zapisu notatki)
            if (e.PropertyName == nameof(FileManagerViewModel.CurrentFolder) && !_isSavingNote)
            {
                CenterScrollViewer();
            }
        }

        private void SaveScrollPosition()
        {
            if (CanvasScrollViewer != null)
            {
                _savedScrollX = CanvasScrollViewer.HorizontalOffset;
                _savedScrollY = CanvasScrollViewer.VerticalOffset;
            }
        }

        private void RestoreScrollPosition()
        {
            if (CanvasScrollViewer != null)
            {
                CanvasScrollViewer.ScrollToHorizontalOffset(_savedScrollX);
                CanvasScrollViewer.ScrollToVerticalOffset(_savedScrollY);
            }
        }

        private void CenterScrollViewer()
        {
            // Użyj Dispatcher aby zapewnić, że layout został przeliczony
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (CanvasScrollViewer != null && MainCanvas != null && CanvasScaleTransform != null)
                {
                    // Oblicz zoom, aby cały Canvas zmieścił się w widocznym obszarze
                    double viewportWidth = CanvasScrollViewer.ActualWidth;
                    double viewportHeight = CanvasScrollViewer.ActualHeight;

                    if (viewportWidth > 0 && viewportHeight > 0)
                    {
                        double scaleX = viewportWidth / MainCanvas.Width;
                        double scaleY = viewportHeight / MainCanvas.Height;

                        // Wybierz mniejszy zoom, aby cały Canvas był widoczny
                        double fitZoom = Math.Min(scaleX, scaleY);

                        // Dodaj mały margines (95% fit)
                        fitZoom *= 0.95;

                        // Oblicz dynamiczny minimalny zoom
                        double minZoom = GetMinimumZoom();

                        // Ogranicz zoom do dozwolonych wartości
                        fitZoom = Math.Max(minZoom, Math.Min(MaxZoom, fitZoom));

                        _currentZoom = fitZoom;
                        CanvasScaleTransform.ScaleX = _currentZoom;
                        CanvasScaleTransform.ScaleY = _currentZoom;

                        // Wycentruj widok
                        double scaledWidth = MainCanvas.Width * _currentZoom;
                        double scaledHeight = MainCanvas.Height * _currentZoom;

                        double horizontalCenter = Math.Max(0, (scaledWidth - viewportWidth) / 2);
                        double verticalCenter = Math.Max(0, (scaledHeight - viewportHeight) / 2);

                        CanvasScrollViewer.ScrollToHorizontalOffset(horizontalCenter);
                        CanvasScrollViewer.ScrollToVerticalOffset(verticalCenter);
                    }
                }
            }), DispatcherPriority.Loaded);
        }

        private void FileManagerView_Loaded(object? sender, RoutedEventArgs e)
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
                System.Diagnostics.Debug.WriteLine($"Error in FileManagerView_Loaded: {ex.Message}");
            }
        }

        private void FileManagerView_Unloaded(object? sender, RoutedEventArgs e)
        {
            // Zatrzymaj timer gdy widok jest zamykany
            StopCurrentTimeTimer();

            // Odpięcie ViewModelu
            if (DataContext is FileManagerViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
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

        // === ZOOM ===
        private double GetMinimumZoom()
        {
            if (CanvasScrollViewer == null || MainCanvas == null)
                return 0.1;

            double viewportWidth = CanvasScrollViewer.ActualWidth;
            double viewportHeight = CanvasScrollViewer.ActualHeight;

            if (viewportWidth <= 0 || viewportHeight <= 0)
                return 0.1;

            // Oblicz zoom, aby cały Canvas zmieścił się w viewport (fit-to-screen)
            double scaleX = viewportWidth / MainCanvas.Width;
            double scaleY = viewportHeight / MainCanvas.Height;

            // Mniejszy współczynnik zapewni, że cały Canvas jest widoczny
            // To pozwala na pełny scroll po Canvas przy przybliżeniu
            return Math.Min(scaleX, scaleY);
        }

        private void ClampScrollOffset()
        {
            if (CanvasScrollViewer == null || MainCanvas == null || _isClampingScroll)
                return;

            _isClampingScroll = true;

            try
            {
                double viewportWidth = CanvasScrollViewer.ViewportWidth;
                double viewportHeight = CanvasScrollViewer.ViewportHeight;

                if (viewportWidth <= 0 || viewportHeight <= 0)
                    return;

                double scaledWidth = MainCanvas.Width * _currentZoom;
                double scaledHeight = MainCanvas.Height * _currentZoom;

                double maxOffsetX = Math.Max(0, scaledWidth - viewportWidth);
                double maxOffsetY = Math.Max(0, scaledHeight - viewportHeight);

                double clampedX = Math.Max(0, Math.Min(maxOffsetX, CanvasScrollViewer.HorizontalOffset));
                double clampedY = Math.Max(0, Math.Min(maxOffsetY, CanvasScrollViewer.VerticalOffset));

                if (Math.Abs(CanvasScrollViewer.HorizontalOffset - clampedX) > 0.5)
                {
                    CanvasScrollViewer.ScrollToHorizontalOffset(clampedX);
                }

                if (Math.Abs(CanvasScrollViewer.VerticalOffset - clampedY) > 0.5)
                {
                    CanvasScrollViewer.ScrollToVerticalOffset(clampedY);
                }
            }
            finally
            {
                _isClampingScroll = false;
            }
        }

        private void CanvasScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ClampScrollOffset();
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Zoom tylko z Ctrl
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            e.Handled = true;

            if (CanvasScaleTransform == null || CanvasScrollViewer == null)
                return;

            // Pobierz pozycję myszy względem Canvas
            Point mousePosition = e.GetPosition(MainCanvas);

            // Oblicz dynamiczny minimalny zoom (Canvas musi wypełniać viewport)
            double minZoom = GetMinimumZoom();

            // Oblicz nowy zoom
            double delta = e.Delta > 0 ? ZoomSpeed : -ZoomSpeed;
            double newZoom = _currentZoom + delta;

            // Ogranicz zoom (nie można oddalić poza granice Canvas)
            newZoom = Math.Max(minZoom, Math.Min(MaxZoom, newZoom));

            if (newZoom == _currentZoom)
                return;

            // Oblicz punkt, który ma pozostać w tej samej pozycji na ekranie
            double oldZoom = _currentZoom;
            _currentZoom = newZoom;

            // Pobierz obecny offset scroll
            double oldOffsetX = CanvasScrollViewer.HorizontalOffset;
            double oldOffsetY = CanvasScrollViewer.VerticalOffset;

            // Zastosuj nowy zoom
            CanvasScaleTransform.ScaleX = _currentZoom;
            CanvasScaleTransform.ScaleY = _currentZoom;

            // Przelicz nowy offset, aby punkt pod myszą pozostał w tym samym miejscu
            double zoomRatio = _currentZoom / oldZoom;
            double newOffsetX = (oldOffsetX + mousePosition.X * oldZoom) * zoomRatio - mousePosition.X * _currentZoom;
            double newOffsetY = (oldOffsetY + mousePosition.Y * oldZoom) * zoomRatio - mousePosition.Y * _currentZoom;

            // Wymuszenie odświeżenia layoutu
            CanvasScrollViewer.UpdateLayout();

            // Ustaw nowy offset
            CanvasScrollViewer.ScrollToHorizontalOffset(newOffsetX);
            CanvasScrollViewer.ScrollToVerticalOffset(newOffsetY);
        }

        // === ARROW MODE ===
        private string GetSelectedArrowColor()
        {
            // Domyślnie czarne strzałki
            return "#FF000000"; // Czarny
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isArrowMode)
            {
                // Tryb rysowania strzałki
                var position = e.GetPosition(MainCanvas);

                if (_arrowStart == null)
                {
                    // Pierwszy klik - początek strzałki
                    _arrowStart = position;
                }
                else
                {
                    // Drugi klik - koniec strzałki
                    if (DataContext is FileManagerViewModel vm)
                    {
                        var arrow = new Arrow
                        {
                            StartX = _arrowStart.Value.X,
                            StartY = _arrowStart.Value.Y,
                            EndX = position.X,
                            EndY = position.Y,
                            Color = GetSelectedArrowColor(),
                            Thickness = 2
                        };

                        vm.AddArrow(arrow);
                    }

                    _arrowStart = null;
                    _isArrowMode = false; // Wyłącz tryb strzałki
                }
            }
            else
            {
                // Tryb przesuwania (panning)
                _isPanning = true;
                _panningStart = e.GetPosition(CanvasScrollViewer);
                MainCanvas.CaptureMouse();
                MainCanvas.Cursor = Cursors.Hand;
                e.Handled = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Zapisz ostatnią pozycję myszy na canvas
            _lastMousePosition = e.GetPosition(MainCanvas);

            if (_isPanning && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(CanvasScrollViewer);
                var delta = currentPosition - _panningStart;

                double targetOffsetX = CanvasScrollViewer.HorizontalOffset - delta.X;
                double targetOffsetY = CanvasScrollViewer.VerticalOffset - delta.Y;

                CanvasScrollViewer.ScrollToHorizontalOffset(targetOffsetX);
                CanvasScrollViewer.ScrollToVerticalOffset(targetOffsetY);

                _panningStart = currentPosition;
                e.Handled = true;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                MainCanvas.ReleaseMouseCapture();
                MainCanvas.Cursor = Cursors.Arrow;
                e.Handled = true;
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Zapisz pozycję kliknięcia dla menu kontekstowego
            _canvasContextMenuPosition = e.GetPosition(MainCanvas);
        }

        // === CANVAS CONTEXT MENU HANDLERS ===
        private void CanvasAddFolder_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FileManagerViewModel vm)
            {
                // Utwórz folder w pozycji menu kontekstowego
                var folder = new FolderItem
                {
                    Left = _canvasContextMenuPosition.X,
                    Top = _canvasContextMenuPosition.Y,
                    ParentId = vm.CurrentFolder?.Id
                };

                if (vm.CurrentFolder == null)
                {
                    vm.RootFolders.Add(folder);
                }
                else
                {
                    vm.CurrentFolder.SubFolders.Add(folder);
                }

                vm.CurrentFolders.Add(folder);
                vm.SaveData();
            }
        }

        private void CanvasAddNote_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FileManagerViewModel vm)
            {
                // Utwórz notatkę w pozycji menu kontekstowego
                var note = new StickyNote
                {
                    Text = "Nowa notatka",
                    Left = _canvasContextMenuPosition.X,
                    Top = _canvasContextMenuPosition.Y,
                    ParentFolderId = vm.CurrentFolder?.Id
                };

                if (vm.CurrentFolder == null)
                {
                    vm.RootNotes.Add(note);
                }
                else
                {
                    vm.CurrentFolder.Notes.Add(note);
                }

                vm.CurrentNotes.Add(note);
                vm.SaveData();
            }
        }

        private void CanvasDrawArrow_Click(object sender, RoutedEventArgs e)
        {
            // Włącz tryb rysowania strzałki
            _isArrowMode = true;
            _arrowStart = null;
        }

        private void CanvasAddImage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FileManagerViewModel vm)
            {
                // Utwórz zdjęcie w pozycji menu kontekstowego
                var image = new ImageNote
                {
                    Left = _canvasContextMenuPosition.X,
                    Top = _canvasContextMenuPosition.Y,
                    ParentFolderId = vm.CurrentFolder?.Id
                };

                var dialog = new EditImageDialog(image);
                if (dialog.ShowDialog() == true)
                {
                    if (vm.CurrentFolder == null)
                    {
                        vm.RootImages.Add(image);
                    }
                    else
                    {
                        vm.CurrentFolder.Images.Add(image);
                    }

                    vm.CurrentImages.Add(image);
                    vm.SaveData();
                }
            }
        }

        private void CanvasPasteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FileManagerViewModel vm)
            {
                // Wklej folder w pozycji gdzie było menu kontekstowe
                vm.PasteFolderAt(_canvasContextMenuPosition.X, _canvasContextMenuPosition.Y);
            }
        }

        private void PasteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FileManagerViewModel vm)
            {
                // Wklej folder w pozycji gdzie jest obecnie kursor myszy
                vm.PasteFolderAt(_lastMousePosition.X, _lastMousePosition.Y);
            }
        }

        // === FOLDER DRAG & DROP ===
        private void Folder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Zapobiegnij panningowi Canvas
            e.Handled = true;

            // Zaznacz folder jako wybrany
            var border = sender as Border;
            _selectedFolder = border?.DataContext as FolderItem;

            if (e.ClickCount == 2)
            {
                var folder = border?.DataContext as FolderItem;
                if (folder != null && DataContext is FileManagerViewModel vm)
                {
                    vm.OpenFolderCommand.Execute(folder);
                }
            }
            else
            {
                _dragStart = e.GetPosition(MainCanvas);
                _draggedItem = (sender as FrameworkElement)?.DataContext;
                _isDragging = false;
            }
        }

        private void Folder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _dragStart.HasValue && _draggedItem is FolderItem folder)
            {
                var currentPos = e.GetPosition(MainCanvas);
                var diff = currentPos - _dragStart.Value;

                if (!_isDragging && (Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5))
                {
                    _isDragging = true;
                }

                if (_isDragging)
                {
                    folder.Left = Math.Max(0, Math.Min(MainCanvas.Width - folder.Width, folder.Left + diff.X));
                    folder.Top = Math.Max(0, Math.Min(MainCanvas.Height - folder.Height, folder.Top + diff.Y));
                    _dragStart = currentPos;
                }
            }
        }

        private void Folder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && DataContext is FileManagerViewModel vm)
            {
                vm.EditFolderCommand.Execute(_draggedItem);
            }
            _dragStart = null;
            _draggedItem = null;
            _isDragging = false;
        }

        // === NOTE DRAG & DROP ===
        private void Note_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // W trybie strzałki przekazuj kliknięcie do canvas
            if (_isArrowMode)
            {
                e.Handled = false;
                return;
            }

            if (e.ClickCount == 2)
            {
                var note = (sender as Border)?.DataContext as StickyNote;
                if (note != null)
                {
                    note.IsEditing = true;
                    e.Handled = true;
                }
                return;
            }

            _dragStart = e.GetPosition(MainCanvas);
            _draggedItem = (sender as FrameworkElement)?.DataContext;
            _isDragging = false;
            e.Handled = true; // Zapobiegnij panningowi Canvas
        }

        private void Note_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _dragStart.HasValue && _draggedItem is StickyNote note)
            {
                var currentPos = e.GetPosition(MainCanvas);
                var diff = currentPos - _dragStart.Value;

                if (!_isDragging && (Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5))
                {
                    _isDragging = true;
                }

                if (_isDragging)
                {
                    note.Left = Math.Max(0, Math.Min(MainCanvas.Width - note.Width, note.Left + diff.X));
                    note.Top = Math.Max(0, Math.Min(MainCanvas.Height - note.Height, note.Top + diff.Y));
                    _dragStart = currentPos;
                }
            }
        }

        private void Note_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && DataContext is FileManagerViewModel vm && _draggedItem is StickyNote note)
            {
                vm.SaveNoteCommand.Execute(note);
            }
            _dragStart = null;
            _draggedItem = null;
            _isDragging = false;
        }

        // === CHANGE BACKGROUND ===
        private void ChangeBackground_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not FileManagerViewModel vm)
                return;

            var dialog = new OpenFileDialog
            {
                Title = "Wybierz obraz tła pulpitu",
                Filter = "Pliki obrazów|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Wszystkie pliki|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Sprawdź czy plik istnieje i czy to obraz
                    if (File.Exists(dialog.FileName))
                    {
                        // Ustaw tło dla aktualnego folderu lub roota
                        if (vm.CurrentFolder == null)
                        {
                            vm.RootCanvasBackgroundImagePath = dialog.FileName;
                            MessageBox.Show("Tło pulpitu głównego zostało zmienione.", 
                                "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            vm.CurrentFolder.CanvasBackgroundImagePath = dialog.FileName;
                            // Wymuszenie ponownego pobrania CurrentCanvasBackgroundImagePath
                            var temp = vm.CurrentFolder;
                            vm.CurrentFolder = null;
                            vm.CurrentFolder = temp;
                            MessageBox.Show($"Tło pulpitu folderu '{vm.CurrentFolder.Name}' zostało zmienione.", 
                                "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        vm.SaveData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd przy ustawianiu tła: {ex.Message}", 
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // === CONTEXT MENU HANDLERS ===
        private void FolderOpen_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var folder = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as FolderItem : null;
            
            if (folder != null && DataContext is FileManagerViewModel vm)
            {
                vm.OpenFolderCommand.Execute(folder);
            }
        }

        private void FolderEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var folder = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as FolderItem : null;

            if (folder != null)
            {
                var dialog = new EditFolderDialog(folder);
                if (dialog.ShowDialog() == true && DataContext is FileManagerViewModel vm)
                {
                    vm.EditFolderCommand.Execute(folder);
                }
            }
        }

        private void FolderDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var folder = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as FolderItem : null;

            if (folder != null && DataContext is FileManagerViewModel vm)
            {
                var result = MessageBox.Show($"Czy na pewno chcesz usunąć folder '{folder.Name}'?",
                    "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    vm.DeleteFolderCommand.Execute(folder);
                }
            }
        }

        private void FolderCopy_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var folder = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as FolderItem : null;

            if (folder != null && DataContext is FileManagerViewModel vm)
            {
                vm.CopyFolderCommand.Execute(folder);
            }
        }

        private void FolderCut_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var folder = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as FolderItem : null;

            if (folder != null && DataContext is FileManagerViewModel vm)
            {
                vm.CutFolderCommand.Execute(folder);
            }
        }

        private void NoteEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var note = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as StickyNote : null;

            if (note != null)
            {
                SaveScrollPosition();
                _isSavingNote = true;

                var dialog = new EditNoteDialog(note);
                if (dialog.ShowDialog() == true && DataContext is FileManagerViewModel vm)
                {
                    vm.SaveNoteCommand.Execute(note);
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    RestoreScrollPosition();
                    _isSavingNote = false;
                }), DispatcherPriority.Loaded);
            }
        }

        private void NoteCopy_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var note = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as StickyNote : null;

            if (note != null && DataContext is FileManagerViewModel vm)
            {
                vm.CopyNoteCommand.Execute(note);
            }
        }

        private void NoteCut_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var note = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as StickyNote : null;

            if (note != null && DataContext is FileManagerViewModel vm)
            {
                vm.CutNoteCommand.Execute(note);
            }
        }

        private void NoteTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as TextBox;
                if (textBox?.DataContext is StickyNote note && DataContext is FileManagerViewModel vm)
                {
                    _isSavingNote = true;
                    SaveScrollPosition();

                    vm.SaveNoteCommand.Execute(note);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RestoreScrollPosition();
                        _isSavingNote = false;
                    }), DispatcherPriority.Loaded);

                    e.Handled = true;
                }
            }
        }

        private void NoteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void TitleEditTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.IsVisible)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.Focus();
                    Keyboard.Focus(textBox);
                    textBox.SelectAll();
                }), DispatcherPriority.Input);
            }
        }

        private void NoteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox?.DataContext is StickyNote note && DataContext is FileManagerViewModel vm)
            {
                if (note.IsEditing)
                {
                    _isSavingNote = true;
                    SaveScrollPosition();

                    vm.SaveNoteCommand.Execute(note);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RestoreScrollPosition();
                        _isSavingNote = false;
                    }), DispatcherPriority.Loaded);
                }
            }
        }

        private void NoteDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var note = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as StickyNote : null;

            if (note != null && DataContext is FileManagerViewModel vm)
            {
                var result = MessageBox.Show("Czy na pewno chcesz usunąć tę notatkę?",
                    "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    vm.DeleteNoteCommand.Execute(note);
                }
            }
        }

        // === ARROW CONTEXT MENU ===
        private void Arrow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ArrowDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;

            // Znajdź Arrow z DataContext
            Arrow? arrow = null;
            if (contextMenu?.PlacementTarget is Line line)
            {
                arrow = line.DataContext as Arrow;
            }
            else if (contextMenu?.PlacementTarget is Polygon polygon)
            {
                arrow = polygon.DataContext as Arrow;
            }
            else if (contextMenu?.PlacementTarget is System.Windows.Shapes.Path path)
            {
                arrow = path.DataContext as Arrow;
            }

            if (arrow != null && DataContext is FileManagerViewModel vm)
            {
                vm.DeleteArrowCommand.Execute(arrow);
            }
        }

        private void ArrowColorChange_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var colorTag = menuItem?.Tag as string;

            if (string.IsNullOrEmpty(colorTag))
                return;

            // Znajdź Arrow z DataContext
            Arrow? arrow = null;
            if (contextMenu?.PlacementTarget is Line line)
            {
                arrow = line.DataContext as Arrow;
            }
            else if (contextMenu?.PlacementTarget is System.Windows.Shapes.Path path)
            {
                arrow = path.DataContext as Arrow;
            }

            if (arrow != null && DataContext is FileManagerViewModel vm)
            {
                // Zmień kolor strzałki
                switch (colorTag)
                {
                    case "Black":
                        arrow.Color = "#FF000000";
                        break;
                    case "Red":
                        arrow.Color = "#FFFF0000";
                        break;
                    case "White":
                        arrow.Color = "#FFFFFFFF";
                        break;
                    case "Green":
                        arrow.Color = "#FF00FF00";
                        break;
                    case "Yellow":
                        arrow.Color = "#FFFFFF00";
                        break;
                }

                vm.SaveData();
            }
        }

        // === IMAGE DRAG & DROP ===
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Zapobiegnij panningowi Canvas
            e.Handled = true;

            if (e.ClickCount == 2)
            {
                // Double-click - edytuj zdjęcie
                var image = (sender as Border)?.DataContext as ImageNote;
                if (image != null && DataContext is FileManagerViewModel vm)
                {
                    vm.EditImageCommand.Execute(image);
                }
            }
            else
            {
                // Single click - przygotuj do drag
                _dragStart = e.GetPosition(MainCanvas);
                _draggedItem = (sender as FrameworkElement)?.DataContext;
                _isDragging = false;
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _dragStart.HasValue && _draggedItem is ImageNote image)
            {
                var currentPos = e.GetPosition(MainCanvas);
                var diff = currentPos - _dragStart.Value;

                if (!_isDragging && (Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5))
                {
                    _isDragging = true;
                }

                if (_isDragging)
                {
                    image.Left = Math.Max(0, Math.Min(MainCanvas.Width - image.Width, image.Left + diff.X));
                    image.Top = Math.Max(0, Math.Min(MainCanvas.Height - image.Height, image.Top + diff.Y));
                    _dragStart = currentPos;
                }
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && DataContext is FileManagerViewModel vm)
            {
                vm.SaveData();
            }
            _dragStart = null;
            _draggedItem = null;
            _isDragging = false;
        }

        // === IMAGE CONTEXT MENU ===
        private void ImageEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var image = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as ImageNote : null;

            if (image != null && DataContext is FileManagerViewModel vm)
            {
                vm.EditImageCommand.Execute(image);
            }
        }

        private void ImageDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var image = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as ImageNote : null;

            if (image != null && DataContext is FileManagerViewModel vm)
            {
                var result = MessageBox.Show("Czy na pewno chcesz usunąć to zdjęcie?",
                    "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    vm.DeleteImageCommand.Execute(image);
                }
            }
        }

        // === SHORTCUT DRAG & DROP ===
        private void Shortcut_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Zapobiegnij panningowi Canvas
            e.Handled = true;

            if (e.ClickCount == 2)
            {
                // Double-click - przejdź do planu
                var shortcut = (sender as Border)?.DataContext as PlanShortcut;
                if (shortcut != null)
                {
                    // Znajdź MainViewModel i wywołaj SwitchPlanCommand
                    var mainWindow = Application.Current.MainWindow;
                    if (mainWindow?.DataContext is MainViewModel mainVm)
                    {
                        var plan = mainVm.Plans.FirstOrDefault(p => p.Name == shortcut.PlanName);
                        if (plan != null)
                        {
                            mainVm.SwitchPlanCommand.Execute(plan);
                        }
                    }
                }
            }
            else
            {
                // Single click - przygotuj do drag
                _dragStart = e.GetPosition(MainCanvas);
                _draggedItem = (sender as FrameworkElement)?.DataContext;
                _isDragging = false;
            }
        }

        private void Shortcut_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _dragStart.HasValue && _draggedItem is PlanShortcut shortcut)
            {
                var currentPos = e.GetPosition(MainCanvas);
                var diff = currentPos - _dragStart.Value;

                if (!_isDragging && (Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5))
                {
                    _isDragging = true;
                }

                if (_isDragging)
                {
                    shortcut.Left = Math.Max(0, Math.Min(MainCanvas.Width - shortcut.Width, shortcut.Left + diff.X));
                    shortcut.Top = Math.Max(0, Math.Min(MainCanvas.Height - shortcut.Height, shortcut.Top + diff.Y));
                    _dragStart = currentPos;
                }
            }
        }

        private void Shortcut_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && DataContext is FileManagerViewModel vm)
            {
                vm.SaveData();
            }
            _dragStart = null;
            _draggedItem = null;
            _isDragging = false;
        }

        // === SHORTCUT CONTEXT MENU ===
        private void ShortcutEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var shortcut = (menuItem?.Parent as ContextMenu)?.PlacementTarget is Border border 
                ? border.DataContext as PlanShortcut 
                : (menuItem?.Parent as ContextMenu)?.PlacementTarget is Button button
                    ? button.DataContext as PlanShortcut 
                    : null;

            if (shortcut != null && DataContext is FileManagerViewModel vm)
            {
                var dialog = new Dialogs.EditShortcutDialog(shortcut);
                if (dialog.ShowDialog() == true)
                {
                    vm.SaveData();
                }
            }
        }

        // === TASKBAR SHORTCUT CLICK ===
        private void TaskbarShortcut_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var shortcut = button?.DataContext as PlanShortcut;
            if (shortcut != null)
            {
                // Znajdź MainViewModel i wywołaj SwitchPlanCommand
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow?.DataContext is MainViewModel mainVm)
                {
                    var plan = mainVm.Plans.FirstOrDefault(p => p.Name == shortcut.PlanName);
                    if (plan != null)
                    {
                        mainVm.SwitchPlanCommand.Execute(plan);
                    }
                }
            }
        }
    }
}
