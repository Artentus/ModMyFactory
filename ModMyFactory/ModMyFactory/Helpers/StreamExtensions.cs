using ModMyFactory.ModSettings.Serialization;
using System.IO;

namespace ModMyFactory.Helpers
{
    static class StreamExtensions
    {
        public static BinaryVersion ReadBinaryVersion(this BinaryReader reader)
        {
            ushort main = reader.ReadUInt16();
            ushort major = reader.ReadUInt16();
            ushort minor = reader.ReadUInt16();
            ushort revision = reader.ReadUInt16();
            return new BinaryVersion(main, major, minor, revision);
        }

        public static void Write(this BinaryWriter writer, BinaryVersion version)
        {
            writer.Write(version.Main);
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Revision);
        }
    }
}
