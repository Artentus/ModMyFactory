using System;
using System.ComponentModel;
using System.Text;
using WPFCore;

namespace ModMyFactory.Models
{
    sealed class ModDependency : NotifyPropertyChangedBase
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
        /// Indicates whether this dependency is unsatisfied.
        /// </summary>
        public bool Unsatisfied { get; private set; }

        /// <summary>
        /// Checks if a collection of mods satisfies this dependency.
        /// </summary>
        public bool IsMet(ModCollection mods, Version factorioVersion)
        {
            bool result;

            if (IsBase)
            {
                result = true;
            }
            else
            {
                var mod = mods.FindByFactorioVersion(ModName, factorioVersion);
                if (mod == null)
                {
                    result = false;
                }
                else
                {
                    if (HasVersionRestriction)
                        result = mod.Version >= ModVersion;
                    else
                        result = true;
                }
            }

            Unsatisfied = !result;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Unsatisfied)));

            return result;
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
            else
            {
                var mod = mods.FindByFactorioVersion(ModName, factorioVersion);
                if (mod == null) return false;

                if (HasVersionRestriction)
                    return (mod.Version >= ModVersion) && mod.Active;
                else
                    return mod.Active;
            }
        }

        public ModDependency(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            value = value.Trim();

            if (value.StartsWith("?"))
            {
                IsOptional = true;
                value = value.Substring(1).TrimStart();
            }

            string[] parts = value.Split(new[] { ">=" }, StringSplitOptions.None);
            if (parts.Length == 1) parts = value.Split(new[] { '=' }, StringSplitOptions.None);
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
        }

        /// <summary>
        /// Activates this dependency if possible.
        /// </summary>
        public void Activate(ModCollection mods, Version factorioVersion)
        {
            if (IsBase) return;

            var mod = mods.FindByFactorioVersion(ModName, factorioVersion);
            if (mod == null) return;

            if (!HasVersionRestriction || (mod.Version >= ModVersion))
                mod.Active = true;
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            if (IsOptional) result.Append("? ");
            result.Append(ModName);
            if (HasVersionRestriction)
            {
                result.Append(" >= ");
                result.Append(ModVersion.ToString());
            }

            return result.ToString();
        }

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
