using System.ComponentModel;
using System.Windows;

namespace ModMyFactory.Views
{
    partial class OnlineModsWindow
    {
        public OnlineModsWindow()
        {
            InitializeComponent();

            WindowInfo windowInfo = App.Instance.Settings.OnlineModsWindowInfo;
            if (windowInfo == WindowInfo.Empty)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowState = windowInfo.State;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Width = windowInfo.Width;
                Height = windowInfo.Height;
                Left = windowInfo.PosX;
                Top = windowInfo.PosY;
            }

            Closing += ClosingHandler;
        }

        private void ClosingHandler(object sender, CancelEventArgs e)
        {
            var windowInfo = new WindowInfo();
            if (WindowState == WindowState.Normal)
            {
                windowInfo.PosX = (int)Left;
                windowInfo.PosY = (int)Top;
                windowInfo.Width = (int)Width;
                windowInfo.Height = (int)Height;
            }
            else
            {
                windowInfo.PosX = (int)RestoreBounds.Left;
                windowInfo.PosY = (int)RestoreBounds.Top;
                windowInfo.Width = (int)RestoreBounds.Width;
                windowInfo.Height = (int)RestoreBounds.Height;
            }
            windowInfo.State = WindowState == WindowState.Maximized
                ? WindowState.Maximized
                : WindowState.Normal;

            App.Instance.Settings.OnlineModsWindowInfo = windowInfo;
            App.Instance.Settings.Save();
        }
    }
}
