using System;
using System.Runtime.InteropServices;

namespace ModMyFactory.Zlib
{
    sealed class Crc32ChecksumGenerator
    {
        [DllImport("zlib1.dll", EntryPoint = "crc32", CallingConvention = CallingConvention.Cdecl)]
        static extern uint Crc32Native(uint crc, IntPtr data, uint length);

        public uint CurrentChecksum { get; private set; }

        public Crc32ChecksumGenerator(uint initialChecksum = 0)
        {
            CurrentChecksum = initialChecksum;
        }

        public void Reset(uint checksum = 0)
        {
            CurrentChecksum = checksum;
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
                CurrentChecksum = Crc32Native(CurrentChecksum, handle.AddrOfPinnedObject() + offset, (uint)count);
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
    }
}
