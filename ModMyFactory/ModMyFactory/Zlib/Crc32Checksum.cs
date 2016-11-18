using System.IO;

namespace ModMyFactory.Zlib
{
    static class Crc32Checksum
    {
        public static uint Generate(byte[] data, int offset, int count)
        {
            var generator = new Crc32ChecksumGenerator();
            generator.Update(data, offset, count);
            return generator.CurrentChecksum;
        }

        public static uint Generate(byte[] data)
        {
            var generator = new Crc32ChecksumGenerator();
            generator.Update(data);
            return generator.CurrentChecksum;
        }

        public static uint Generate(Stream data, long byteCount)
        {
            var generator = new Crc32ChecksumGenerator();
            generator.Update(data, byteCount);
            return generator.CurrentChecksum;
        }

        public static uint Generate(Stream data)
        {
            var generator = new Crc32ChecksumGenerator();
            generator.Update(data);
            return generator.CurrentChecksum;
        }
    }
}
