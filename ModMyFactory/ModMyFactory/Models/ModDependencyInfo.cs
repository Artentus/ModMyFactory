using ModMyFactory.Web.ModApi;
using System;
using System.ComponentModel;
using WPFCore;

namespace ModMyFactory.Models
{
    sealed class ModDependencyInfo : NotifyPropertyChangedBase
    {
        bool isOptional;
        bool isSelected;

        public ModRelease Release { get; }

        public string Name { get; }

        public Version FactorioVersion { get; }

        public GameCompatibleVersion Version => Release.Version;

        public bool IsOptional
        {
            get => isOptional;
            set
            {
                if (value != isOptional)
                {
                    isOptional = value;
                    IsSelected = !isOptional;
                }
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public ModDependencyInfo(ModRelease release, string name, Version factorioVersion, bool isOptional)
        {
            Release = release;
            Name = name;
            FactorioVersion = factorioVersion;
            IsOptional = isOptional;
            IsSelected = !isOptional;
        }
    }
}
