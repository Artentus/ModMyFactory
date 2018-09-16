using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.Models
{
    partial class Mod
    {
        private sealed class ModFileCollection : ICollection<ModFile>
        {
            private readonly List<ModFile> list;

            public ModFile Latest { get; private set; }

            public int Count => list.Count;

            public bool IsReadOnly => false;

            public ModFileCollection()
            {
                list = new List<ModFile>();
            }

            public void Add(ModFile item)
            {
                list.Add(item);

                if ((Latest == null) || (Latest.Version < item.Version))
                    Latest = item;
            }

            public bool Remove(ModFile item)
            {
                if (item == Latest)
                    Latest = null;

                return list.Remove(item);
            }

            public void Clear()
            {
                Latest = null;
                list.Clear();
            }

            public bool Contains(ModFile item)
            {
                return list.Contains(item);
            }

            public void CopyTo(ModFile[] array, int arrayIndex)
            {
                list.CopyTo(array, arrayIndex);
            }

            public IEnumerator<ModFile> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return list.GetEnumerator();
            }
        }
    }
}
