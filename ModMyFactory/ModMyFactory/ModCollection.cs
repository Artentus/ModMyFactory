using System;
using System.Collections.ObjectModel;
using System.Linq;
using ModMyFactory.Models;

namespace ModMyFactory
{
    class ModCollection : ObservableCollection<Mod>
    {
        /// <summary>
        /// Checks if the collection contains a mod.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <returns>Returns true if the collection contains the mod, otherwise false.</returns>
        public bool Contains(string name)
        {
            return this.Any(mod => string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Checks if the collection contains a mod.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <param name="version">The mods version.</param>
        /// <returns>Returns true if the collection contains the mod, otherwise false.</returns>
        public bool Contains(string name, Version version)
        {
            return this.Any(mod =>
                string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase)
                && (mod.Version == version));
        }

        /// <summary>
        /// Checks if the collection contains a mod.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <param name="factorioVersion">The mods Factorio version.</param>
        /// <returns>Returns true if the collection contains the mod, otherwise false.</returns>
        public bool ContainsByFactorioVersion(string name, Version factorioVersion)
        {
            if (App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion)
            {
                return this.Any(mod =>
                    string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase)
                    && (mod.FactorioVersion == factorioVersion));
            }
            else
            {
                return this.Any(mod =>
                    string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Finds mods in this collection.
        /// </summary>
        /// <param name="name">The name of the mods.</param>
        /// <returns>Returns the mods searched for.</returns>
        public Mod[] Find(string name)
        {
            return this.Where(mod => string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        }

        /// <summary>
        /// Finds a mod in this collection.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <param name="version">The mods version.</param>
        /// <returns>Returns the mod searched for.</returns>
        public Mod Find(string name, Version version)
        {
            return this.FirstOrDefault(mod =>
                string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase)
                && (mod.Version == version));
        }

        /// <summary>
        /// Finds a mod in this collection.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <param name="factorioVersion">The mods Factorio version.</param>
        /// <returns>Returns the mod searched for.</returns>
        public Mod FindByFactorioVersion(string name, Version factorioVersion)
        {
            if (App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion)
            {
                return this.FirstOrDefault(mod =>
                    string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase)
                    && (mod.FactorioVersion == factorioVersion));
            }
            else
            {
                return this.FirstOrDefault(mod =>
                    string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
