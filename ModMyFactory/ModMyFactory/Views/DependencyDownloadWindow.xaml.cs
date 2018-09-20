using System.Windows;

namespace ModMyFactory.Views
{
    partial class DependencyDownloadWindow
    {
        public DependencyDownloadWindow()
        {
            InitializeComponent();
        }

        private void DownloadButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
