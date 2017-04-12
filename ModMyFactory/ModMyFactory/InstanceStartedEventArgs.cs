using System;

namespace ModMyFactory
{
    class InstanceStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The command line of the started instance.
        /// </summary>
        public CommandLine CommandLine { get; }

        public bool GameStarted { get; }

        public InstanceStartedEventArgs(CommandLine commandLine, bool gameStarted)
        {
            CommandLine = commandLine;
            GameStarted = gameStarted;
        }
    }
}
