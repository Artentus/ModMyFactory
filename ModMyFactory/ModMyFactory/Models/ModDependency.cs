using ModMyFactory.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WPFCore;

namespace ModMyFactory.Models
{
    sealed class ModDependency : NotifyPropertyChangedBase
    {
        readonly string stringRepresentation;

        /// <summary>
        /// Indicates whether this dependency is optional.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// Indicates whether this dependency is inverted.
        /// </summary>
        public bool IsInverted { get; }

        /// <summary>
        /// The name of the mod specified by this dependency.
        /// </summary>
        public string ModName { get; }

        /// <summary>
        /// Indicates whether this dependency enforces a specific mod version.
        /// </summary>
        public bool HasVersionRestriction { get; }

        /// <summary>
        /// Indicates whether this dependencies restriction is exact.
        /// Only valid if <see cref="HasVersionRestriction"/> is true.
        /// </summary>
        public bool ExactRestriction { get; }

        /// <summary>
        /// The lowest allowed version of the mod.
        /// Only valid if <see cref="HasVersionRestriction"/> is true.
        /// </summary>
        public GameCompatibleVersion ModVersion { get; }

        /// <summary>
        /// Indicates whether this dependency is referencing the base game.
        /// </summary>
        public bool IsBase => ModName == "base";

        /// <summary>
        /// A human-readable description of this dependency.
        /// </summary>
        public string FriendlyDescription { get; }

        /// <summary>
        /// Indicates whether this dependency is unsatisfied.
        /// </summary>
        public bool Unsatisfied { get; private set; }
        
        /// <summary>
        /// Checks if a collection of mods satisfies this dependency.
        /// </summary>
        public bool IsMet(ModCollection mods, Version factorioVersion)
        {
            bool result;

            if (IsBase || IsInverted)
            {
                result = true;
            }
            else
            {
                if (HasVersionRestriction)
                {
                    if (ExactRestriction)
                        result = mods.Contains(ModName, ModVersion);
                    else
                    {
                        var candidates = mods.Find(ModName, factorioVersion);
                        result = candidates.Any(item => item.Version >= ModVersion);
                    }
                }
                else
                {
                    result = mods.ContainsbyFactorioVersion(ModName, factorioVersion);
                }
            }

            Unsatisfied = !result;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Unsatisfied)));

            return result;
        }

        private IEnumerable<Mod> GetTargetMods(ModCollection mods, Version factorioVersion)
        {
            if (HasVersionRestriction)
            {
                if (ExactRestriction)
                {
                    if (mods.TryGetMod(ModName, ModVersion, out Mod mod))
                        return mod.EnumerateSingle();
                    else
                        return Enumerable.Empty<Mod>();
                }
                else
                {
                    return mods.Find(ModName, factorioVersion).Where(item => item.Version >= ModVersion);
                }
            }
            else
            {
                return mods.Find(ModName, factorioVersion);
            }
        }

        /// <summary>
        /// Checks if a collection of mods satisfies this dependency and the dependency is active.
        /// </summary>
        public bool IsActive(ModCollection mods, Version factorioVersion)
        {
            if (IsBase)
            {
                return true;
            }
            else if (IsInverted)
            {
                var candidates = GetTargetMods(mods, factorioVersion);
                return candidates.All(item => !item.Active);
            }
            else
            {
                var candidates = GetTargetMods(mods, factorioVersion);
                return candidates.Any(item => item.Active);
            }
        }

        public ModDependency(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            value = value.Trim();

            if (value[0] == '?')
            {
                IsOptional = true;
                value = value.Substring(1).TrimStart();
            }
            else if (value[0] == '!')
            {
                IsInverted = true;
                value = value.Substring(1).TrimStart();
            }

            string[] parts = value.Split(new[] { ">=" }, StringSplitOptions.None);
            if (parts.Length == 1)
            {
                ExactRestriction = true;
                parts = value.Split(new[] { '=' }, StringSplitOptions.None);
            }
            if ((parts.Length == 0) || (parts.Length > 2)) throw new ArgumentException("Invalid dependency string.");

            string name = parts[0].TrimEnd();
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid dependency string.");
            ModName = name;

            if (parts.Length == 2)
            {
                string versionString = parts[1].TrimStart();
                if (!Version.TryParse(versionString, out var version)) throw new ArgumentException("Invalid dependency string.");
                ModVersion = version;
                HasVersionRestriction = true;
            }


            // Friendly Description
            var result = new StringBuilder();
            result.Append(IsBase ? "Factorio" : ModName);

            if (HasVersionRestriction)
            {
                result.Append(ExactRestriction ? " = " : " >= ");
                result.Append(ModVersion.ToString());
            }

            FriendlyDescription = result.ToString();


            // String Representation
            result.Clear();

            if (IsOptional) result.Append("? ");
            else if (IsInverted) result.Append('!');

            result.Append(ModName);
            if (HasVersionRestriction)
            {
                result.Append(ExactRestriction ? " = " : " >= ");
                result.Append(ModVersion.ToString());
            }

             stringRepresentation = result.ToString();
        }

        /// <summary>
        /// Activates this dependency if possible.
        /// </summary>
        public void Activate(ModCollection mods, Version factorioVersion)
        {
            if (IsBase)
            {
                return;
            }
            else if (IsInverted)
            {
                var candidates = GetTargetMods(mods, factorioVersion);
                foreach (var item in candidates) item.Active = false;
            }
            else
            {
                var candidates = GetTargetMods(mods, factorioVersion);
                if (!candidates.Any(item => item.Active))
                {
                    Mod max = candidates.MaxBy(item => item.Version, new VersionComparer());
                    if (max != null) max.Active = true;
                }
            }
        }

        public override string ToString() => stringRepresentation;

        public static implicit operator ModDependency(string value)
        {
            return new ModDependency(value);
        }

        public static implicit operator string(ModDependency value)
        {
            return value.ToString();
        }
    }
}
