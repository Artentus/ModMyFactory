using System;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace ModMyFactory.Models
{
    partial class ModFile
    {
        private class ArchiveSettingsScriptLoader : ScriptLoaderBase
        {
            readonly Func<ZipArchive, string, string> callback;
            readonly ZipArchive archive;
            readonly ModCollection parentCollection;
            readonly InfoFile infoFile;

            public ArchiveSettingsScriptLoader(Func<ZipArchive, string, string> callback, ZipArchive archive, ModCollection parentCollection, InfoFile infoFile)
            {
                this.callback = callback;
                this.archive = archive;
                this.parentCollection = parentCollection;
                this.infoFile = infoFile;
            }

            public override object LoadFile(string file, Table globalContext)
            {
                const string pattern = @"__(?<name>.*)__\/";
                var matches = Regex.Matches(file, pattern);
                if (matches.Count > 0)
                {
                    var match = matches[0];
                    file = file.Substring(match.Length);
                    string modName = match.Groups["name"].Value;

                    var dependency = infoFile.Dependencies.FirstOrDefault(item => item.ModName == modName);
                    if (dependency == null) return string.Empty;

                    if (dependency.IsPresent(parentCollection, infoFile.FactorioVersion, out Mod mod))
                    {
                        return mod.File.LoadSettingsFile(file);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return callback(archive, file);
                }
            }

            public override bool ScriptFileExists(string name)
            {
                return true;
            }
        }

        private class DirectorySettingsScriptLoader : ScriptLoaderBase
        {
            readonly Func<string, string> callback;
            readonly ModCollection parentCollection;
            readonly InfoFile infoFile;

            public DirectorySettingsScriptLoader(Func<string, string> callback, ModCollection parentCollection, InfoFile infoFile)
            {
                this.callback = callback;
                this.parentCollection = parentCollection;
                this.infoFile = infoFile;
            }

            public override object LoadFile(string file, Table globalContext)
            {
                const string pattern = @"__(?<name>.*)__\/";
                var matches = Regex.Matches(file, pattern);
                if (matches.Count > 0)
                {
                    var match = matches[0];
                    file = file.Substring(match.Length);
                    string modName = match.Groups["name"].Value;

                    var dependency = infoFile.Dependencies.FirstOrDefault(item => item.ModName == modName);
                    if (dependency == null) return string.Empty;

                    if (dependency.IsPresent(parentCollection, infoFile.FactorioVersion, out Mod mod))
                    {
                        return mod.File.LoadSettingsFile(file);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return callback(file);
                }
            }

            public override bool ScriptFileExists(string name)
            {
                return true;
            }
        }
    }
}
