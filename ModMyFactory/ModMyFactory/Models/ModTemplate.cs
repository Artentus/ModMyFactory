using System.ComponentModel;
using ModMyFactory.Export;
using WPFCore;

namespace ModMyFactory.Models
{
    sealed class ModTemplate : NotifyPropertyChangedBase
    {
        ExportMode exportMode;
        bool include;

        public ExportMode ExportMode
        {
            get { return exportMode; }
            set
            {
                if (value != exportMode)
                {
                    exportMode = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ExportMode)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UseNewestVersion)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UseSpecificVersion)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UseFactorioVersion)));
                }
            }
        }

        public bool UseNewestVersion
        {
            get { return ExportMode == ExportMode.NewestVersion; }
            set
            {
                if (value && (ExportMode != ExportMode.NewestVersion))
                    ExportMode = ExportMode.NewestVersion;
            }
        }

        public bool UseSpecificVersion
        {
            get { return ExportMode == ExportMode.SpecificVersion; }
            set
            {
                if (value && (ExportMode != ExportMode.SpecificVersion))
                    ExportMode = ExportMode.SpecificVersion;
            }
        }

        public bool UseFactorioVersion
        {
            get { return ExportMode == ExportMode.FactorioVersion; }
            set
            {
                if (value && (ExportMode != ExportMode.FactorioVersion))
                    ExportMode = ExportMode.FactorioVersion;
            }
        }

        public bool Include
        {
            get { return include; }
            set
            {
                if (value != include)
                {
                    include = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Include)));

                    if (include) UseSpecificVersion = true;
                }
            }
        }



        public Mod Mod { get; }

        public string Name => Mod.FriendlyName;

        public string VersionInfo => $"({Mod.FactorioVersion.ToString(2)})";

        public ModTemplate(Mod mod)
        {
            Mod = mod;
            exportMode = ExportMode.NewestVersion;
        }
    }
}
