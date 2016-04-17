using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory
{
    sealed class MainViewModel : NotifyPropertyChangedBase
    {
        static MainViewModel instance;

        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());

        public BindingList<Mod> Mods { get; }

        public BindingList<Modpack> Modpacks { get; }

        private MainViewModel()
        {
            Mods = new BindingList<Mod>();
            Modpacks = new BindingList<Modpack>();

            Mods.Add(new Mod("aaa", new FileInfo("a")));
        }
    }
}
