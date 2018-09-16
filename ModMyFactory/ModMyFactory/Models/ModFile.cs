using ModMyFactory.Helpers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents a mod file or directory.
    /// </summary>
    sealed class ModFile
    {
        private readonly bool isFile;
        private FileSystemInfo file;
        
        /// <summary>
        /// The mods info file.
        /// </summary>
        public InfoFile InfoFile { get; }

        /// <summary>
        /// The mods name.
        /// </summary>
        public string Name => InfoFile.Name;

        /// <summary>
        /// The mods version.
        /// </summary>
        public Version Version => InfoFile.Version;

        private string BuildNewFileName(int uid)
        {
            var sb = new StringBuilder();

            if (uid >= 0)
            {
                sb.Append(uid);
                sb.Append('+');
            }

            sb.Append(Name);
            sb.Append('_');
            sb.Append(Version);

            if (isFile) sb.Append(".zip");

            return sb.ToString();
        }

        private FileSystemInfo GetNewFile(string newPath)
        {
            return isFile ? (FileSystemInfo)(new FileInfo(newPath)) : (FileSystemInfo)(new DirectoryInfo(newPath));
        }

        /// <summary>
        /// Moves this mod file to a new location.
        /// </summary>
        /// <param name="destination">The location to move the file to.</param>
        /// <param name="uid">Optional. A UID to append to the file name.</param>
        public async Task MoveToAsync(string destination, int uid = -1)
        {
            string newName = BuildNewFileName(uid);
            string newPath = Path.Combine(destination, newName);
            await file.MoveToAsync(newPath);

            file = GetNewFile(newPath);
        }

        /// <summary>
        /// Copies this mod file to a specified location.
        /// </summary>
        /// <param name="destination">The location to copy the file to.</param>
        /// <param name="uid">Optional. A UID to append to the file name.</param>
        /// <returns>Returns a new mod file object representing the copy created.</returns>
        public async Task<ModFile> CopyToAsync(string destination, int uid = -1)
        {
            string newName = BuildNewFileName(uid);
            string newPath = Path.Combine(destination, newName);
            await file.CopyToAsync(newPath);

            var newFile = GetNewFile(newPath);
            return new ModFile(newFile, InfoFile, isFile);
        }

        /// <summary>
        /// Deletes this mod file.
        /// </summary>
        public void Delete()
        {
            file.DeleteRecursive();
        }

        private ModFile(FileSystemInfo file, InfoFile infoFile, bool isFile)
        {
            this.file = file;
            InfoFile = infoFile;
            this.isFile = isFile;
        }

        /// <summary>
        /// Tries to load a file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <param name="result">Out. The loaded mod file.</param>
        /// <param name="hasUid">Optional. Specifies if the file to load contains a UID in its name.</param>
        /// <returns>Returns true if the specified file is a valid mod file, otherwise false.</returns>
        public static bool TryLoadFromFile(FileInfo file, out ModFile result, bool hasUid = false)
        {
            result = null;
            if (!file.Exists) return false;

            InfoFile infoFile;
            if (!ArchiveFileValid(file, out infoFile, hasUid)) return false;

            result = new ModFile(file, infoFile, true);
            return true;
        }

        /// <summary>
        /// Tries to load a directory.
        /// </summary>
        /// <param name="directory">The directory to load.</param>
        /// <param name="result">Out. The loaded mod file.</param>
        /// <param name="hasUid">Optional. Specifies if the directory to load contains a UID in its name.</param>
        /// <returns>Returns true if the specified directory is a valid mod directory, otherwise false.</returns>
        public static bool TryLoadFromDirectory(DirectoryInfo directory, out ModFile result, bool hasUid = false)
        {
            result = null;
            if (!directory.Exists) return false;

            InfoFile infoFile;
            if (!DirectoryValid(directory, out infoFile, hasUid)) return false;

            result = new ModFile(directory, infoFile, false);
            return true;
        }

        /// <summary>
        /// Tries to load a file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <param name="result">Out. The loaded mod file.</param>
        /// <param name="hasUid">Optional. Specifies if the file to load contains a UID in its name.</param>
        /// <returns>Returns true if the specified file is a valid mod file, otherwise false.</returns>
        public static bool TryLoad(FileSystemInfo file, out ModFile result, bool hasUid = false)
        {
            result = null;
            if (!file.Exists) return false;

            var fi = file as FileInfo;
            if (fi != null) return TryLoadFromFile(fi, out result, hasUid);

            var di = file as DirectoryInfo;
            if (di != null) return TryLoadFromDirectory(di, out result, hasUid);

            return false;
        }
        
        /// <summary>
        /// Removes the UID from a mods name, if it is specified.
        /// </summary>
        /// <param name="name">The mods name, optionally containing a UID.</param>
        /// <returns>Returns the mods name without UID.</returns>
        private static string GetNameWithoutUid(string name)
        {
            int index = name.IndexOf('+');
            if (index < 0) return name;

            return name.Substring(index + 1);
        }

        /// <summary>
        /// Tries to read mod name and mod version from a file name.
        /// The file name can contain an extension.
        /// </summary>
        /// <param name="fileName">The file name to parse.</param>
        /// <param name="name">Out. The parsed mod name.</param>
        /// <param name="version">Out. The parsed mod version.</param>
        /// <param name="hasUid">Specifies if the file name contains a UID.</param>
        /// <returns>Returns true if the file name was a valid mod file name, otherwise false.</returns>
        private static bool TryParseModName(string fileName, out string name, out Version version, bool hasUid)
        {
            name = null;
            version = null;

            fileName = Path.GetFileNameWithoutExtension(fileName);
            int index = fileName.LastIndexOf('_');
            if ((index < 1) || (index >= fileName.Length - 1)) return false;

            name = fileName.Substring(0, index);
            if (hasUid) name = GetNameWithoutUid(name);
            string versionString = fileName.Substring(index + 1);
            return Version.TryParse(versionString, out version);
        }

        /// <summary>
        /// Tries to deserialize a given stream into an info file object.
        /// </summary>
        /// <param name="stream">The stream to deserialize.</param>
        /// <param name="infoFile">Out. The deserialized info file if the operation was successful.</param>
        /// <returns>Returns true if deserialization succeeded, otherwise false.</returns>
        private static bool TryDeserializeInfoFileFromStream(Stream stream, out InfoFile infoFile)
        {
            try
            {
                infoFile = InfoFile.FromJsonStream(stream);
                return infoFile.IsValid;
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);

                infoFile = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to read an info file from an archive.
        /// </summary>
        /// <param name="archiveFile">The archive to read from.</param>
        /// <param name="infoFile">Out. The info file that has been read.</param>
        /// <returns>Returns true if an info file could be read from the archive, otherwise false.</returns>
        private static bool TryReadInfoFileFromArchive(FileInfo archiveFile, out InfoFile infoFile)
        {
            infoFile = null;

            try
            {
                using (var archive = ZipFile.OpenRead(archiveFile.FullName))
                {
                    var entry = archive.Entries.FirstOrDefault(e => e.Name == "info.json");
                    if (entry == null) return false;

                    using (var stream = entry.Open())
                        return TryDeserializeInfoFileFromStream(stream, out infoFile) && infoFile.IsValid;
                }
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);
                return false;
            }
        }

        /// <summary>
        /// Tries to read an info file from a directory.
        /// </summary>
        /// <param name="directory">The directory to read fromn.</param>
        /// <param name="infoFile">Out. The info file that has been read.</param>
        /// <returns>Returns true if an info file could be read from the directory, otherwise false.</returns>
        private static bool TryReadInfoFileFromDirectory(DirectoryInfo directory, out InfoFile infoFile)
        {
            infoFile = null;

            try
            {
                var file = directory.EnumerateFiles("info.json").FirstOrDefault();
                if (file == null) return false;

                using (var stream = file.OpenRead())
                    return TryDeserializeInfoFileFromStream(stream, out infoFile) && infoFile.IsValid;
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);
                return false;
            }
        }

        /// <summary>
        /// Determines if an archive file is a valid mod.
        /// </summary>
        /// <param name="archiveFile">The archive file to check.</param>
        /// <param name="infoFile">Out. The mods info file.</param>
        /// <param name="hasUid">Specifies if the file name contains a UID.</param>
        /// <returns>Returns true if the specified file is a valid mod, otherwise false.</returns>
        private static bool ArchiveFileValid(FileInfo archiveFile, out InfoFile infoFile, bool hasUid)
        {
            infoFile = null;

            string fileName;
            Version fileVersion;
            if (!TryParseModName(archiveFile.Name, out fileName, out fileVersion, hasUid)) return false;

            if (TryReadInfoFileFromArchive(archiveFile, out infoFile))
            {
                return (infoFile.Name == fileName) && (infoFile.Version == fileVersion);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if a directory is a valid mod.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <param name="infoFile">Out. The mods info file.</param>
        /// <param name="hasUid">Specifies if the directory name contains a UID.</param>
        /// <returns>Returns true if the specified directory is a valid mod, otherwise false.</returns>
        private static bool DirectoryValid(DirectoryInfo directory, out InfoFile infoFile, bool hasUid)
        {
            infoFile = null;

            string fileName;
            Version fileVersion;
            if (!TryParseModName(directory.Name, out fileName, out fileVersion, hasUid)) return false;

            if (TryReadInfoFileFromDirectory(directory, out infoFile))
            {
                return (infoFile.Name == fileName) && (infoFile.Version == fileVersion);
            }
            else
            {
                return false;
            }
        }
    }
}
