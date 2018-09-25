using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory
{
    sealed class VersionComparer : IComparer<Version>, IComparer
    {
        public int Compare(Version x, Version y)
        {
            if ((x == null) && (y == null))
            {
                return 0;
            }
            else if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }
            else
            {
                return x.CompareTo(y);
            }
        }

        public int Compare(object x, object y)
        {
            Version vX = x as Version;
            Version vY = y as Version;

            if ((x != null) && (vX == null))
                throw new ArgumentException("Parameters need to be of type Version.");
            if ((y != null) && (vY == null))
                throw new ArgumentException("Parameters need to be of type Version.");

            return Compare(vX, vY);
        }
    }
}
