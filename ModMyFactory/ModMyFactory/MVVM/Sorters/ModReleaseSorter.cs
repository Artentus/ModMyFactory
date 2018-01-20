using System;
using System.Collections;
using System.Collections.Generic;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModReleaseSorter : IComparer<ModRelease>, IComparer
    {
        public int Compare(ModRelease x, ModRelease y)
        {
            return y.Version.CompareTo(x.Version);
        }

        public int Compare(object x, object y)
        {
            if (!(x is ModRelease) || !(y is ModRelease))
                throw new ArgumentException("Parameters need to be of type ModRelease.");

            return Compare((ModRelease)x, (ModRelease)y);
        }
    }
}
