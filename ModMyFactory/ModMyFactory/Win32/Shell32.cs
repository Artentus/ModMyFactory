using System;
using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    static class Shell32
    {
        [DllImport("shell32.dll", EntryPoint = "SHChangeNotify")]
        public static extern void ChangeNotify(ChangeNotifyEventId eventId, ChangeNotifyFlags flags, IntPtr item1, IntPtr item2);

        public static void ChangeNotify(ChangeNotifyEventId eventId, ChangeNotifyFlags flags)
        {
            ChangeNotify(eventId, flags, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
