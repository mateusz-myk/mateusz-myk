using Kalendarz.Helpers;
using Kalendarz.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using Kalendarz.Views;

namespace Kalendarz.ViewModels
{
    // ViewModel dla widoku tygodniowego
    public class WeekViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public Plan Plan { get; }

        public ObservableCollection<Zajecia> Zajecia { get; } = new ObservableCollection<Zajecia>();
        public ObservableCollection<Note> Notes { get; } = new ObservableCollection<Note>();
        private readonly System.Collections.Generic.Dictionary<System.Guid, string> _noteEditBackup = new System.Collections.Generic.Dictionary<System.Guid, string>();
        private Note? _selectedNote;
        public Note? SelectedNote
        {
            get => _selectedNote;
            set
            {
                if (SetProperty(ref _selectedNote, value))
                {
                    // Powiadomienie, żeby CanExecute komend zostało przeliczone
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // Add a zajecie at a specific day and start time (default 1h duration)
        public void AddZajecieAt(DayOfWeek day, TimeSpan start)
        {
            var nowe = new Zajecia
            {
                Title = "Nowe zajęcia",
                Day = day,
                Start = start,
                End = start.Add(System.TimeSpan.FromHours(1))
            };

            // Sprawdź kolizję
            if (HasOverlap(nowe))
            {
                MessageBox.Show(Application.Current.MainWindow, "Nowe zajęcia nakładają się na istniejące. Zmień godzinę lub usuń wcześniejsze zajęcia.", "Kolizja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            nowe.SerialNumber = NextSerialNumber();
            Zajecia.Add(nowe);
            Plan.Zajecia.Add(nowe);
            _main.SavePlans();
        }

        // Add a note at a specific day and start time (default 1h duration)
        public void AddNoteAt(DayOfWeek day, TimeSpan start)
        {
            var note = new Note
            {
                Text = string.Empty,
                Day = day,
                Start = start,
                End = start.Add(System.TimeSpan.FromHours(1)),
                SerialNumber = NextNoteSerial()
            };

            Notes.Add(note);
            Plan.Notes.Add(note);
            _main.SavePlans();
        }

        private string _noteEditorText = string.Empty;
        public string NoteEditorText
        {
            get => _noteEditorText;
            set => SetProperty(ref _noteEditorText, value);
        }

        public ICommand AddZajecieCommand { get; }
        public ICommand EditZajecieCommand { get; }
        public ICommand EditByNumberCommand { get; }
        public ICommand DeleteZajecieCommand { get; }
        public ICommand DeleteByNumberCommand { get; }
        public ICommand AddNoteCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand EditNoteCommand { get; }
        public ICommand CancelEditNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand SelectNoteCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand BackCommand { get; }

        public WeekViewModel(MainViewModel main, Plan plan)
        {
            try
            {
                _main = main;
                Plan = plan;

                // Uporządkuj i przypisz numery seryjne sekwencyjnie
                ReassignSerialNumbers();

                // Kopiuj istniejące zajęcia z modelu do ObservableCollection
                foreach (var z in plan.Zajecia)
                    Zajecia.Add(z);

                // Kopiuj notatki
                ReassignNoteSerials();
                foreach (var n in plan.Notes)
                    Notes.Add(n);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WeekViewModel constructor failed: {ex}");
                MessageBox.Show($"Błąd podczas otwierania planu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            AddZajecieCommand = new RelayCommand(_ =>
            {
                var nowe = new Zajecia { Title = "Nowe zajęcia", Start = System.TimeSpan.FromHours(9).Add(System.TimeSpan.FromMinutes(15)), End = System.TimeSpan.FromHours(10).Add(System.TimeSpan.FromMinutes(45)), Day = DayOfWeek.Monday };
                var dlg = new AddEditZajeciaWindow(nowe) { Owner = Application.Current.MainWindow };
                var result = dlg.ShowDialog();
                if (result == true)
                {
                    // Sprawdź kolizję przed dodaniem
                    if (HasOverlap(nowe))
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Nowe zajęcia nakładają się na istniejące. Zmień godziny lub usuń wcześniejsze zajęcia.", "Kolizja", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // przypisz numer seryjny
                    nowe.SerialNumber = NextSerialNumber();

                    Zajecia.Add(nowe);
                    Plan.Zajecia.Add(nowe);
                    // Zapisz zmiany planu
                    _main.SavePlans();
                }
            });

            ClearAllCommand = new RelayCommand(_ =>
            {
                // Najpierw sprawdź PIN
                var pinDialog = new PinDialog { Owner = Application.Current.MainWindow };
                var result = pinDialog.ShowDialog();

                if (result != true || !pinDialog.IsPinCorrect)
                {
                    // PIN niepoprawny lub anulowano
                    return;
                }

                // PIN poprawny - pokaż potwierdzenie
                var res = MessageBox.Show(Application.Current.MainWindow, "Czy usunąć wszystkie zajęcia w tym planie?", "Potwierdź", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    // Usuń wszystkie zajęcia z widoku i modelu
                    Zajecia.Clear();
                    Plan.Zajecia.Clear();
                    _main.SavePlans();
                }
            });

            // Notes commands
            var canAlways = new RelayCommand(_ => { });

            AddNoteCommand = new RelayCommand(_ =>
            {
                var text = NoteEditorText?.Trim();
                if (string.IsNullOrEmpty(text)) return;
                var note = new Note { Text = text, SerialNumber = NextNoteSerial() };
                Notes.Add(note);
                Plan.Notes.Add(note);
                NoteEditorText = string.Empty;
                _main.SavePlans();
            });

            // Save (end edit) - can accept parameter (Note) or use SelectedNote
            SaveNoteCommand = new RelayCommand(p =>
            {
                var note = p as Note ?? SelectedNote;
                if (note == null) return;
                // end edit
                note.IsEditing = false;
                // remove backup if any
                if (_noteEditBackup.ContainsKey(note.Id)) _noteEditBackup.Remove(note.Id);
                NoteEditorText = string.Empty;
                SelectedNote = null;
                _main.SavePlans();
            }, p => (p as Note) != null || SelectedNote != null);

            EditNoteCommand = new RelayCommand(_ =>
            {
                if (SelectedNote == null) return;
                // cancel any other editing
                foreach (var n in Notes) n.IsEditing = false;
                // backup original text to allow cancel
                _noteEditBackup[SelectedNote.Id] = SelectedNote.Text;
                SelectedNote.IsEditing = true;
                NoteEditorText = SelectedNote.Text;
            }, _ => SelectedNote != null);

            CancelEditNoteCommand = new RelayCommand(p =>
            {
                var note = p as Note ?? SelectedNote;
                if (note == null) return;
                if (_noteEditBackup.TryGetValue(note.Id, out var orig))
                {
                    note.Text = orig;
                    _noteEditBackup.Remove(note.Id);
                }
                note.IsEditing = false;
                NoteEditorText = string.Empty;
                SelectedNote = null;
            }, p => (p as Note) != null || SelectedNote != null);

            DeleteNoteCommand = new RelayCommand(_ =>
            {
                if (SelectedNote == null) return;
                Plan.Notes.Remove(SelectedNote);
                Notes.Remove(SelectedNote);
                ReassignNoteSerials();
                NoteEditorText = string.Empty;
                SelectedNote = null;
                _main.SavePlans();
            });

            SelectNoteCommand = new RelayCommand(p =>
            {
                if (p is Note n)
                {
                    SelectedNote = n;
                    NoteEditorText = n.Text;
                }
            });

            EditZajecieCommand = new RelayCommand(p =>
            {
                if (p is Zajecia z)
                {
                    var dlg = new AddEditZajeciaWindow(z) { Owner = Application.Current.MainWindow };
                    var res = dlg.ShowDialog();
                    // Zmiany są kopiowane w oknie dialogowym do modelu
                    if (res == true)
                    {
                        // po edycji sprawdź kolizję (pomijamy edytowany element)
                        if (HasOverlap(z, ignoreThis: z))
                        {
                            MessageBox.Show(Application.Current.MainWindow, "Zmiany powodują nakładanie się zajęć z innymi. Cofam zmiany.", "Kolizja", MessageBoxButton.OK, MessageBoxImage.Warning);
                            // odśwież kolekcję z oryginalnego modelu
                            Zajecia.Clear();
                            foreach (var it in Plan.Zajecia) Zajecia.Add(it);
                            return;
                        }

                        _main.SavePlans();
                    }
                }
            });

            EditByNumberCommand = new RelayCommand(p =>
            {
                if (p is string s && int.TryParse(s, out var num))
                {
                    var item = Plan.Zajecia.FirstOrDefault(z => z.SerialNumber == num);
                    if (item == null)
                    {
                        MessageBox.Show(Application.Current.MainWindow, $"Nie znaleziono zajęć o numerze {num}.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var dlg = new AddEditZajeciaWindow(item) { Owner = Application.Current.MainWindow };
                    var res = dlg.ShowDialog();
                    if (res == true)
                    {
                        if (HasOverlap(item, ignoreThis: item))
                        {
                            MessageBox.Show(Application.Current.MainWindow, "Zmiany powodują nakładanie się zajęć z innymi. Cofam zmiany.", "Kolizja", MessageBoxButton.OK, MessageBoxImage.Warning);
                            Zajecia.Clear();
                            foreach (var it in Plan.Zajecia) Zajecia.Add(it);
                            return;
                        }
                        _main.SavePlans();
                    }
                }
            });

            DeleteZajecieCommand = new RelayCommand(p =>
            {
                if (p is Zajecia z)
                {
                    var res = MessageBox.Show(Application.Current.MainWindow, $"Usunąć zajęcia '{z.Title}'?", "Usuń", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        Zajecia.Remove(z);
                        Plan.Zajecia.Remove(z);
                        // przerenumeruj i zapisz
                        ReassignSerialNumbers();
                        _main.SavePlans();
                    }
                }
            });

            DeleteByNumberCommand = new RelayCommand(p =>
            {
                if (p is string s && int.TryParse(s, out var num))
                {
                    var item = Plan.Zajecia.FirstOrDefault(z => z.SerialNumber == num);
                    if (item == null)
                    {
                        MessageBox.Show(Application.Current.MainWindow, $"Nie znaleziono zajęć o numerze {num}.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var res = MessageBox.Show(Application.Current.MainWindow, $"Usunąć zajęcia '{item.Title}' (nr {num})?", "Usuń", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        Zajecia.Remove(item);
                        Plan.Zajecia.Remove(item);
                        // renumeruj po usunięciu
                        ReassignSerialNumbers();
                        _main.SavePlans();
                    }
                }
            });

            BackCommand = new RelayCommand(_ =>
            {
                // Wróć do Pulpitu
                var pulpitPlan = _main.Plans.FirstOrDefault(p => p.Name == "Pulpit");
                if (pulpitPlan != null)
                {
                    _main.ShowFileManagerView(pulpitPlan);
                }
            });
        }

        // Sprawdza czy candidate nakłada się na istniejące zajęcia w danym planie.
        // Jeśli ignoreThis jest podane to ten element jest pomijany w porównaniach (przy edycji).
        private bool HasOverlap(Zajecia candidate, Zajecia? ignoreThis = null)
        {
            return Plan.Zajecia
                .Where(z => !ReferenceEquals(z, ignoreThis))
                .Any(z => z.Day == candidate.Day && TimeIntervalsOverlap(z.Start, z.End, candidate.Start, candidate.End));
        }

        private static bool TimeIntervalsOverlap(TimeSpan aStart, TimeSpan aEnd, TimeSpan bStart, TimeSpan bEnd)
        {
            // overlap jeśli aStart < bEnd && aEnd > bStart
            return aStart < bEnd && aEnd > bStart;
        }

        private int NextSerialNumber()
        {
            if (Plan.Zajecia == null || Plan.Zajecia.Count == 0) return 1;
            return Plan.Zajecia.Max(z => z.SerialNumber) + 1;
        }

        private void ReassignSerialNumbers()
        {
            if (Plan.Zajecia == null) return;
            // Sortuj po dniu i czasie rozpoczęcia, przypisz numery 1..N
            var ordered = Plan.Zajecia.OrderBy(z => z.Day).ThenBy(z => z.Start).ToList();
            int i = 1;
            foreach (var z in ordered)
            {
                z.SerialNumber = i++;
            }
            // Jeśli kolejność Plan.Zajecia nie jest zgodna z ordered, nadpisz kolejność
            Plan.Zajecia.Clear();
            foreach (var z in ordered) Plan.Zajecia.Add(z);
        }

        private int NextNoteSerial()
        {
            if (Plan.Notes == null || Plan.Notes.Count == 0) return 1;
            return Plan.Notes.Max(n => n.SerialNumber) + 1;
        }

        private void ReassignNoteSerials()
        {
            if (Plan.Notes == null) return;
            var ordered = Plan.Notes.OrderBy(n => n.SerialNumber).ToList();
            int i = 1;
            foreach (var n in ordered)
            {
                n.SerialNumber = i++;
            }
            Plan.Notes.Clear();
            foreach (var n in ordered) Plan.Notes.Add(n);
            // saving is handled by callers
        }
    }
}