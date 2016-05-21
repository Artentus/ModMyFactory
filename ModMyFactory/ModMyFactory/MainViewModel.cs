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
            Mod mod3 = new Mod("ccc", new FileInfo("c"));
            Mod mod4 = new Mod("ddd", new FileInfo("d"));
            Mod mod5 = new Mod("eee", new FileInfo("e"));
            Mods.Add(mod1);
            Mods.Add(mod2);
            Mods.Add(mod3);
            Mods.Add(mod4);
            Mods.Add(mod5);
            Modpack modpack1 = new Modpack("aaa");
            Modpack modpack2 = new Modpack("bbb");
            Modpack modpack3 = new Modpack("ccc");
            //modpack.Mods.Add(mod1);
            //modpack.Mods.Add(mod2);
            Modpacks.Add(modpack1);
            Modpacks.Add(modpack2);
            Modpacks.Add(modpack3);
        }
    }
}
