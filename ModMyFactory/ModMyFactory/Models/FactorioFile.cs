using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.Models
{
    sealed class FactorioFile
    {
        public FileInfo ArchiveFile { get; }

        public Version Version { get; }

        public bool Is64Bit { get; }
    }
}
