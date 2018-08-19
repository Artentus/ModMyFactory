using ModMyFactory.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModpackSorter : IComparer<Modpack>, IComparer<ModpackTemplate>, IComparer<InnerModpackTemplate>, IComparer
    {
        public int Compare(Modpack x, Modpack y)
        {
            return string.Compare(x?.Name, y?.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int Compare(ModpackTemplate x, ModpackTemplate y)
        {
            return string.Compare(x?.Name, y?.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int Compare(InnerModpackTemplate x, InnerModpackTemplate y)
        {
            return string.Compare(x?.Name, y?.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int Compare(object x, object y)
        {
            if ((x is Modpack) && (y is Modpack))
            {
                return Compare((Modpack)x, (Modpack)y);
            }
            else if ((x is ModpackTemplate) && (y is ModpackTemplate))
            {
                return Compare((ModpackTemplate)x, (ModpackTemplate)y);
            }
            else if ((x is InnerModpackTemplate) && (y is InnerModpackTemplate))
            {
                return Compare((InnerModpackTemplate)x, (InnerModpackTemplate)y);
            }
            else
            {
                throw new ArgumentException("Parameters need to be of type Modpack.");
            }
        }
    }
}
