using System;

namespace ModMyFactory
{
    class InstanceStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The command line of the started instance.
        /// </summary>
        public CommandLine CommandLine { get; }

        public InstanceStartedEventArgs(CommandLine commandLine)
        {
            CommandLine = commandLine;
        }
    }
}
