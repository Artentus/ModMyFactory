using System.Windows;

namespace ModMyFactory.Views
{
    partial class CopyOrMoveMessageWindow
    {
        public CopyOrMoveMessageWindow()
        {
            InitializeComponent();
        }

        public bool Move { get; private set; }

        private void CopyButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Move = false;
            Close();
        }

        private void MoveButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Move = true;
            Close();
        }
    }
}
