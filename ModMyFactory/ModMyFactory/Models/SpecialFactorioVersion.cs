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
                    if (wrappedVersion != null)
                        wrappedVersion.PropertyChanged -= WrappedVersionPropertyChangedHandler;

                    wrappedVersion = value;

                    if (wrappedVersion != null)
                        wrappedVersion.PropertyChanged += WrappedVersionPropertyChangedHandler;

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Version)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Directory)));
                }
            }
        }

        public override Version Version => WrappedVersion?.Version;

        public override DirectoryInfo Directory => WrappedVersion?.Directory;

        protected override abstract string LoadName();

        public override void Run(string args = null)
        {
            WrappedVersion?.Run(args);
        }

        protected SpecialFactorioVersion(FactorioVersion wrappedVersion)
        {
            WrappedVersion = wrappedVersion;
        }

        private void WrappedVersionPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }
    }
}
