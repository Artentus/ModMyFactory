using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ModMyFactory.Win32
{
    static class AdvApi32
    {
        #region OpenProcessToken

        [DllImport("advapi32.dll", EntryPoint = "OpenProcessToken", SetLastError = true)]
        private static extern bool OpenProcessTokenNative(IntPtr processHandle, TokenAccessLevels desiredAccess, out IntPtr tokenHandle);

        /// <summary>
        /// The OpenProcessToken function opens the access token associated with a process.
        /// </summary>
        /// <param name="processHandle">A handle to the process whose access token is opened. The process must have the PROCESS_QUERY_INFORMATION access permission.</param>
        /// <param name="desiredAccess">
        /// Specifies an access mask that specifies the requested types of access to the access token.
        /// These requested access types are compared with the discretionary access control list (DACL) of the token to determine which accesses are granted or denied.
        /// </param>
        /// <returns>Returns a handle that identifies the newly opened access token.</returns>
        public static IntPtr OpenProcessToken(IntPtr processHandle, TokenAccessLevels desiredAccess)
        {
            IntPtr tokenHandle;
            if (!OpenProcessTokenNative(processHandle, desiredAccess, out tokenHandle))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }

            return tokenHandle;
        }

        #endregion

        #region LookupPrivilegeValue

        [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueW",
            CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool LookupPrivilegeValueNative(string systemName, string name, out Luid luid);

        private static Luid LookupPrivilegeValueInternal(string name, string systemName)
        {
            Luid luid;
            if (!LookupPrivilegeValueNative(systemName, name, out luid))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }

            return luid;
        }

        /// <summary>
        /// Retrieves the locally unique identifier (LUID) used on a specified system to locally represent the specified privilege name.
        /// </summary>
        /// <param name="name">A  string that specifies the name of the privilege.</param>
        /// <param name="systemName">A string that specifies the name of the system on which the privilege name is retrieved.</param>
        /// <returns>Returns the LUID by which the privilege is known on the system specified by the systemName parameter.</returns>
        public static Luid LookupPrivilegeValue(string name, string systemName)
        {
            return LookupPrivilegeValueInternal(name, systemName);
        }

        /// <summary>
        /// Retrieves the locally unique identifier (LUID) used on the local system to represent the specified privilege name.
        /// </summary>
        /// <param name="name">A  string that specifies the name of the privilege.</param>
        /// <returns>Returns the LUID by which the privilege is known on the local system.</returns>
        public static Luid LookupPrivilegeValue(string name)
        {
            return LookupPrivilegeValueInternal(name, null);
        }

        #endregion

        #region AdjustTokenPrivileges

        [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges", SetLastError = true)]
        private static extern bool AdjustTokenPrivilegesNative(
            IntPtr tokenHandle, bool disableAllPrivileges,
            [In] ref TokenPrivileges newState,
            int bufferLength, [In, Out] ref TokenPrivileges previousState, out int returnLength);

        [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges", SetLastError = true)]
        private static extern bool AdjustTokenPrivilegesNative(
            IntPtr tokenHandle, bool disableAllPrivileges,
            [In] ref TokenPrivileges newState,
            int bufferLength, IntPtr previousState, IntPtr returnLength);

        [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges", SetLastError = true)]
        private static extern bool AdjustTokenPrivilegesNative(
            IntPtr tokenHandle, bool disableAllPrivileges,
            IntPtr newState,
            int bufferLength, IntPtr previousState, IntPtr returnLength);

        /// <summary>
        /// Enables or disables privileges in the specified access token.
        /// </summary>
        /// <param name="tokenHandle">A handle to the access token that contains the privileges to be modified.</param>
        /// <param name="newState">A TokenPrivileges structure that specifies an array of privileges and their attributes.</param>
        /// <param name="previousState">Out. A TokenPrivileges structure that contains the previous state of any privileges that the function modifies.</param>
        public static void AdjustTokenPrivileges(
            IntPtr tokenHandle,
            TokenPrivileges newState,
            out TokenPrivileges previousState)
        {
            var previous = new TokenPrivileges();
            int returnLength;
            if (!AdjustTokenPrivilegesNative(tokenHandle, false, ref newState,
                Marshal.SizeOf(previous), ref previous, out returnLength))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
            previousState = previous;
        }

        /// <summary>
        /// Enables or disables privileges in the specified access token.
        /// </summary>
        /// <param name="tokenHandle">A handle to the access token that contains the privileges to be modified.</param>
        /// <param name="newState">A TokenPrivileges structure that specifies an array of privileges and their attributes.</param>
        public static void AdjustTokenPrivileges(IntPtr tokenHandle, TokenPrivileges newState)
        {
            if (!AdjustTokenPrivilegesNative(tokenHandle, false, ref newState,
                0, IntPtr.Zero, IntPtr.Zero))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        /// <summary>
        /// Disables all privileges in the specified access token.
        /// </summary>
        /// <param name="tokenHandle">A handle to the access token that contains the privileges to be modified.</param>
        public static void DisableTokenPrivileges(IntPtr tokenHandle)
        {
            if (!AdjustTokenPrivilegesNative(tokenHandle, true, IntPtr.Zero,
                0, IntPtr.Zero, IntPtr.Zero))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        #endregion
    }
}
