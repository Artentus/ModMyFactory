using System;
using System.Runtime.InteropServices;

namespace ModMyFactory.Helpers
{
    static class ShellHelper
    {
        /// <summary>
        /// Creates a shortcut.
        /// </summary>
        /// <param name="filePath">The path to the shortcut.</param>
        /// <param name="target">The shortcuts target.</param>
        /// <param name="arguments">The shortcuts arguments.</param>
        /// <param name="iconLocation">The shortcuts icon.</param>
        public static void CreateShortcut(string filePath, string target, string arguments, string iconLocation)
        {
            // Create the windows script host shell object.
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            dynamic shell = Activator.CreateInstance(t);

            try
            {
                dynamic shortcut = shell.CreateShortcut(filePath);

                try
                {
                    shortcut.TargetPath = target;
                    shortcut.Arguments = arguments;
                    shortcut.IconLocation = iconLocation;
                    shortcut.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shortcut);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
