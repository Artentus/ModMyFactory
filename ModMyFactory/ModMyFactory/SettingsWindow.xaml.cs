using System.Windows;

namespace ModMyFactory
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
