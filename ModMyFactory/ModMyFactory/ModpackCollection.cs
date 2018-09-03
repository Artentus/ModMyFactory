using System.Collections.ObjectModel;
using ModMyFactory.Models;

namespace ModMyFactory
{
    class ModpackCollection : ObservableCollection<Modpack>
    {
        public void ExchangeMods(Mod oldMod, Mod newMod)
        {
            foreach (var modpack in this)
            {
                ModReference reference;
                if (modpack.Contains(oldMod, out reference))
                {
                    modpack.Mods.Remove(reference);
                    modpack.Mods.Add(new ModReference(newMod, modpack));
                }
            }
        }

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
