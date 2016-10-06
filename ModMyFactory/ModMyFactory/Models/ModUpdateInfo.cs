using System;
using System.ComponentModel;
using ModMyFactory.MVVM;

namespace ModMyFactory.Models
{
    class ModUpdateInfo : NotifyPropertyChangedBase
    {
        bool isSelected;

        public string Name { get; }

        public Version CurrentVersion { get; }

        public Version NewestVersion { get; }

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

        public ModUpdateInfo(string name, Version currentVersion, Version newestVersion)
        {
            Name = name;
            CurrentVersion = currentVersion;
            NewestVersion = newestVersion;
            isSelected = true;
        }
    }
}
