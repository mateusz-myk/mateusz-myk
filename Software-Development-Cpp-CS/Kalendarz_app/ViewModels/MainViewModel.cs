using Kalendarz.Helpers;
using Kalendarz.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kalendarz.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<Plan> Plans { get; } = new ObservableCollection<Plan>();

        private ViewModelBase? _currentView;
        public ViewModelBase? CurrentView
        {
            get => _currentView;
            set
            {
                SetProperty(ref _currentView!, value);
                Debug.WriteLine($"MainViewModel: CurrentView changed to {value?.GetType().Name}");
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set
            {
                if (SetProperty(ref _notes, value))
                {
                    try { SaveNotes(); }
                    catch (Exception ex) { Debug.WriteLine($"Nie udało się zapisać notatek: {ex.Message}"); }
                }
            }
        }

        // Komenda do przełączania planów — używana przez przyciski w WeekView
        public ICommand SwitchPlanCommand { get; }

        private readonly string NotesFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kalendarz", "notes.json");
        private readonly string PlansFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kalendarz", "plans.json");

        public MainViewModel()
        {
            Debug.WriteLine("MainViewModel: constructed, loading notes and plans...");
            LoadNotes();
            LoadPlans();

            if (Plans.Count == 0)
            {
                Plans.Add(new Plan { Name = "Plan uczelniany" });
                Plans.Add(new Plan { Name = "Zajęcia jednorazowe" });
                Plans.Add(new Plan { Name = "Plan pracy" });
                Plans.Add(new Plan { Name = "Planer" });
                Plans.Add(new Plan { Name = "Zarządzaj uczniami" });
                Plans.Add(new Plan { Name = "Pulpit" });
            }
            else
            {
                // Upewnij się, że plan "Zajęcia jednorazowe" istnieje
                if (!Plans.Any(p => p.Name == "Zajęcia jednorazowe"))
                {
                    // Dodaj na drugiej pozycji (po Plan uczelniany)
                    var index = Plans.ToList().FindIndex(p => p.Name == "Plan uczelniany");
                    if (index >= 0)
                        Plans.Insert(index + 1, new Plan { Name = "Zajęcia jednorazowe" });
                    else
                        Plans.Insert(0, new Plan { Name = "Zajęcia jednorazowe" });
                    SavePlans();
                }
                // Upewnij się, że plan "Zarządzaj uczniami" istnieje
                if (!Plans.Any(p => p.Name == "Zarządzaj uczniami"))
                {
                    Plans.Add(new Plan { Name = "Zarządzaj uczniami" });
                    SavePlans();
                }
                // Upewnij się, że plan "Planer" istnieje
                if (!Plans.Any(p => p.Name == "Planer"))
                {
                    Plans.Add(new Plan { Name = "Planer" });
                    SavePlans();
                }
                // Upewnij się, że plan "Pulpit" istnieje
                if (!Plans.Any(p => p.Name == "Pulpit"))
                {
                    Plans.Add(new Plan { Name = "Pulpit" });
                    SavePlans();
                }
            }

            SwitchPlanCommand = new RelayCommand(p =>
            {
                if (p is Plan plan)
                {
                    if (plan.Name == "Zarządzaj uczniami")
                    {
                        ShowStudentsView(plan);
                    }
                    else if (plan.Name == "Planer")
                    {
                        ShowCalendarView(plan);
                    }
                    else if (plan.Name == "Pulpit")
                    {
                        ShowFileManagerView(plan);
                    }
                    else
                    {
                        ShowWeekView(plan);
                    }
                }
            });

            // Start bezpośrednio w "Pulpit"
            var pulpit = Plans.FirstOrDefault(p => p.Name == "Pulpit") ?? Plans[0];
            ShowFileManagerView(pulpit);
        }

        // Wywoływane gdy użytkownik chce zobaczyć widok tygodnia
        public void ShowWeekView(Plan plan)
        {
            try
            {
                // Tworzymy WeekViewModel z przekazanym planem
                CurrentView = new WeekViewModel(this, plan);
            }
            catch (System.Exception ex)
            {
                // Złap i zaloguj wyjątek — zapobiegamy nagłemu zamknięciu aplikacji
                System.Diagnostics.Debug.WriteLine($"ShowWeekView failed: {ex}");
                System.Windows.MessageBox.Show($"Nie udało się otworzyć widoku tygodnia: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // Wywoływane gdy użytkownik chce zobaczyć widok uczniów
        public void ShowStudentsView(Plan plan)
        {
            try
            {
                CurrentView = new StudentsViewModel(this, plan);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowStudentsView failed: {ex}");
                System.Windows.MessageBox.Show($"Nie udało się otworzyć widoku uczniów: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // Wywoływane gdy użytkownik chce zobaczyć widok kalendarza
        public void ShowCalendarView(Plan plan)
        {
            try
            {
                CurrentView = new CalendarViewModel(this, plan);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowCalendarView failed: {ex}");
                System.Windows.MessageBox.Show($"Nie udało się otworzyć widoku kalendarza: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // Wywoływane gdy użytkownik chce zobaczyć widok pulpitu
        public void ShowFileManagerView(Plan plan)
        {
            try
            {
                CurrentView = new FileManagerViewModel(plan, () => { });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowFileManagerView failed: {ex}");
                System.Windows.MessageBox.Show($"Nie udało się otworzyć pulpitu: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void SavePlans()
        {
            try
            {
                var dir = Path.GetDirectoryName(PlansFilePath);
                if (!Directory.Exists(dir!)) Directory.CreateDirectory(dir!);
                var json = JsonSerializer.Serialize(Plans, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(PlansFilePath, json);
                Debug.WriteLine($"Plans saved to {PlansFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SavePlans failed: {ex.Message}");
            }
        }

        private void LoadPlans()
        {
            try
            {
                if (File.Exists(PlansFilePath))
                {
                    var json = File.ReadAllText(PlansFilePath);
                    try
                    {
                        var loaded = JsonSerializer.Deserialize<ObservableCollection<Plan>>(json);
                        if (loaded != null)
                        {
                            Plans.Clear();
                            foreach (var p in loaded) Plans.Add(p);
                            Debug.WriteLine("Plans loaded from file");
                        }
                    }
                    catch (System.Text.Json.JsonException jex)
                    {
                        Debug.WriteLine($"Plans JSON parse error: {jex.Message}");
                        // Backup corrupted file and continue with empty plans
                        try
                        {
                            var bk = PlansFilePath + ".corrupt." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak";
                            File.Move(PlansFilePath, bk);
                            Debug.WriteLine($"Backed up corrupted plans file to {bk}");
                        }
                        catch (Exception mex)
                        {
                            Debug.WriteLine($"Failed to backup corrupted plans file: {mex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadPlans failed: {ex.Message}");
            }
        }

        private void SaveNotes()
        {
            var dir = Path.GetDirectoryName(NotesFilePath);
            if (!Directory.Exists(dir!)) Directory.CreateDirectory(dir!);
            var doc = new { notes = this.Notes };
            var json = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(NotesFilePath, json);
            Debug.WriteLine($"Notes saved to {NotesFilePath}");
        }

        private void LoadNotes()
        {
            try
            {
                if (File.Exists(NotesFilePath))
                {
                    var json = File.ReadAllText(NotesFilePath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("notes", out var n))
                    {
                        _notes = n.GetString() ?? string.Empty;
                        OnPropertyChanged(nameof(Notes));
                        Debug.WriteLine("Notes loaded from file");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadNotes failed: {ex.Message}");
            }
        }
    }
}
