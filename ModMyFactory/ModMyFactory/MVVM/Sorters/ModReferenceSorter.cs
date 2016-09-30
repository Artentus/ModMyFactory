using System;
using System.Collections;
using System.Collections.Generic;
using ModMyFactory.Models;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModReferenceSorter : IComparer<IModReference>, IComparer
    {
        public int Compare(IModReference x, IModReference y)
        {
            var modReferenceX = x as ModReference;
            var modpackReferenceX = x as ModpackReference;
            var modReferenceY = y as ModReference;
            var modpackReferenceY = y as ModpackReference;

            if (modReferenceX != null && modReferenceY != null)
            {
                return string.Compare(x.DisplayName, y.DisplayName,
                    StringComparison.InvariantCultureIgnoreCase);
            }
            else if (modpackReferenceX != null && modpackReferenceY != null)
            {
                return string.Compare(x.DisplayName, y.DisplayName,
                    StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return modpackReferenceX != null ? -1 : 1;
            }
        }

        public int Compare(object x, object y)
        {
            if (!(x is IModReference) || !(y is IModReference))
                throw new ArgumentException("Parameters need to be of type IModReference.");

            return Compare((IModReference)x, (IModReference)y);
        }
    }
}
