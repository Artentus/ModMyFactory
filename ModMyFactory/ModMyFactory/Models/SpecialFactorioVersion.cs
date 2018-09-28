using System;
using System.ComponentModel;
using System.IO;

namespace ModMyFactory.Models
{
    abstract class SpecialFactorioVersion : FactorioVersion
    {
        FactorioVersion wrappedVersion;

        public override string DisplayName => Name;

        protected FactorioVersion WrappedVersion
        {
            get => wrappedVersion;
            set
            {
                if (value != wrappedVersion)
                {
                    wrappedVersion = value;

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Version)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Directory)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Executable)));
                }
            }
        }

        public override Version Version => WrappedVersion?.Version;

        public override DirectoryInfo Directory => WrappedVersion?.Directory;

        public override FileInfo Executable => WrappedVersion?.Executable;

        protected override abstract string LoadName();

        protected SpecialFactorioVersion(FactorioVersion wrappedVersion)
        {
            WrappedVersion = wrappedVersion;
        }
    }
}
