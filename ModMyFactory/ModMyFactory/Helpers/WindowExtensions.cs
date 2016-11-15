using System;
using System.Windows;
using System.Windows.Interop;

namespace ModMyFactory.Helpers
{
    static class WindowExtensions
    {
        public static IntPtr Handle(this Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }
    }
}
