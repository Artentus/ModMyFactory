using System.Text;
using System.Windows;
using ModMyFactory.Helpers;

namespace ModMyFactory.Views
{
    partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OKButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        void SaveCredentialsBoxCheckedHandler(object sender, RoutedEventArgs e)
        {
            UsernameBox.Text = GlobalCredentials.Instance.Username;
        }

        void SaveCredentialsBoxUncheckedHandler(object sender, RoutedEventArgs e)
        {
            UsernameBox.Text = null;
            PasswordBox.Password = null;
        }
    }
}
