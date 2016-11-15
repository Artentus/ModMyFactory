using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace ModMyFactory.Win32
{
    static class Kernel32
    {
        #region Console

        const int AttachToParentProcess = -1;

        [DllImport("kernel32.dll", EntryPoint = "AttachConsole", SetLastError = true)]
        private static extern bool AttachConsoleNative(int processId);

        /// <summary>
        /// Attaches the calling process to the console of the specified process.
        /// </summary>
        /// <param name="processId">The identifier of the process whose console is to be used.</param>
        public static void AttachConsole(int processId)
        {
            if (!AttachConsoleNative(processId))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        /// <summary>
        /// Attaches the calling process to the console of its parent process.
        /// </summary>
        public static void AttachConsole()
        {
            AttachConsole(AttachToParentProcess);
        }

        /// <summary>
        /// Tries to attach the calling process to the console of the specified process.
        /// </summary>
        /// <param name="processId">The identifier of the process whose console is to be used.</param>
        public static bool TryAttachConsole(int processId)
        {
            return AttachConsoleNative(processId);
        }

        /// <summary>
        /// Tries to attach the calling process to the console of its parent process.
        /// </summary>
        public static bool TryAttachConsole()
        {
            return AttachConsoleNative(AttachToParentProcess);
        }

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true)]
        private static extern bool AllocConsoleNative();

        /// <summary>
        /// Allocates a new console for the calling process.
        /// </summary>
        public static void AllocConsole()
        {
            if (!AllocConsoleNative())
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "FreeConsole", SetLastError = true)]
        private static extern bool FreeConsoleNative();

        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        public static void FreeConsole()
        {
            if (!FreeConsoleNative())
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        #endregion

        #region DeviceIOControl

        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl",
            CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        private static extern bool DeviceIOControlNative(
            SafeFileHandle deviceHandle, IOControlCode controlCode,
            IntPtr inBuffer, int inBufferSize,
            IntPtr outBuffer, int outBufferSize,
            out int bytesReturned, IntPtr overlapped);

        private static void DeviceIOControlInternal(
            SafeFileHandle deviceHandle, IOControlCode controlCode,
            IntPtr inBuffer, int inBufferSize,
            IntPtr outBuffer, int outBufferSize,
            out int bytesReturned)
        {
            if (!DeviceIOControlNative(deviceHandle, controlCode, inBuffer, inBufferSize,
                outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        /// <summary>
        /// Sends a control code directly to a specified device driver, causing the corresponding device to perform the corresponding operation.
        /// </summary>
        /// <param name="deviceHandle">
        /// A handle to the device on which the operation is to be performed. The device is typically a volume, directory, file, or stream.
        /// To retrieve a device handle, use the CreateFile function.
        /// </param>
        /// <param name="controlCode">
        /// The control code for the operation. This value identifies the specific operation to be performed and the type of device on which to perform it.
        /// </param>
        /// <param name="inBuffer">
        /// A pointer to the input buffer that contains the data required to perform the operation.
        /// The format of this data depends on the value of the controlCode parameter.
        /// </param>
        /// <param name="inBufferSize">
        /// The size of the input buffer, in bytes.
        /// </param>
        /// <param name="outBuffer">
        /// A pointer to the output buffer that is to receive the data returned by the operation.
        /// The format of this data depends on the value of the controlCode parameter.
        /// </param>
        /// <param name="outBufferSize">
        /// The size of the output buffer, in bytes.
        /// </param>
        /// <param name="bytesReturned">
        /// Out. The size of the data stored in the output buffer, in bytes.
        /// </param>
        public static void DeviceIOControl(
            SafeFileHandle deviceHandle, IOControlCode controlCode,
            IntPtr inBuffer, int inBufferSize,
            IntPtr outBuffer, int outBufferSize,
            out int bytesReturned)
        {
            DeviceIOControlInternal(deviceHandle, controlCode, inBuffer, inBufferSize, outBuffer, outBufferSize, out bytesReturned);
        }

        /// <summary>
        /// Sends a control code directly to a specified device driver, causing the corresponding device to perform the corresponding operation.
        /// </summary>
        /// <param name="deviceHandle">
        /// A handle to the device on which the operation is to be performed. The device is typically a volume, directory, file, or stream.
        /// To retrieve a device handle, use the CreateFile function.
        /// </param>
        /// <param name="controlCode">
        /// The control code for the operation. This value identifies the specific operation to be performed and the type of device on which to perform it.
        /// </param>
        public static void DeviceIOControl(SafeFileHandle deviceHandle, IOControlCode controlCode)
        {
            int bytesReturned;
            DeviceIOControlInternal(deviceHandle, controlCode, IntPtr.Zero, 0, IntPtr.Zero, 0, out bytesReturned);
        }

        #endregion

        #region CreateFile

        [StructLayout(LayoutKind.Sequential)]
        private struct SecurityAttributes
        {
            public int Length;
            public IntPtr SecurityDescriptor;
            public bool InheritHandle;
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW",
            CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern SafeFileHandle CreateFileNative(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            [MarshalAs(UnmanagedType.Struct), In] ref SecurityAttributes securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode mode,
            [MarshalAs(UnmanagedType.U4)] uint attributes,
            IntPtr templateFile);

        private static SafeFileHandle CreateFileInternal(string path, uint access, FileShare share, FileMode mode, uint attributes)
        {
            var securityAttributes = new SecurityAttributes();
            securityAttributes.Length = Marshal.SizeOf(securityAttributes);
            securityAttributes.SecurityDescriptor = IntPtr.Zero;
            securityAttributes.InheritHandle = false;

            if (share.HasFlag(FileShare.Inheritable))
            {
                share &= ~FileShare.Inheritable;
                securityAttributes.InheritHandle = true;
            }

            SafeFileHandle fileHandle = CreateFileNative(path, access, share, ref securityAttributes, mode, attributes, IntPtr.Zero);

            if (fileHandle.IsInvalid)
            {
                fileHandle.Close();

                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }

            return fileHandle;
        }

        /// <summary>
        /// Creates or opens a file or I/O device. The most commonly used I/O devices are as follows: file, file stream, directory, physical disk, volume, console buffer, tape drive, communications resource, mailslot, and pipe.
        /// The function returns a handle that can be used to access the file or device for various types of I/O depending on the file or device and the flags and attributes specified.
        /// </summary>
        /// <param name="path">
        /// The name of the file or device to be created or opened. You may use either forward slashes (/) or backslashes (\) in this name.
        /// </param>
        /// <param name="access">
        /// The requested access to the file or device, which can be summarized as read, write, both or neither.
        /// </param>
        /// <param name="share">
        /// The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none.
        /// Access requests to attributes or extended attributes are not affected by this flag.
        /// </param>
        /// <param name="mode">
        /// An action to take on a file or device that exists or does not exist.
        /// For devices other than files, this parameter is usually set to FileMode.Open.
        /// </param>
        /// <param name="attributes">
        /// The file or device attributes.
        /// </param>
        /// <param name="flags">
        /// The file or device flags.
        /// </param>
        /// <returns>If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot.</returns>
        public static SafeFileHandle CreateFile(string path, FileAccessRights access, FileShare share, FileMode mode, FileAttributes attributes, FileFlags flags)
        {
            return CreateFileInternal(path, (uint)access, share, mode, (uint)attributes | (uint)flags);
        }

        /// <summary>
        /// Creates or opens a file or I/O device. The most commonly used I/O devices are as follows: file, file stream, directory, physical disk, volume, console buffer, tape drive, communications resource, mailslot, and pipe.
        /// The function returns a handle that can be used to access the file or device for various types of I/O depending on the file or device and the flags and attributes specified.
        /// </summary>
        /// <param name="path">
        /// The name of the file or device to be created or opened. You may use either forward slashes (/) or backslashes (\) in this name.
        /// </param>
        /// <param name="access">
        /// The requested access to the file or device, which can be summarized as read, write, both or neither.
        /// </param>
        /// <param name="share">
        /// The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none.
        /// Access requests to attributes or extended attributes are not affected by this flag.
        /// </param>
        /// <param name="mode">
        /// An action to take on a file or device that exists or does not exist.
        /// For devices other than files, this parameter is usually set to FileMode.Open.
        /// </param>
        /// <param name="attributes">
        /// The file or device attributes.
        /// </param>
        /// <param name="flags">
        /// The file or device flags.
        /// </param>
        /// <returns>If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot.</returns>
        public static SafeFileHandle CreateFile(string path, GenericAccessRights access, FileShare share, FileMode mode, FileAttributes attributes, FileFlags flags)
        {
            return CreateFileInternal(path, (uint)access, share, mode, (uint)attributes | (uint)flags);
        }

        #endregion

        #region RemoveDirectory

        [DllImport("kernel32.dll", EntryPoint = "RemoveDirectoryW",
            CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool RemoveDirectoryNative(string path);

        /// <summary>
        /// Deletes an existing empty directory.
        /// </summary>
        /// <param name="path">The path of the directory to be removed. This path must specify an empty directory, and the calling process must have delete access to the directory.</param>
        public static void RemoveDirectory(string path)
        {
            if (!RemoveDirectoryNative(path))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        #endregion

        #region GetFileAttributes

        [DllImport("kernel32.dll", EntryPoint = "GetFileAttributesW",
            CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern int GetFileAttributesNative(string path);

        /// <summary>
        /// Retrieves file system attributes for a specified file or directory.
        /// </summary>
        /// <param name="path">The name of the file or directory.</param>
        /// <returns>Returns the attributes of the specified file or directory.</returns>
        public static FileAttributes GetFileAttributes(string path)
        {
            const int invalidFileAttributes = -1;

            int result = GetFileAttributesNative(path);
            if (result == invalidFileAttributes)
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }

            return (FileAttributes)result;
        }

        /// <summary>
        /// Tries to retrieve file system attributes for a specified file or directory.
        /// </summary>
        /// <param name="path">The name of the file or directory.</param>
        /// <param name="attributes">Out. If the function succeeds, contains the attributes of the specified file or directory.</param>
        /// <returns>Returns true if the function succeeds, othwerwise false.</returns>
        public static bool TryGetFileAttributes(string path, out FileAttributes attributes)
        {
            const int invalidFileAttributes = -1;

            int result = GetFileAttributesNative(path);
            if (result == invalidFileAttributes)
            {
                attributes = FileAttributes.Normal;
                return false;
            }

            attributes = (FileAttributes)result;
            return true;
        }

        #endregion

        #region CloseHandle

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool CloseHandleNative(IntPtr handle);

        /// <summary>
        /// Closes an open object handle.
        /// </summary>
        /// <param name="handle">A valid handle to an open object.</param>
        public static void CloseHandle(IntPtr handle)
        {
            if (!CloseHandleNative(handle))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        #endregion
    }
}
