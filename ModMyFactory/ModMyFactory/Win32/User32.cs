using System;
using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    static class User32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern int GetWindowLong32(int hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Retrieves information about the specified window. The function also retrieves the value at a specified offset into the extra window memory.
        /// </summary>
        /// <param name="windowHandle">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="index">The zero-based offset to the value to be retrieved. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer.</param>
        /// <returns>
        /// If the function succeeds, the return value is the requested value.
        /// If the function fails, the return value is zero.To get extended error information, call GetLastError.
        /// </returns>
        public static IntPtr GetWindowLong(IntPtr windowHandle, WindowLongIndex index)
        {
            return Environment.Is64BitProcess ? GetWindowLong64(windowHandle, (int)index) : (IntPtr)GetWindowLong32(windowHandle.ToInt32(), (int)index);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong32(int hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets a value at the specified offset in the extra window memory.
        /// </summary>
        /// <param name="windowHandle">A handle to the window and, indirectly, the class to which the window belongs. The SetWindowLongPtr function fails if the process that owns the window specified by the hWnd parameter is at a higher process privilege in the UIPI hierarchy than the process the calling thread resides in.</param>
        /// <param name="index">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer.</param>
        /// <param name="newLong">The replacement value.</param>
        /// <returns>
        /// If the function succeeds, the return value is the previous value of the specified offset.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        public static IntPtr SetWindowLong(IntPtr windowHandle, WindowLongIndex index, IntPtr newLong)
        {
            return Environment.Is64BitProcess ? SetWindowLong64(windowHandle, (int)index, newLong) : (IntPtr)SetWindowLong32(windowHandle.ToInt32(), (int)index, newLong.ToInt32());
        }
    }
}
