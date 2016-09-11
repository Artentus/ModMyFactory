using System.Windows;

namespace ModMyFactory
{
    public partial class VersionListWindow : Window
    {
        public VersionListWindow()
        {
            InitializeComponent();
        }

        private void AddButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
