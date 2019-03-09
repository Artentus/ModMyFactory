using System;
using System.Collections;
using System.Collections.Generic;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.MVVM.Sorters
{
    sealed class ModInfoSorter : IComparer<ModInfo>, IComparer
    {
        public ModInfoSorterMode Mode { get; set; }

        private int CompareAlphabetical(ModInfo x, ModInfo y)
        {
            return string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase);
        }

        private int CompareDownloadCount(ModInfo x, ModInfo y)
        {
            return y.DownloadCount - x.DownloadCount;
        }

        private int CompareScore(ModInfo x, ModInfo y)
        {
            double diff = y.Score - x.Score;
            return Math.Sign(diff);
        }

        private int CompareLastUpdate(ModInfo x, ModInfo y)
        {
            return DateTime.Compare(y.LatestRelease.ReleaseDate, x.LatestRelease.ReleaseDate);
        }
        
        public int Compare(ModInfo x, ModInfo y)
        {
            int result = 0;

            switch (Mode)
            {
                case ModInfoSorterMode.Alphabetical:
                    result = 0; // No special logic
                    break;
                case ModInfoSorterMode.DownloadCount:
                    result = CompareDownloadCount(x, y);
                    break;
                case ModInfoSorterMode.Score:
                    result = CompareScore(x, y);
                    break;
                case ModInfoSorterMode.LastUpdate:
                    result = CompareLastUpdate(x, y);
                    break;
            }

            if (result == 0) return CompareAlphabetical(x, y);
            else return result;
        }

        public int Compare(object x, object y)
        {
            if (!(x is ModInfo) || !(y is ModInfo))
                throw new ArgumentException("Parameters need to be of type ModInfo.");

            return Compare((ModInfo)x, (ModInfo)y);
        }
    }
}
