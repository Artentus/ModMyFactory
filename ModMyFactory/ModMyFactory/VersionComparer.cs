using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory
{
    sealed class VersionComparer : IComparer<Version>, IComparer<GameCompatibleVersion>, IComparer
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

        public int Compare(GameCompatibleVersion x, GameCompatibleVersion y)
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
            if ((x is Version) && (y is Version))
            {
                return Compare((Version)x, (Version)y);
            }
            else if ((x is GameCompatibleVersion) && (y is GameCompatibleVersion))
            {
                return Compare((GameCompatibleVersion)x, (GameCompatibleVersion)y);
            }
            else
            {
                throw new ArgumentException("Parameters need to be of type Version.");
            }
        }
    }
}
