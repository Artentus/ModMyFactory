using System;
using System.Collections;
using System.Collections.Generic;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModInfoSorter : IComparer<ModInfo>, IComparer
    {
        public int Compare(ModInfo x, ModInfo y)
        {
            return string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase);
        }

        public int Compare(object x, object y)
        {
            if (!(x is ModInfo) || !(y is ModInfo))
                throw new ArgumentException("Parameters need to be of type ModInfo.");

            return Compare((ModInfo)x, (ModInfo)y);
        }
    }
}
