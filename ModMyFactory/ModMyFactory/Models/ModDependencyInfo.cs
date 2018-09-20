using System;
using System.ComponentModel;
using WPFCore;

namespace ModMyFactory.Models
{
    sealed class ModDependencyInfo : NotifyPropertyChangedBase
    {
        bool isOptional;
        bool isSelected;

        public string Name { get; }

        public Version FactorioVersion { get; }

        public Version Version { get; set; }

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

        public ModDependencyInfo(string name, Version factorioVersion, Version version, bool isOptional)
        {
            Name = name;
            FactorioVersion = factorioVersion;
            Version = version;
            IsOptional = isOptional;
        }
    }
}
