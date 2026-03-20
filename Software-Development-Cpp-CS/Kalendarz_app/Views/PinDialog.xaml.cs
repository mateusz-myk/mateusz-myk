using System.Windows;

namespace Kalendarz.Views
{
    public partial class PinDialog : Window
    {
        private const string CorrectPin = "6490";
        
        public bool IsPinCorrect { get; private set; }

        public PinDialog()
        {
            InitializeComponent();
            PinPasswordBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var enteredPin = PinPasswordBox.Password;
            
            if (enteredPin == CorrectPin)
            {
                IsPinCorrect = true;
                DialogResult = true;
                Close();
            }
            else
            {
                IsPinCorrect = false;
                ErrorLabel.Text = "Nieprawidłowy PIN!";
                ErrorLabel.Visibility = Visibility.Visible;
                PinPasswordBox.Clear();
                PinPasswordBox.Focus();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsPinCorrect = false;
            DialogResult = false;
            Close();
        }
    }
}
