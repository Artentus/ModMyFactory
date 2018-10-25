using ModMyFactory.Models.ModSettings;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModSettingSorter : IComparer<IModSetting>, IComparer
    {
        public int Compare(IModSetting x, IModSetting y)
        {
            int result = string.Compare(x.Ordering, y.Ordering, StringComparison.InvariantCulture);
            if (result == 0) result = x.Name.CompareTo(y.Name);
            return result;
        }

        public int Compare(object x, object y)
        {
            if ((x is IModSetting) && (y is IModSetting))
            {
                return Compare((IModSetting)x, (IModSetting)y);
            }
            else
            {
                throw new ArgumentException("Parameters need to be of type ModSetting.");
            }
        }
    }
}
