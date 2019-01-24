using System;
using System.IO;

namespace ModMyFactory.IO
{
    sealed class JunctionInfo
    {
        public string Name { get; }

        public string FullName { get; }

        public string DestinationPath { get; private set; }

        public bool Exists => Junction.Exists(FullName);

        public JunctionInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            FullName = Path.GetFullPath(path);
            Name = Path.GetFileName(FullName.TrimEnd(Path.DirectorySeparatorChar));

            if (Exists)
            {
                DestinationPath = Junction.GetDestination(FullName);
            }
        }

        public void Create(string destinationPath)
        {
            Junction.Create(FullName, destinationPath);
            DestinationPath = destinationPath;
        }

        public void SetDestination(string destinationPath)
        {
            Junction.SetDestination(FullName, destinationPath);
            DestinationPath = Path.GetFullPath(destinationPath);
        }

        public void Delete()
        {
            Junction.Delete(FullName);
        }
    }
}
