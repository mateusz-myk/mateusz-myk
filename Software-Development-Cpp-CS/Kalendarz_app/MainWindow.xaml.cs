using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kalendarz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Ustawienie rozmiaru okna na ~90% szerokości i ~95% wysokości rozdzielczości ekranu i wyśrodkowanie
            this.Width = SystemParameters.PrimaryScreenWidth * 0.7;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.84;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Ustawienie głównego ViewModelu aplikacji (punkt wejścia MVVM)
            var vm = new ViewModels.MainViewModel();
            this.DataContext = vm;
            // Start bezpośrednio w planie uczelniany — obsługiwane przez MainViewModel
        }
    }
}