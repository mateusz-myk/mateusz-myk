using System;
using System.Windows.Controls;
using System.Windows;
using Kalendarz.ViewModels;
using System.ComponentModel;
using System.Windows.Input;

namespace Kalendarz.Views
{
    public partial class StudentsView : UserControl
    {
        public StudentsView()
        {
            InitializeComponent();
            this.DataContextChanged += StudentsView_DataContextChanged;
            this.Loaded += StudentsView_Loaded;

            // Ustaw focus aby UserControl mógł odbierać zdarzenia klawiatury
            this.Loaded += (s, e) => this.Focus();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is StudentsViewModel vm && vm.BackCommand.CanExecute(null))
                {
                    vm.BackCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void StudentsView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ustaw dzisiejszą datę
            if (TodayLabel != null)
                TodayLabel.Text = DateTime.Now.ToString("dddd, d MMMM yyyy", new System.Globalization.CultureInfo("pl-PL"));
        }

        private void StudentsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Odsubskrybuj stary ViewModel
            if (e.OldValue is StudentsViewModel oldVm)
            {
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;
            }

            // Zasubskrybuj nowy ViewModel
            if (e.NewValue is StudentsViewModel newVm)
            {
                newVm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StudentsViewModel.SelectedStudentLessons))
            {
                // Wymuś odświeżenie DataGrid
                LessonsDataGrid.Items.Refresh();
            }
        }

        private void StudentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is StudentsViewModel vm && vm.SelectedStudent != null)
            {
                vm.EditStudentCommand.Execute(vm.SelectedStudent);
            }
        }
    }
}
