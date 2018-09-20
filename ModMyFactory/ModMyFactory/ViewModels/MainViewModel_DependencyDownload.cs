using ModMyFactory.Models;
using ModMyFactory.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModMyFactory.ViewModels
{
    partial class MainViewModel
    {
        private Dictionary<string, ModDependencyInfo> GetSubDict(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, Version factorioVersion)
        {
            Dictionary<string, ModDependencyInfo> subDict;
            if (!dict.TryGetValue(factorioVersion, out subDict))
            {
                subDict = new Dictionary<string, ModDependencyInfo>();
                dict.Add(factorioVersion, subDict);
            }
            return subDict;
        }

        private void AddDependency(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, ModDependency dependency, Version factorioVersion)
        {
            var subDict = GetSubDict(dict, factorioVersion);

            ModDependencyInfo info;
            if (!subDict.TryGetValue(dependency.ModName, out info))
            {
                info = new ModDependencyInfo(dependency.ModName, factorioVersion, dependency.ModVersion, dependency.IsOptional);
                subDict.Add(dependency.ModName, info);
            }
            else
            {
                if (dependency.ModVersion > info.Version)
                    info.Version = dependency.ModVersion;

                if (!dependency.IsOptional)
                    info.IsOptional = false;
            }
        }

        private void AddDependencies(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, Mod mod)
        {
            foreach (var dependency in mod.Dependencies)
            {
                if (!dependency.IsBase && !dependency.IsMet(Mods))
                    AddDependency(dict, dependency, mod.FactorioVersion);
            }
        }

        private List<ModDependencyInfo> GetDependencies()
        {
            ICollection<Mod> selectedMods = new List<Mod>(Mods.Where(mod => mod.IsSelected));
            if (selectedMods.Count == 0) selectedMods = Mods;

            var dict = new Dictionary<Version, Dictionary<string, ModDependencyInfo>>();
            foreach (var mod in selectedMods)
                AddDependencies(dict, mod);

            return new List<ModDependencyInfo>(dict.SelectMany(kvp => kvp.Value.Values));
        }

        private async Task DownloadDependencies()
        {
            var dependencies = GetDependencies();

            var window = new DependencyDownloadWindow();
            window.Owner = Window;
            var viewModel = (DependencyDownloadViewModel)window.ViewModel;
            viewModel.Dependencies = dependencies;
            if (window.ShowDialog() == true)
            {

            }
        }
    }
}
