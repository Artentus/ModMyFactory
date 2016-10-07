using System.Windows;

namespace ModMyFactory.Views
{
    partial class ModUpdateWindow
    {
        public ModUpdateWindow()
        {
            InitializeComponent();
        }

        private void UpdateButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
