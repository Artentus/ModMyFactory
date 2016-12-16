using System;
using System.Windows.Shell;
using ModMyFactory.Helpers;
using ModMyFactory.ViewModels;
using ModMyFactory.Win32;

namespace ModMyFactory.Views
{
    partial class ProgressWindow
    {
        public ProgressWindow()
        {
            InitializeComponent();

            Loaded += LoadedHandler;
            Closing += ClosingHandler;
        }

        private void LoadedHandler(object sender, EventArgs e)
        {
            IntPtr handle = this.Handle();
            IntPtr windowLong = User32.GetWindowLong(handle, WindowLongIndex.Style);
            windowLong = (IntPtr)(windowLong.ToInt64() & (long)~WindowStyles.SystemMenu);
            User32.SetWindowLong(handle, WindowLongIndex.Style, windowLong);

            var viewModel = (ProgressViewModel)ViewModel;
            TaskbarItemInfo = new TaskbarItemInfo
            {
                ProgressState = viewModel.IsIndeterminate ? TaskbarItemProgressState.Indeterminate : TaskbarItemProgressState.Normal,
                ProgressValue = viewModel.Progress,
            };
        }

        private void ClosingHandler(object sender, EventArgs e)
        {
            TaskbarItemInfo.ProgressValue = 0;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            TaskbarItemInfo = null;
        }
    }
}
