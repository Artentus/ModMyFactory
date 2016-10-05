using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    static class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(uint processId);

        public static bool AttachConsole()
        {
            return AttachConsole(uint.MaxValue);
        }

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
    }
}
