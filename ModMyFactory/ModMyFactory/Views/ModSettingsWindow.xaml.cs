using System.Windows;
using System.Windows.Input;

namespace ModMyFactory.Views
{
    partial class ModSettingsWindow
    {
        public ModSettingsWindow()
        {
            InitializeComponent();

            Loaded += LoadedHandler;
        }

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
