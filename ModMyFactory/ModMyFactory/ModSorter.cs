using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory
{
    sealed class ModSorter : IComparer<Mod>, IComparer
    {
        public int Compare(Mod x, Mod y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int Compare(object x, object y)
        {
            if (!(x is Mod) || !(y is Mod))
                throw new ArgumentException("Parameters need to be of type Mod.");

            return Compare((Mod)x, (Mod)y);
        }
    }
}
