using System;
using System.ComponentModel;
using ModMyFactory.Web.ModApi;
using WPFCore;

namespace ModMyFactory.Models
{
    class ModUpdateInfo : NotifyPropertyChangedBase
    {
        bool isSelected;

        public string Title { get; }

        public string Name { get; }

        public Version CurrentVersion { get; }

        public Version NewestVersion { get; }

        public Mod Mod { get; }

        public ModRelease NewestRelease { get; }

        public Version CurrentFactorioVersion { get; }

        public Version NewestFactorioVersion { get; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public ModUpdateInfo(Mod mod, ModRelease newestRelease)
        {
            Title = mod.Title;
            Name = mod.Name;
            CurrentVersion = mod.Version;
            NewestVersion = newestRelease.Version;
            Mod = mod;
            NewestRelease = newestRelease;
            CurrentFactorioVersion = mod.FactorioVersion;
            NewestFactorioVersion = newestRelease.InfoFile.FactorioVersion;
            isSelected = true;
        }
    }
}
