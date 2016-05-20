using System.Collections.ObjectModel;
using System.IO;

namespace ModMyFactory
{
    sealed class MainViewModel : NotifyPropertyChangedBase
    {
        static MainViewModel instance;

        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());

        public ObservableCollection<Mod> Mods { get; }

        public ObservableCollection<Modpack> Modpacks { get; }

        private MainViewModel()
        {
            Mods = new ObservableCollection<Mod>();
            Modpacks = new ObservableCollection<Modpack>();

            Mod mod1 = new Mod("aaa", new FileInfo("a"));
            Mod mod2 = new Mod("bbb", new FileInfo("b"));
            Mods.Add(mod1);
            Mods.Add(mod2);
            Modpack modpack = new Modpack("aaa");
            //modpack.Mods.Add(mod1);
            //modpack.Mods.Add(mod2);
            Modpacks.Add(modpack);
        }
    }
}
