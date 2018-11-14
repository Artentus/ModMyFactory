using ModMyFactory.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModUpdateInfoSorter : IComparer<ModUpdateInfo>, IComparer
    {
        public int Compare(ModUpdateInfo x, ModUpdateInfo y)
        {
            int result = y.FactorioVersion.CompareTo(x.FactorioVersion);

            if (result == 0)
                result = string.Compare(x.FriendlyName, y.FriendlyName, StringComparison.InvariantCultureIgnoreCase);

            return result;
        }

        public int Compare(object x, object y)
        {
            if (!(x is ModUpdateInfo) || !(y is ModUpdateInfo))
                throw new ArgumentException("Parameters need to be of type ModUpdateInfo.");

            return Compare((ModUpdateInfo)x, (ModUpdateInfo)y);
        }
    }
}
