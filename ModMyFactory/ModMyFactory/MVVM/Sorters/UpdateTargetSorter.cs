using System;
using System.Collections;
using System.Collections.Generic;
using ModMyFactory.Models;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class UpdateTargetSorter : IComparer<UpdateTarget>, IComparer
    {
        public int Compare(UpdateTarget x, UpdateTarget y)
        {
            return y.TargetVersion.CompareTo(x.TargetVersion);
        }

        public int Compare(object x, object y)
        {
            if (!(x is UpdateTarget) || !(y is UpdateTarget))
                throw new ArgumentException("Parameters need to be of type UpdateTarget.");

            return Compare((UpdateTarget)x, (UpdateTarget)y);
        }
    }
}
