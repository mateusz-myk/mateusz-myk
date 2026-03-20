using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

namespace Kalendarz
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Unhandled UI exception: {e.Exception}");
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kalendarz");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "crash.log");
                File.AppendAllText(path, DateTime.Now.ToString("s") + "\n" + e.Exception.ToString() + "\n\n");
                MessageBox.Show($"Wystąpił błąd: {e.Exception.Message}\nSzczegóły zapisane: {path}", "Błąd aplikacji", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Unhandled domain exception: {e.ExceptionObject}");
            try
            {
                var ex = e.ExceptionObject as Exception;
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kalendarz");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "crash.log");
                File.AppendAllText(path, DateTime.Now.ToString("s") + "\n" + (ex?.ToString() ?? e.ExceptionObject.ToString()) + "\n\n");
                MessageBox.Show($"Wystąpił nieoczekiwany błąd: {ex?.Message}\nSzczegóły zapisane: {path}", "Błąd aplikacji", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Unobserved task exception: {e.Exception}");
            e.SetObserved();
        }
    }

}
