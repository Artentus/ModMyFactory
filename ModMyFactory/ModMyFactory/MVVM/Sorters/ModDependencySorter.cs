using ModMyFactory.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModDependencySorter : IComparer<ModDependencyInfo>, IComparer
    {
        public int Compare(ModDependencyInfo x, ModDependencyInfo y)
        {
            int result = x.IsOptional.CompareTo(y.IsOptional);

            if (result == 0)
                result = x.Name.CompareTo(y.Name);

            return result;
        }

        public int Compare(object x, object y)
        {
            if ((x is ModDependencyInfo) && (y is ModDependencyInfo))
            {
                return Compare((ModDependencyInfo)x, (ModDependencyInfo)y);
            }
            else
            {
                throw new ArgumentException("Parameters need to be of type ModDependencyInfo.");
            }
        }
    }
}
