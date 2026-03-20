using System;
using System.Windows;

namespace Kalendarz.Views
{
    public partial class AddLessonWindow : Window
    {
        public DateTime LessonDate { get; set; } = DateTime.Today;
        public int LessonHours { get; set; } = 1;

        public AddLessonWindow()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (LessonHours <= 0)
            {
                MessageBox.Show(this, "Liczba godzin musi być większa od 0.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
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
