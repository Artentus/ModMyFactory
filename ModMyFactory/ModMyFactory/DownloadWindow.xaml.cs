using System;
using System.Windows;
using System.Windows.Interop;
using ModMyFactory.Win32;

namespace ModMyFactory
{
    public partial class DownloadWindow : Window
    {
        public DownloadWindow()
        {
            InitializeComponent();
        }

        private void LoadedHandler(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            IntPtr windowLong = User32.GetWindowLong(handle, WindowLongIndex.Style);
            windowLong = (IntPtr)(windowLong.ToInt64() & (long)~WindowStyles.SystemMenu);
            User32.SetWindowLong(handle, WindowLongIndex.Style, windowLong);
        }
    }
}
