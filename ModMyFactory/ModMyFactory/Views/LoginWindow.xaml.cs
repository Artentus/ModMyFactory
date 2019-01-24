using System.Windows;

namespace ModMyFactory.Views
{
    partial class LoginWindow
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
