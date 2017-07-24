using System.Windows;
using System.Windows.Forms;
using WPFCore.Windows;

namespace ModMyFactory.Controls
{
    abstract class LayoutRoundingWindow : ViewModelBoundWindow
    {
        protected LayoutRoundingWindow()
        {
            UseLayoutRounding = true;
        }

        protected LayoutRoundingWindow(WindowInfo info, int defaultWidth, int defaultHeight)
            : this()
        {
            if (info == WindowInfo.Empty)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

                Width = defaultWidth;
                Height = defaultHeight;
            }
            else
            {
                WindowState = info.State;
                WindowStartupLocation = WindowStartupLocation.Manual;

                if (info.IsInScreenBounds)
                {
                    Width = info.Width;
                    Height = info.Height;
                    Left = info.PosX;
                    Top = info.PosY;
                }
                else
                {
                    var bounds = Screen.PrimaryScreen.WorkingArea;
                    if ((info.Width <= bounds.Width) && (info.Height <= bounds.Height))
                    {
                        Width = info.Width;
                        Height = info.Height;
                    }
                    else
                    {
                        Width = bounds.Width * 0.9;
                        Height = bounds.Height * 0.9;
                    }

                    Left = (bounds.Width - Width) / 2;
                    Top = (bounds.Height - Height) / 2;
                }
            }
        }

        public WindowInfo CreateInfo()
        {
            var info = new WindowInfo();
            if (WindowState == WindowState.Normal)
            {
                info.PosX = (int)Left;
                info.PosY = (int)Top;
                info.Width = (int)Width;
                info.Height = (int)Height;
            }
            else
            {
                info.PosX = (int)RestoreBounds.Left;
                info.PosY = (int)RestoreBounds.Top;
                info.Width = (int)RestoreBounds.Width;
                info.Height = (int)RestoreBounds.Height;
            }
            info.State = (WindowState == WindowState.Maximized) ? WindowState.Maximized : WindowState.Normal;

            return info;
        }
    }
}
