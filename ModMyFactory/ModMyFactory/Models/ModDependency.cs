using System;
using System.Collections.Generic;
using System.Text;

namespace ModMyFactory.Models
{
    sealed class ModDependency
    {
        /// <summary>
        /// Indicates whether this dependency is optional.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// The name of the mod specified by this dependency.
        /// </summary>
        public string ModName { get; }

        /// <summary>
        /// Indicates whether this dependency enforces a specific mod version.
        /// </summary>
        public bool HasVersionRestriction { get; }

        /// <summary>
        /// The lowest allowed version of the mod.
        /// Only valid if <see cref="HasVersionRestriction"/> is true.
        /// </summary>
        public Version ModVersion { get; }

        /// <summary>
        /// Indicates whether this dependency is referencing the base game.
        /// </summary>
        public bool IsBase => ModName == "base";

        /// <summary>
        /// A human-readable description of this dependency.
        /// </summary>
        public string FriendlyDescription
        {
            get
            {
                var result = new StringBuilder();
                result.Append(IsBase ? "Factorio" : ModName);

                if (HasVersionRestriction)
                {
                    result.Append(" >= ");
                    result.Append(ModVersion.ToString());
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Checks if a collection of mods satisfies this dependency.
        /// </summary>
        public bool IsMet(ModCollection mods, ICollection<FactorioVersion> factorioVersions)
        {
            if (IsBase)
            {
                if (!HasVersionRestriction) return true;

                foreach (var factorio in factorioVersions)
                {
                    if ((factorio.Version.Major == ModVersion.Major) && (factorio.Version.Minor == ModVersion.Minor) && (factorio.Version >= ModVersion)) return true;
                }
            }
            else
            {
                foreach (Mod mod in mods)
                {
                    if ((mod.Name == ModName) &&
                        (!HasVersionRestriction || (mod.Version >= ModVersion)))
                        return true;
                }
            }

            return false;
        }

        private void ThrowIfInvalid(string[] parts, int index)
        {
            if (parts.GetUpperBound(0) < index)
                throw new ArgumentException("Invalid dependency string.");
        }
        
        public ModDependency(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

            string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 4) throw new ArgumentException("Invalid dependency string.");

            int index = 0;

            if (parts[index] == "?")
            {
                IsOptional = true;
                index++;
            }

            ThrowIfInvalid(parts, index);
            ModName = parts[index];
            index++;

            if (parts.GetUpperBound(0) >= index)
            {
                if (parts[index] == ">=")
                {
                    index++;

                    ThrowIfInvalid(parts, index);
                    if (!Version.TryParse(parts[index], out Version version))
                        throw new ArgumentException("Invalid dependency string.");
                    ModVersion = version;
                }
                else
                {
                    throw new ArgumentException("Invalid dependency string.");
                }
            }
        }
    }
}
