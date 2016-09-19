using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory
{
    sealed class FactorioVersionSorter : IComparer<FactorioVersion>, IComparer
    {
        public int Compare(FactorioVersion x, FactorioVersion y)
        {
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
