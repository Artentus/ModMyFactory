using System;
using System.IO;
using System.IO.Compression;

namespace ModMyFactory.Models
{
    sealed class FactorioFile
    {
        public FileInfo ArchiveFile { get; }

        public Version Version { get; }

        public bool Is64Bit { get; }

        private FactorioFile(FileInfo archiveFile, Version version, bool is64Bit)
        {
            ArchiveFile = archiveFile;
            Version = version;
            Is64Bit = is64Bit;
        }

        private static bool TryLoadVersion(ZipArchive archive, out Version version)
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(@"data/base/info.json"))
                {
                    using (var stream = entry.Open())
                    {
                        try
                        {
                            var infoFile = InfoFile.FromJsonStream(stream);
                            version = infoFile.Version;
                            return infoFile.IsValid;
                        }
                        catch (Exception ex)
                        {
                            App.Instance.WriteExceptionLog(ex);
                            version = null;
                            return false;
                        }
                    }
                }
            }

            version = null;
            return false;
        }

        private static bool TryLoadBitness(ZipArchive archive, out bool is64Bit)
        {
            const string win32BinName = "Win32";
            const string win64BinName = "x64";

            bool win32BinExists = false;
            bool win64BinExists = false;
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith($@"bin/{win32BinName}/factorio.exe"))
                {
                    win32BinExists = true;
                }
                else if (entry.FullName.EndsWith($@"bin/{win64BinName}/factorio.exe"))
                {
                    win64BinExists = true;
                }
            }

            is64Bit = win64BinExists;
            return win32BinExists || win64BinExists;
        }

        public static bool TryLoad(FileInfo archiveFile, out FactorioFile file)
        {
            file = null;
            if (!archiveFile.Exists) return false;

            try
            {
                using (var archive = ZipFile.OpenRead(archiveFile.FullName))
                {
                    if (!TryLoadVersion(archive, out var version)) return false;
                    if (!TryLoadBitness(archive, out bool is64Bit)) return false;

                    file = new FactorioFile(archiveFile, version, is64Bit);
                    return true;
                }
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);
                return false;
            }
        }
    }
}
