using System.Windows;

namespace ModMyFactory.Views
{
    partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 0;
        }

        private void OKButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SaveCredentialsBoxCheckedHandler(object sender, RoutedEventArgs e)
        {
            UsernameBox.Text = GlobalCredentials.Instance.Username;
        }

        private void SaveCredentialsBoxUncheckedHandler(object sender, RoutedEventArgs e)
        {
            UsernameBox.Text = null;
            PasswordBox.Password = null;
        }
    }
}
