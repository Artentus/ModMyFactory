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

        public ModUpdateInfo(string title, string name, Version currentVersion, Version newestVersion, Mod mod, ModRelease newestRelease)
        {
            Title = title;
            Name = name;
            CurrentVersion = currentVersion;
            NewestVersion = newestVersion;
            Mod = mod;
            NewestRelease = newestRelease;
            isSelected = true;
        }
    }
}
