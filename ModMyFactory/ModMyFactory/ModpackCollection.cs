using System.Collections.ObjectModel;
using ModMyFactory.Models;

namespace ModMyFactory
{
    class ModpackCollection : ObservableCollection<Modpack>
    {
        public bool Contains(string name)
        {
            foreach (var modpack in this)
            {
                if (modpack.Name == name)
                    return true;
            }
            return false;
        }
    }
}
