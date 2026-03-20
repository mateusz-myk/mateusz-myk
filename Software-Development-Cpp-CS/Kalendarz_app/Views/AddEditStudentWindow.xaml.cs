using Kalendarz.Models;
using System;
using System.Windows;

namespace Kalendarz.Views
{
    public partial class AddEditStudentWindow : Window
    {
        private Student _student;

        public string Name
        {
            get => _student.Name;
            set => _student.Name = value;
        }

        public string LastName
        {
            get => _student.LastName;
            set => _student.LastName = value;
        }

        public string LastTopic
        {
            get => _student.LastTopic;
            set => _student.LastTopic = value;
        }

        public string DiscordName
        {
            get => _student.DiscordName;
            set => _student.DiscordName = value;
        }

        public string ClassName
        {
            get => _student.ClassName;
            set => _student.ClassName = value;
        }

        public int PaidHours
        {
            get => _student.PaidHours;
            set => _student.PaidHours = value;
        }

        public decimal HourlyRate
        {
            get => _student.HourlyRate;
            set => _student.HourlyRate = value;
        }

        public string Status
        {
            get => _student.Status;
            set => _student.Status = value;
        }

        public AddEditStudentWindow(Student student)
        {
            _student = student ?? throw new ArgumentNullException(nameof(student));
            this.DataContext = _student;
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_student.Name) || string.IsNullOrWhiteSpace(_student.LastName))
            {
                MessageBox.Show(this, "Imię i nazwisko są wymagane.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
