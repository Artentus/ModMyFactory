using ModMyFactory.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class FactorioVersionSorter : IComparer<FactorioVersion>, IComparer
    {
        public int Compare(FactorioVersion x, FactorioVersion y)
        {
            if ((x is SpecialFactorioVersion) && (y is SpecialFactorioVersion))
                return 0;
            else if (x is SpecialFactorioVersion)
                return int.MinValue;
            else if (y is SpecialFactorioVersion)
                return int.MaxValue;
            else
                return y.Version.CompareTo(x.Version);
        }

        public int Compare(object x, object y)
        {
            if (!(x is FactorioVersion) || !(y is FactorioVersion))
                throw new ArgumentException("Parameters need to be of type FactorioVersion.");

            return Compare((FactorioVersion)x, (FactorioVersion)y);
        }
    }
}
