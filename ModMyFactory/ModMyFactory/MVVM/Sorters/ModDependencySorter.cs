using ModMyFactory.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModDependencySorter : IComparer<ModDependency>, IComparer<ModDependencyInfo>, IComparer
    {
        public int Compare(ModDependencyInfo x, ModDependencyInfo y)
        {
            int result = x.IsOptional.CompareTo(y.IsOptional);

            if (result == 0)
                result = x.Name.CompareTo(y.Name);

            return result;
        }

        public int Compare(ModDependency x, ModDependency y)
        {
            if (x.IsBase && !y.IsBase)
            {
                return -1;
            }
            else if (!x.IsBase && y.IsBase)
            {
                return 1;
            }

            int result = x.IsOptional.CompareTo(y.IsOptional);

            if (result == 0)
                result = x.ModName.CompareTo(y.ModName);

            return result;
        }

        public int Compare(object x, object y)
        {
            if ((x is ModDependencyInfo) && (y is ModDependencyInfo))
            {
                return Compare((ModDependencyInfo)x, (ModDependencyInfo)y);
            }
            else if ((x is ModDependency) && (y is ModDependency))
            {
                return Compare((ModDependency)x, (ModDependency)y);
            }
            else
            {
                throw new ArgumentException("Parameters need to be of type ModDependency.");
            }
        }
    }
}
