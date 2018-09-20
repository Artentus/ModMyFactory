using ModMyFactory.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModSorter : IComparer<Mod>, IComparer<ModTemplate>, IComparer
    {
        public int Compare(Mod x, Mod y)
        {
            int versionOrder = y.FactorioVersion.CompareTo(x.FactorioVersion);
            if (versionOrder != 0) return versionOrder;

            return string.Compare(x.FriendlyName, y.FriendlyName, StringComparison.InvariantCultureIgnoreCase);
        }

        public int Compare(ModTemplate x, ModTemplate y)
        {
            return Compare(x.Mod, y.Mod);
        }

        public int Compare(object x, object y)
        {
            if ((x is Mod) && (y is Mod))
            {
                return Compare((Mod)x, (Mod)y);
            }
            else if ((x is ModTemplate) && (y is ModTemplate))
            {
                return Compare((ModTemplate)x, (ModTemplate)y);
            }
            else
            {
                throw new ArgumentException("Parameters need to be of type Mod.");
            }
        }
    }
}
