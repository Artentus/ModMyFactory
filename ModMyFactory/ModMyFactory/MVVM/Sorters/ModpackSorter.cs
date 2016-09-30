using ModMyFactory.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModpackSorter : IComparer<Modpack>, IComparer
    {
        public int Compare(Modpack x, Modpack y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int Compare(object x, object y)
        {
            if (!(x is Modpack) || !(y is Modpack))
                throw new ArgumentException("Parameters need to be of type Modpack.");

            return Compare((Modpack)x, (Modpack)y);
        }
    }
}
