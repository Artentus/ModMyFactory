using System;
using System.IO;

namespace ModMyFactory.Zlib
{
    static class Crc32Checksum
    {
        public static uint Generate(byte[] data, int offset, int count, uint initialChecksum = 0)
        {
            var generator = new Crc32ChecksumGenerator(initialChecksum);
            generator.Update(data, offset, count);
            return generator.CurrentChecksum;
        }

        public static uint Generate(byte[] data, uint initialChecksum = 0)
        {
            var generator = new Crc32ChecksumGenerator(initialChecksum);
            generator.Update(data);
            return generator.CurrentChecksum;
        }

        public static uint Generate(Stream data, long length, uint initialChecksum = 0)
        {
            var generator = new Crc32ChecksumGenerator(initialChecksum);

            byte[] buffer = new byte[8192];
            while (data.Position < length)
            {
                int count = data.Read(buffer, 0, (int)Math.Min(buffer.Length, length - data.Position));
                generator.Update(buffer, 0, count);
            }

            return generator.CurrentChecksum;
        }

        public static uint Generate(Stream data, uint initialChecksum = 0)
        {
            return Generate(data, data.Length, initialChecksum);
        }
    }
}
