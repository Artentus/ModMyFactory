using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;
using ModMyFactory.Win32;

namespace ModMyFactory.IO
{
    static class Junction
    {
        const FileAttributes JunctionAttributes = FileAttributes.Directory | FileAttributes.ReparsePoint;
        const string DestinationPrefix = @"\??\";

        [StructLayout(LayoutKind.Sequential)]
        private struct ReparseDataBuffer
        {
            public const int HeaderSize = sizeof(uint) + 2 * sizeof(ushort);
            public const int MaximumSize = 16 * 1024; // 16 kB

            public ReparseTagType ReparseTag;
            public ushort ReparseDataLength;
            private ushort Reserved;
            public ushort SubstituteNameOffset;
            public ushort SubstituteNameLength;
            public ushort PrintNameOffset;
            public ushort PrintNameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
        }

        private enum ReparseTagType : uint
        {
            MountPoint = 0xA0000003,
            Hsm = 0xC0000004,
            Sis = 0x80000007,
            Dfs = 0x8000000A,
            Symlink = 0xA000000C,
            Dfsr = 0x80000012,
        }

        private static SafeFileHandle OpenReparsePoint(string path, bool writeAccess)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                IntPtr tokenHandle = IntPtr.Zero;
                try
                {
                    tokenHandle = AdvApi32.OpenProcessToken(currentProcess.Handle, TokenAccessLevels.AdjustPrivileges | TokenAccessLevels.Query);

                    Luid luid = AdvApi32.LookupPrivilegeValue(writeAccess ? PrivilegeName.Restore : PrivilegeName.Backup);
                    var privileges = new TokenPrivileges
                    {
                        PrivilegeCount = 1,
                        Privileges = new[]
                        {
                            new Privilege()
                            {
                                Luid = luid,
                                Attributes = Privilege.EnabledAttribute,
                            }
                        }
                    };

                    AdvApi32.AdjustTokenPrivileges(tokenHandle, privileges);
                }
                finally
                {
                    if (tokenHandle != IntPtr.Zero)
                        Kernel32.CloseHandle(tokenHandle);
                }
            }

            GenericAccessRights access = GenericAccessRights.Read;
            if (writeAccess) access |= GenericAccessRights.Write;

            SafeFileHandle handle = Kernel32.CreateFile(path, access, FileShare.None,
                FileMode.Open, FileAttributes.Normal, FileFlags.OpenReparsePoint | FileFlags.BackupSemantics);
            return handle;
        }

        private static ReparseDataBuffer GetReparseData(string path)
        {
            using (SafeFileHandle reparsePointHandle = OpenReparsePoint(path, false))
            {
                IntPtr buffer = Marshal.AllocHGlobal(ReparseDataBuffer.MaximumSize);
                try
                {
                    int bytesReturned;
                    Kernel32.DeviceIOControl(reparsePointHandle, IOControlCode.FsctlGetReparsePoint,
                        IntPtr.Zero, 0, buffer, ReparseDataBuffer.MaximumSize, out bytesReturned);

                    ReparseDataBuffer data = Marshal.PtrToStructure<ReparseDataBuffer>(buffer);
                    return data;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        private static void SetReparseData(string path, ReparseDataBuffer data)
        {
            using (SafeFileHandle reparsePointHandle = OpenReparsePoint(path, true))
            {
                IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf(data));
                try
                {
                    Marshal.StructureToPtr(data, buffer, false);

                    int bytesReturned;
                    Kernel32.DeviceIOControl(reparsePointHandle, IOControlCode.FsctlSetReparsePoint,
                        buffer, ReparseDataBuffer.HeaderSize + data.ReparseDataLength, IntPtr.Zero, 0, out bytesReturned);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory junction on disk.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>Returns true if path refers to an existing directory junction, othwerwise false.</returns>
        public static bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            FileAttributes attributes;
            if (!Kernel32.TryGetFileAttributes(path, out attributes))
            {
                return false;
            }

            if ((attributes & JunctionAttributes) != JunctionAttributes)
            {
                return false;
            }

            ReparseDataBuffer data = GetReparseData(path);
            return data.ReparseTag == ReparseTagType.MountPoint;
        }

        /// <summary>
        /// Deletes a specified directory junction.
        /// </summary>
        /// <param name="path">The name of the directory junction to remove.</param>
        public static void Delete(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (!Junction.Exists(path))
                throw new IOException($"The path '{path}' does not exist or does not point to a valid directory junction.");

            Kernel32.RemoveDirectory(path);
        }

        public static string GetDestination(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            FileAttributes attributes;
            if (!Kernel32.TryGetFileAttributes(path, out attributes))
            {
                throw new IOException($"The path '{path}' does not exist.");
            }

            if ((attributes & JunctionAttributes) == JunctionAttributes)
            {
                ReparseDataBuffer data = GetReparseData(path);
                if (data.ReparseTag == ReparseTagType.MountPoint)
                {
                    string destination = Encoding.Unicode.GetString(data.PathBuffer, data.SubstituteNameOffset, data.SubstituteNameLength);
                    if (destination.StartsWith(DestinationPrefix)) destination = destination.Substring(DestinationPrefix.Length);
                    return destination;
                }
            }

            throw new IOException($"The path '{path}' does not point to a valid directory junction.");
        }

        public static void SetDestination(string path, string destination)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentNullException(nameof(destination));

            FileAttributes attributes;
            if (!Kernel32.TryGetFileAttributes(path, out attributes))
            {
                throw new IOException($"The path '{path}' does not exist.");
            }

            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                destination = Path.GetFullPath(destination);
                string print = destination;
                destination = DestinationPrefix + destination;

                int destinationLength = Encoding.Unicode.GetByteCount(destination);
                int printLength = Encoding.Unicode.GetByteCount(print);

                var data = new ReparseDataBuffer()
                {
                    ReparseTag = ReparseTagType.MountPoint,
                    ReparseDataLength = (ushort)(4 * sizeof(ushort) + destinationLength + sizeof(char) + printLength + sizeof(char)),
                    SubstituteNameOffset = 0,
                    SubstituteNameLength = (ushort)destinationLength,
                    PrintNameOffset = (ushort)(destinationLength + sizeof(char)),
                    PrintNameLength = (ushort)printLength,
                    PathBuffer = new byte[0x3FF0]
                };

                Encoding.Unicode.GetBytes(destination, 0, destination.Length, data.PathBuffer, 0);
                Encoding.Unicode.GetBytes(print, 0, print.Length, data.PathBuffer, destinationLength + sizeof(char));

                SetReparseData(path, data);
                return;
            }

            throw new IOException($"The path '{path}' does not point to a valid directory.");
        }

        /// <summary>
        /// Creates the specified director junction.
        /// Creates all the directories in the specified path.
        /// </summary>
        /// <param name="path">The directory junction to create.</param>
        /// <param name="destination">The destination of the junction.</param>
        public static void Create(string path, string destination)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentNullException(nameof(destination));

            if (Junction.Exists(path))
            {
                destination = Path.GetFullPath(destination);
                SetDestination(path, destination);
            }
            else if (File.Exists(path))
            {
                throw new InvalidOperationException($"The path '{path}' already exists but points to a file.");
            }
            else if (Directory.Exists(path))
            {
                throw new InvalidOperationException($"The path '{path}' already exists but points to a directory.");
            }
            else
            {
                Directory.CreateDirectory(path);
                destination = Path.GetFullPath(destination);
                SetDestination(path, destination);
            }
        }

        /// <summary>
        /// Creates the specified director junction.
        /// Creates all the directories in the specified path.
        /// </summary>
        /// <param name="path">The directory junction to create.</param>
        /// <param name="destination">The destination of the junction.</param>
        public static JunctionInfo CreateJunction(string path, string destination)
        {
            Create(path, destination);
            return new JunctionInfo(path);
        }
    }
}
