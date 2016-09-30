using System.Windows;

namespace ModMyFactory.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void OKButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
