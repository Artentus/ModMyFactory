using ModMyFactory.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFCore;

namespace ModMyFactory.Models
{
    sealed class ModDependency : NotifyPropertyChangedBase
    {
        private static readonly Dictionary<string, Func<GameCompatibleVersion, GameCompatibleVersion, bool>> comparisonFunctions;
        private static readonly Dictionary<string, string> comparisonRepresentations;

        static ModDependency()
        {
            comparisonFunctions = new Dictionary<string, Func<GameCompatibleVersion, GameCompatibleVersion, bool>>()
            {
                { ">=", (a, b) => a >= b },
                { ">", (a, b) => a > b },
                { "<=", (a, b) => a <= b },
                { "<", (a, b) => a < b },
                { "=", (a, b) => a == b }
            };

            comparisonRepresentations = new Dictionary<string, string>()
            {
                { ">=", "≥" },
                { ">", ">" },
                { "<=", "≤" },
                { "<", "<" },
                { "=", "=" }
            };
        }


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
        /// Indicates whether the dependency has a version restriction.
        /// </summary>
        public bool HasRestriction { get; }

        /// <summary>
        /// The comparison type of the restriction. Empty if no restriction is present.
        /// </summary>
        public string RestrictionComparison { get; }
        
        /// <summary>
        /// The restrictions version.
        /// </summary>
        public GameCompatibleVersion RestrictionVersion { get; }

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
        /// Checks if a collection of mods contains a mod that satisfies this dependency.
        /// </summary>
        public bool IsPresent(ModCollection mods, Version factorioVersion)
        {
            bool result = false;

            if (IsBase || IsInverted)
            {
                result = true;
            }
            else if (HasRestriction)
            {
                var comparison = comparisonFunctions[RestrictionComparison];

                var candidates = mods.Find(ModName, factorioVersion);
                foreach (var candidate in candidates)
                {
                    if (comparison(candidate.Version, RestrictionVersion))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                result = mods.ContainsbyFactorioVersion(ModName, factorioVersion);
            }

            Unsatisfied = !result;
            return result;
        }
        
        /// <summary>
        /// Checks if a collection of mods satisfies this dependency and the dependency is active.
        /// </summary>
        public bool IsActive(ModCollection mods, Version factorioVersion)
        {
            if (IsBase || IsInverted)
            {
                return true;
            }
            else if (HasRestriction)
            {
                var comparison = comparisonFunctions[RestrictionComparison];

                var candidates = mods.Find(ModName, factorioVersion);
                return candidates.Any(candidate => candidate.Active && comparison(candidate.Version, RestrictionVersion));
            }
            else
            {
                var candidates = mods.Find(ModName, factorioVersion);
                return candidates.Any(candidate => candidate.Active);
            }
        }

        public ModDependency(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            value = value.Trim();

            if (value[0] == '?') // Optional
            {
                IsOptional = true;
                value = value.Substring(1).TrimStart();
            }
            else if (value[0] == '!') // Inverted
            {
                IsInverted = true;
                value = value.Substring(1).TrimStart();
            }
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("No mod name specified.", nameof(value));

            HasRestriction = false;
            string[] parts = { };
            foreach (var comparison in comparisonFunctions.Keys)
            {
                parts = value.Split(new[] { comparison }, StringSplitOptions.None);
                if (parts.Length == 1)
                {
                    // Comparison not present
                    continue;
                }
                else if (parts.Length == 2)
                {
                    // Comparison present
                    HasRestriction = true;
                    RestrictionComparison = comparison;
                    break;
                }
                else
                {
                    // Comparison present more than once
                    throw new ArgumentException("Invalid input format.", nameof(value));
                }
            }

            if (HasRestriction)
            {
                ModName = parts[0].TrimEnd();

                string versionString = parts[1].TrimStart();
                if (!GameCompatibleVersion.TryParse(versionString, out var version))
                    throw new ArgumentException("Invalid input format.", nameof(value));
                RestrictionVersion = version;
            }
            else
            {
                ModName = value;
            }


            // Friendly description
            var sb = new StringBuilder();

            string name = ModName;
            if (name == "base") name = "Factorio";
            sb.Append(name);

            if (HasRestriction)
            {
                sb.Append(' ');
                sb.Append(comparisonRepresentations[RestrictionComparison]);
                sb.Append(' ');
                sb.Append(RestrictionVersion);
            }

            FriendlyDescription = sb.ToString();


            // ToString
            sb.Clear();

            if (IsOptional) sb.Append("? ");
            if (IsInverted) sb.Append('!');

            sb.Append(ModName);

            if (HasRestriction)
            {
                sb.Append(' ');
                sb.Append(RestrictionComparison);
                sb.Append(' ');
                sb.Append(RestrictionVersion);
            }

            stringRepresentation = sb.ToString();
        }

        /// <summary>
        /// Activates this dependency if possible.
        /// </summary>
        public void Activate(ModCollection mods, Version factorioVersion)
        {
            if (IsBase) return;

            var candidates = mods.Find(ModName, factorioVersion);
            if (HasRestriction)
            {
                var comparison = comparisonFunctions[RestrictionComparison];
                candidates = candidates.Where(candidate => comparison(candidate.Version, RestrictionVersion));
            }
            
            if (IsInverted)
            {
                foreach (var candidate in candidates)
                    candidate.Active = false;
            }
            else
            {
                if (!candidates.Any(candidate => candidate.Active))
                {
                    var max = candidates.MaxBy(candidate => candidate.Version, new VersionComparer());
                    max.Active = true;
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
