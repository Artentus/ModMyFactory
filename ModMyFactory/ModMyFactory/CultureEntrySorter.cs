using System;
using System.Collections;
using System.Collections.Generic;
using ModMyFactory.Lang;

namespace ModMyFactory
{
    sealed class CultureEntrySorter : IComparer<CultureEntry>, IComparer
    {
        public int Compare(CultureEntry x, CultureEntry y)
        {
            return string.Compare(x.EnglishName, y.EnglishName, StringComparison.InvariantCultureIgnoreCase);
        }

        public int Compare(object x, object y)
        {
            if (!(x is CultureEntry) || !(y is CultureEntry))
                throw new ArgumentException("Parameters need to be of type CultureEntry.");

            return Compare((CultureEntry)x, (CultureEntry)y);
        }
    }
}
