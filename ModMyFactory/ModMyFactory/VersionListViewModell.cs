using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ModMyFactory
{
    sealed class VersionListViewModell : NotifyPropertyChangedBase
    {
        FactorioOnlineVersion selectedVersion;

        public List<FactorioOnlineVersion> FactorioVersions { get; }

        public FactorioOnlineVersion SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                if (value != selectedVersion)
                {
                    selectedVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedVersion)));
                }
            }
        }

        public VersionListViewModell(List<FactorioOnlineVersion> factorioVersions)
        {
            FactorioVersions = factorioVersions;

            //FactorioVersions.Add(new FactorioOnlineVersion(new Version(0, 13, 20), "(alpha)"));
            //FactorioVersions.Add(new FactorioOnlineVersion(new Version(0, 12, 35), "(alpha)"));
            //FactorioVersions.Add(new FactorioOnlineVersion(new Version(0, 11, 22), "(alpha)"));
            //FactorioVersions.Add(new FactorioOnlineVersion(new Version(0, 10, 12), "(alpha)"));
            //FactorioVersions.Add(new FactorioOnlineVersion(new Version(0, 9, 8), "(alpha)"));
            //FactorioVersions.Add(new FactorioOnlineVersion(new Version(0, 8, 8), "(alpha)"));
            //FactorioVersions.Add(new FactorioOnlineVersion(new Version(0, 7, 5), "(alpha)"));
            //FactorioVersions.Add(new FactorioOnlineVersion(new Version(0, 6, 4), "(alpha)"));
        }
    }
}
