using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModMyFactory
{
    /// <summary>
    /// Represents a command line.
    /// </summary>
    sealed class CommandLine
    {
        private class CommandLineOption
        {
            public string Name { get; }

            public string Value { get; }

            public CommandLineOption(char name, string value)
            {
                Name = name.ToString();
                Value = value;
            }

            public CommandLineOption(string argument)
            {
                string[] parts = argument.Split('=');
                Name = parts[0];
                if (parts.Length >= 2) Value = parts[1];
            }
        }

        readonly List<CommandLineOption> options;

        public ReadOnlyCollection<string> Arguments { get; }

        public CommandLine(string[] arguments, params char[] argsWithValue)
        {
            options = new List<CommandLineOption>();
            var argumentList = new List<string>();
            Arguments = new ReadOnlyCollection<string>(argumentList);

            int index = 0;
            while (index < arguments.Length)
            {
                string argument = arguments[index];

                if (argument.StartsWith("--"))
                {
                    options.Add(new CommandLineOption(argument.Substring(2)));
                }
                else if (argument.StartsWith("-"))
                {
                    string value = string.Empty;
                    if ((index + 1 < arguments.Length) && !arguments[index + 1].StartsWith("-"))
                    {
                        value = arguments[index + 1];
                    }

                    bool valueUsed = false;
                    for (int i = 1; i < argument.Length; i++)
                    {
                        char arg = argument[i];
                        bool hasValue = argsWithValue.Contains(arg);

                        options.Add(new CommandLineOption(arg, hasValue ? value : null));
                        if (hasValue) valueUsed = true;
                    }

                    if (valueUsed) index++;
                }
                else
                {
                    argumentList.Add(argument);
                }

                index++;
            }
        }

        /// <summary>
        /// Checks if a command line option has been set.
        /// </summary>
        /// <param name="shortName">The options short name.</param>
        /// <param name="longName">The options long name.</param>
        /// <returns>Returns true if the option is set, otherwise false.</returns>
        public bool IsSet(char? shortName, string longName)
        {
            foreach (var option in options)
            {
                if (shortName.HasValue && (option.Name == shortName.Value.ToString())) return true;
                if (!string.IsNullOrEmpty(longName) && (option.Name == longName)) return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a command line option has been set.
        /// </summary>
        /// <param name="shortName">The options short name;</param>
        /// <returns>Returns true if the option is set, otherwise false.</returns>
        public bool IsSet(char shortName)
        {
            return IsSet(shortName, null);
        }

        /// <summary>
        /// Checks if a command line option has been set.
        /// </summary>
        /// <param name="longName">The options long name.</param>
        /// <returns>Returns true if the option is set, otherwise false.</returns>
        public bool IsSet(string longName)
        {
            return IsSet(null, longName);
        }

        /// <summary>
        /// Tries to get the argument of a command line option.
        /// </summary>
        /// <param name="shortName">The options short name;</param>
        /// <param name="longName">The options long name.</param>
        /// <param name="argument">Out. The argument of the command line option.</param>
        /// <returns>Returns true if the command line contains the specified option, otherwise false.</returns>
        public bool TryGetArgument(char? shortName, string longName, out string argument)
        {
            foreach (var option in options)
            {
                if (shortName.HasValue && (option.Name == shortName.Value.ToString()))
                {
                    argument = option.Value;
                    return true;
                }
                if (!string.IsNullOrEmpty(longName) && (option.Name == longName))
                {
                    argument = option.Value;
                    return true;
                }
            }

            argument = null;
            return false;
        }

        /// <summary>
        /// Tries to get the argument of a command line option.
        /// </summary>
        /// <param name="shortName">The options short name;</param>
        /// <param name="argument">Out. The argument of the command line option.</param>
        /// <returns>Returns true if the command line contains the specified option, otherwise false.</returns>
        public bool TryGetArgument(char shortName, out string argument)
        {
            return TryGetArgument(shortName, null, out argument);
        }

        /// <summary>
        /// Tries to get the argument of a command line option.
        /// </summary>
        /// <param name="longName">The options long name.</param>
        /// <param name="argument">Out. The argument of the command line option.</param>
        /// <returns>Returns true if the command line contains the specified option, otherwise false.</returns>
        public bool TryGetArgument(string longName, out string argument)
        {
            return TryGetArgument(null, longName, out argument);
        }
    }
}
