using Kalendarz.Helpers;
using Kalendarz.Models;
using Kalendarz.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Kalendarz.ViewModels
{
    public class StudentsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly Plan _plan;

        public Plan Plan => _plan;

        public ObservableCollection<Student> Students { get; }

        private Student? _selectedStudent;
        private ObservableCollection<StudentLesson>? _selectedStudentLessons;
        private string _selectedStudentsHeader = "Zajęcia - Wybierz ucznia";

        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    RefreshSelectedStudentLessons();
                }
            }
        }

        public ObservableCollection<StudentLesson>? SelectedStudentLessons
        {
            get => _selectedStudentLessons;
            set
            {
                _selectedStudentLessons = value;
                OnPropertyChanged();
            }
        }

        public string SelectedStudentsHeader
        {
            get => _selectedStudentsHeader;
            set => SetProperty(ref _selectedStudentsHeader, value);
        }

        private void RefreshSelectedStudentLessons()
        {
            Debug.WriteLine($"RefreshSelectedStudentLessons called");

            // Zbierz wszystkich zaznaczonych uczniów
            var selectedStudents = Students.Where(s => s.IsSelected).ToList();

            if (selectedStudents.Count == 0)
            {
                // Jeśli nic nie zaznaczono, pokaż pojedynczego wybranego ucznia (kliknięcie)
                if (SelectedStudent == null)
                {
                    if (SelectedStudentLessons != null)
                    {
                        SelectedStudentLessons.Clear();
                    }
                    SelectedStudentsHeader = "Zajęcia - Wybierz ucznia";
                    Debug.WriteLine("No students selected");
                }
                else
                {
                    Debug.WriteLine($"Showing lessons for clicked student: {SelectedStudent.FullName}");
                    ShowLessonsForStudents(new[] { SelectedStudent });
                    SelectedStudentsHeader = $"Zajęcia - {SelectedStudent.FullName}";
                }
            }
            else
            {
                // Pokaż zajęcia wszystkich zaznaczonych uczniów
                Debug.WriteLine($"Showing lessons for {selectedStudents.Count} selected students");
                ShowLessonsForStudents(selectedStudents);

                if (selectedStudents.Count == 1)
                {
                    SelectedStudentsHeader = $"Zajęcia - {selectedStudents[0].FullName}";
                }
                else
                {
                    var names = string.Join(", ", selectedStudents.Select(s => s.FullName));
                    SelectedStudentsHeader = $"Zajęcia - {selectedStudents.Count} uczniów: {names}";
                }
            }
        }

        private void ShowLessonsForStudents(System.Collections.Generic.IEnumerable<Student> students)
        {
            if (SelectedStudentLessons == null)
            {
                SelectedStudentLessons = new ObservableCollection<StudentLesson>();
            }
            else
            {
                SelectedStudentLessons.Clear();
            }

            // Zbierz wszystkie zajęcia z wybranych uczniów i posortuj po dacie
            var allLessons = students
                .SelectMany(s => s.Lessons)
                .OrderBy(l => l.Date)
                .ToList();

            foreach (var lesson in allLessons)
            {
                SelectedStudentLessons.Add(lesson);
            }

            Debug.WriteLine($"SelectedStudentLessons now has {SelectedStudentLessons.Count} items");
            OnPropertyChanged(nameof(SelectedStudentLessons));
        }

        public ICommand BackCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand EditStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand AddLessonCommand { get; }
        public ICommand DeleteLessonCommand { get; }

        public StudentsViewModel(MainViewModel main, Plan plan)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));

            // Initialize students collection from plan
            if (plan.Students == null)
            {
                plan.Students = new System.Collections.Generic.List<Student>();
            }
            Students = new ObservableCollection<Student>(plan.Students);

            // Podłącz PropertyChanged dla każdego studenta
            foreach (var student in Students)
            {
                student.PropertyChanged += Student_PropertyChanged;
            }

            BackCommand = new RelayCommand(_ =>
            {
                // Wróć do Pulpitu
                var pulpitPlan = _main.Plans.FirstOrDefault(p => p.Name == "Pulpit");
                if (pulpitPlan != null)
                {
                    _main.ShowFileManagerView(pulpitPlan);
                }
            });

            AddStudentCommand = new RelayCommand(_ => AddStudent());
            EditStudentCommand = new RelayCommand(p => EditStudent(p as Student), p => p is Student);
            DeleteStudentCommand = new RelayCommand(p => DeleteStudent(p as Student), p => p is Student);

            AddLessonCommand = new RelayCommand(_ => AddLesson(), _ => SelectedStudent != null || Students.Any(s => s.IsSelected));
            DeleteLessonCommand = new RelayCommand(p => DeleteLesson(p as StudentLesson), p => p is StudentLesson);
        }

        private void Student_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Student.IsSelected))
            {
                RefreshSelectedStudentLessons();
            }
        }

        private void AddStudent()
        {
            var newStudent = new Student
            {
                Name = "Nowy",
                LastName = "Uczeń",
                Status = "AKTYWNY"
            };

            var window = new AddEditStudentWindow(newStudent);
            if (window.ShowDialog() == true)
            {
                newStudent.PropertyChanged += Student_PropertyChanged;
                Students.Add(newStudent);
                _plan.Students.Add(newStudent);
                _main.SavePlans();
            }
        }

        private void EditStudent(Student? student)
        {
            if (student == null) return;

            var window = new AddEditStudentWindow(student);
            if (window.ShowDialog() == true)
            {
                // Refresh the view
                OnPropertyChanged(nameof(Students));
                _main.SavePlans();
            }
        }

        private void DeleteStudent(Student? student)
        {
            if (student == null) return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz usunąć ucznia {student.FullName}?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                student.PropertyChanged -= Student_PropertyChanged;
                Students.Remove(student);
                _plan.Students.Remove(student);
                _main.SavePlans();
            }
        }

        private void AddLesson()
        {
            // Znajdź zaznaczonych uczniów lub użyj wybranego ucznia
            var selectedStudents = Students.Where(s => s.IsSelected).ToList();
            if (selectedStudents.Count == 0 && SelectedStudent != null)
            {
                selectedStudents.Add(SelectedStudent);
            }

            if (selectedStudents.Count == 0) return;

            var window = new AddLessonWindow();
            if (window.ShowDialog() == true)
            {
                var lesson = new StudentLesson
                {
                    Date = window.LessonDate,
                    Hours = window.LessonHours
                };

                // Dodaj zajęcia do wszystkich zaznaczonych uczniów
                foreach (var student in selectedStudents)
                {
                    student.Lessons.Add(new StudentLesson 
                    { 
                        Date = lesson.Date, 
                        Hours = lesson.Hours 
                    });
                    student.RefreshCalculations();
                }

                RefreshSelectedStudentLessons();
                OnPropertyChanged(nameof(Students)); // Refresh to update totals
                _main.SavePlans();
            }
        }

        private void DeleteLesson(StudentLesson? lesson)
        {
            if (lesson == null || SelectedStudent == null) return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz usunąć zajęcia z dnia {lesson.Date:yyyy-MM-dd}?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SelectedStudent.Lessons.Remove(lesson);
                SelectedStudent.RefreshCalculations();
                RefreshSelectedStudentLessons();
                OnPropertyChanged(nameof(Students)); // Refresh to update totals
                _main.SavePlans();
            }
        }
    }
}
