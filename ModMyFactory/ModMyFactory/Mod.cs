using System.ComponentModel;
using System.IO;

namespace ModMyFactory
{
    /// <summary>
    /// A mod.
    /// </summary>
    class Mod : NotifyPropertyChangedBase
    {
        string name;
        FileInfo file;
        bool active;

        /// <summary>
        /// The name of the mod.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        /// <summary>
        /// The mod file.
        /// </summary>
        public FileInfo File
        {
            get { return file; }
            set
            {
                if (value != file)
                {
                    file = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(File)));
                }
            }
        }

        /// <summary>
        /// Indicates whether the mod is currently active.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (value != active)
                {
                    active = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                }
            }
        }

        /// <summary>
        /// creates a mod.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <param name="file">The mod file.</param>
        public Mod(string name, FileInfo file)
        {
            this.name = name;
            this.file = file;
        }
    }
}
