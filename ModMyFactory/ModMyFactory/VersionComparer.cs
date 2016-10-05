using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory
{
    sealed class VersionComparer : IComparer<Version>, IComparer
    {
        public int Compare(Version x, Version y)
        {
            return x.CompareTo(y);
        }

        public int Compare(object x, object y)
        {
            if (!(x is Version) || !(y is Version))
                throw new ArgumentException("Parameters need to be of type Version.");

            return Compare((Version)x, (Version)y);
        }
    }
}
