using System.Windows;

namespace ModMyFactory.Views
{
    partial class LoginWindow
    {
        public bool useLocalToken;
        public static bool isOpened = false;
        public LoginWindow()
        {
            isOpened = true;
            InitializeComponent();
            useLocalToken = false;
        }
        private void UseLocalButtonClickHandler(object sender, RoutedEventArgs e)
        {
            useLocalToken = true;
            isOpened = false;
            DialogResult = true;
        }
        private void OKButtonClickHandler(object sender, RoutedEventArgs e)
        {
            isOpened = false;
            DialogResult = true;
        }

        private void CancelButtonClickHandler(object sender, RoutedEventArgs e)
        {
            isOpened = false;
        }
    }
}
