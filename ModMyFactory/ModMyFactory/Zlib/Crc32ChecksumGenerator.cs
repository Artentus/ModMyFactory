using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ModMyFactory.Zlib
{
    sealed class Crc32ChecksumGenerator
    {
        [DllImport("zlib32.dll", EntryPoint = "crc32", CallingConvention = CallingConvention.StdCall)]
        static extern uint Crc32Native32(uint crc, IntPtr data, uint length);

        [DllImport("zlib64.dll", EntryPoint = "crc32", CallingConvention = CallingConvention.StdCall)]
        static extern uint Crc32Native64(uint crc, IntPtr data, uint length);

        private static uint Crc32(uint crc, IntPtr data, uint length)
        {
            return Environment.Is64BitProcess ? Crc32Native64(crc, data, length) : Crc32Native32(crc, data, length);
        }

        public uint CurrentChecksum { get; private set; }

        public Crc32ChecksumGenerator()
        {
            Reset();
        }

        public void Reset()
        {
            CurrentChecksum = Crc32(0, IntPtr.Zero, 0);
        }

        public void Update(byte[] data, int offset, int count)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if ((offset + count) > data.Length) throw new ArgumentException();

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                CurrentChecksum = Crc32(CurrentChecksum, handle.AddrOfPinnedObject() + offset, (uint)count);
            }
            finally
            {
                handle.Free();
            }
        }

        public void Update(byte[] data)
        {
            Update(data, 0, data.Length);
        }

        public void Update(Stream data, long byteCount)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));

            byte[] buffer = new byte[8192];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                int count;
                do
                {
                    count = data.Read(buffer, 0, (int)Math.Min(buffer.Length, byteCount - data.Position));
                    if (count > 0) CurrentChecksum = Crc32(CurrentChecksum, handle.AddrOfPinnedObject(), (uint)count);
                } while (count > 0);
            }
            finally
            {
                handle.Free();
            }
        }

        public void Update(Stream data)
        {
            Update(data, data.Length);
        }
    }
}
