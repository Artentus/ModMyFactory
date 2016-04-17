using System;
using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    static class User32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
