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

            if(SaveCredentialsBox.IsChecked == true)
            {
                SaveCredentialsBoxCheckedHandler(sender, e);
            }
        }

        private void OKButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SaveCredentialsBoxCheckedHandler(object sender, RoutedEventArgs e)
        {
            string tokenout = GlobalCredentials.Instance.Token;
            if (this.IsLoaded)
            {
                if (!GlobalCredentials.Instance.LogIn(this, out tokenout))
                {
                    SaveCredentialsBox.IsChecked = false;
                    return;
                }
            }
            UsernameBox.Text = GlobalCredentials.Instance.Username;
            PasswordBox.Text = GlobalCredentials.Instance.Token;
        }

        private void SaveCredentialsBoxUncheckedHandler(object sender, RoutedEventArgs e)
        {
            GlobalCredentials.Instance.Token = null;
            UsernameBox.Text = null;
            PasswordBox.Text = null;
        }
    }
}
