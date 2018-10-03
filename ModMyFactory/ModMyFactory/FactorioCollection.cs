using ModMyFactory.Helpers;
using ModMyFactory.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModMyFactory
{
    sealed class FactorioCollection : ObservableCollection<FactorioVersion>
    {
        public static FactorioCollection Load()
        {
            var installedVersions = FactorioVersion.LoadInstalledVersions();
            if (App.Instance.Settings.LoadSteamVersion)
            {
                if (FactorioSteamVersion.TryLoad(out var steamVersion))
                {
                    installedVersions.Add(steamVersion);
                }
                else
                {
                    App.Instance.Settings.LoadSteamVersion = false;
                    App.Instance.Settings.Save();
                }
            }

            return new FactorioCollection(installedVersions);
        }

        public FactorioCollection()
            : base()
        {
            this.Add(new LatestFactorioVersion(this));
        }

        public FactorioCollection(IEnumerable<FactorioVersion> collection)
            : base(collection)
        {
            this.Add(new LatestFactorioVersion(this));
        }

        public FactorioCollection(List<FactorioVersion> list)
            : base(list)
        {
            this.Add(new LatestFactorioVersion(this));
        }

        public bool Contains(string name)
        {
            return this.Any(item => item.Name == name);
        }

        public FactorioVersion Find(string name)
        {
            return this.FirstOrDefault(item => item.Name == name);
        }

        public FactorioVersion Find(Version version, bool exact = true)
        {
            if (exact)
            {
                return this.FirstOrDefault(item => item.Version == version);
            }
            else
            {
                return this.Where(item => (item.Version.Major == version.Major) && (item.Version.Minor == version.Minor))
                           .MaxBy(item => item.Version, new VersionComparer());
            }
        }
    }
}
