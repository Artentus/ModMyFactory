using System.Windows;

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
    }
}
