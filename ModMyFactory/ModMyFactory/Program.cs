using ModMyFactory.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ModMyFactory.Helpers;
using ModMyFactory.Win32;
using System.Diagnostics;

namespace ModMyFactory
{
    public static class Program
    {
        private const string NewInstanceGameStartedSpecifier = "_&_game_started_&_";


        private static readonly object SyncRoot;
        private static NamedPipeServerStream server;
        private static ManualResetEvent resetEvent;

        /// <summary>
        /// Occurs if the program gets started again.
        /// </summary>
        internal static event EventHandler<InstanceStartedEventArgs> NewInstanceStarted;

        /// <summary>
        /// Indicates whether ModMyFatory should check for updates on startup.
        /// </summary>
        internal static bool UpdateCheckOnStartup { get; private set; }

        /// <summary>
        /// A list of files that should be imported on startup.
        /// </summary>
        internal static List<FileInfo> ImportFileList { get; private set; } 

        /// <summary>
        /// The assemblys GUID.
        /// </summary>
        internal static Guid Guid => Guid.Parse(((GuidAttribute)(Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false)[0])).Value);

        static Program()
        {
            SyncRoot = new object();
        }

        /// <summary>
        /// Displays a help message in the console.
        /// </summary>
        private static void DisplayHelp()
        {
            bool attatchedConsole = Kernel32.TryAttachConsole();
            if (attatchedConsole)
            {
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                Kernel32.AllocConsole();
            }

            Console.WriteLine($"ModMyFactory v{App.Version}");
            Console.WriteLine();
            Console.WriteLine(@"Usage:");
            Console.WriteLine(@"  modmyfactory.exe -h | --help");
            Console.WriteLine(@"  modmyfactory.exe [options] [<modpack-file>...]");
            Console.WriteLine(@"  modmyfactory.exe [options] -f <version> | --factorio-version=<version> [(-p <name> | --modpack=<name>) (-s <name> | --savegame=<name>) (-c <commandline> | --commands=<commandline>)]");
            Console.WriteLine();
            Console.WriteLine(@"Options:");
            Console.WriteLine(@"  -h, --help                                 Display this help message.");
            Console.WriteLine(@"  MODPACK-FILE                               Imports the specified modpack file.");
            Console.WriteLine(@"  -l, --no-logs                              Don't create crash logs.");
            Console.WriteLine(@"  -u, --no-update                            Don't search for update on startup.");
            Console.WriteLine(@"  -t, --no-register-filetype                 Don't register file types on startup.");
            Console.WriteLine(@"  -n NAME, --factorio-name=NAME              Start the specified Factorio installation.");
            Console.WriteLine(@"  -f VERSION, --factorio-version=VERSION     Start a Factorio installation that matches this version.");
            Console.WriteLine(@"  -p NAME, --modpack=NAME                    Enable the specified modpack.");
            Console.WriteLine(@"  -s NAME, --savegame=NAME                   Load the specified savegame.");
            Console.WriteLine(@"  -c COMMANDLINE, --commands=COMMANDLINE     Start Factorio with the specified command line.");

            if (attatchedConsole)
            {
                System.Windows.Forms.SendKeys.SendWait("{Enter}");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine(@"Press any key to continue.");
                Console.ReadKey();
            }
            Kernel32.FreeConsole();
        }

        /// <summary>
        /// Creates the app as specified by the command line.
        /// </summary>
        /// <param name="commandLine">The programs command line.</param>
        private static App CreateApp(CommandLine commandLine)
        {
            // Do not create crash logs when debugging.
            bool createCrashLog = !commandLine.IsSet('l', "no-logs");

            // Do not register fily type associations when debugging.
            bool registerFileTypes = !commandLine.IsSet('t', "no-register-filetype");
            
            return new App(createCrashLog, registerFileTypes);
        }

        private static bool TryGetFactorioVersion(string name, bool isVersion, out FactorioVersion factorioVersion)
        {
            factorioVersion = null;
            var factorioVersions = FactorioCollection.Load();

            if (isVersion)
            {
                if (!Version.TryParse(name, out var version)) return false;
                bool exact = name.IndexOf('.') != name.LastIndexOf('.');
                factorioVersion = factorioVersions.Find(version, exact);
            }
            else
            {
                factorioVersion = factorioVersions.Find(name);
            }

            return factorioVersion != null;
        }

        private static void ActivateMods(CommandLine commandLine)
        {
            var mods = new ModCollection();
            var modpacks = new ModpackCollection();

            ModManager.BeginUpdateTemplates();
            Mod.LoadMods(mods, modpacks);
            ModpackTemplateList.Instance.PopulateModpackList(mods, modpacks, null);

            mods.ForEach(mod => mod.Active = false);

            string modpackName;
            if (commandLine.TryGetArgument('p', "modpack", out modpackName))
            {
                Modpack modpack = modpacks.Find(modpackName);
                if (modpack != null)
                {
                    modpack.Active = true;
                }
                else
                {
                    MessageBox.Show(
                        $"No modpack named '{modpackName}' found.\nThe game will be launched without any mods enabled.",
                        "Error loading modpack!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            ModManager.EndUpdateTemplates(true);
            ModManager.SaveTemplates();
        }

        private static string BuildArguments(CommandLine commandLine)
        {
            var sb = new StringBuilder();
            
            if (commandLine.TryGetArgument('s', "savegame", out string savegameName))
                sb.Append($"--load-game \"{savegameName}\"");
            
            if (commandLine.TryGetArgument('c', "commands", out string factorioCommandline))
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(factorioCommandline.Replace('\'', '"'));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Starts Factorio if the command line specifies to do so.
        /// </summary>
        /// <param name="commandLine">The programs command line.</param>
        /// <param name="createApp">Specifies whether an app should be created.</param>
        /// <returns>Returns true if the game was started, otherwise false.</returns>
        private static bool StartGameIfSpecified(CommandLine commandLine, bool createApp)
        {
            FactorioVersion factorioVersion;
            if (commandLine.TryGetArgument('n', "factorio-name", out string name))
            {
                // Sets 'Application.Current'
                if (createApp) CreateApp(commandLine);

                if (!TryGetFactorioVersion(name, false, out factorioVersion))
                {
                    MessageBox.Show(
                        $"A Factorio installation named '{name}' was not found.",
                        "Error starting game!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }
            else if (commandLine.TryGetArgument('f', "factorio-version", out string versionString))
            {
                // Sets 'Application.Current'
                if (createApp) CreateApp(commandLine);

                if (!TryGetFactorioVersion(versionString, true, out factorioVersion))
                {
                    MessageBox.Show(
                        $"A Factorio installation with version {versionString} was not found.",
                        "Error starting game!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }
            else
            {
                return false;
            }
            if (factorioVersion == null) return true;


            ActivateMods(commandLine);
            string args = BuildArguments(commandLine);
            factorioVersion.Run(args);

            return true;
        }

        /// <summary>
        /// Runs the program.
        /// </summary>
        /// <param name="commandLine">The programs command line.</param>
        /// <param name="app">The app to run.</param>
        /// <returns>The programs exit code.</returns>
        private static int Run(CommandLine commandLine, App app)
        {
            // Prevent update search on startup
            UpdateCheckOnStartup = !commandLine.IsSet('u', "no-update");


            var fileList = new List<FileInfo>();
            foreach (string argument in commandLine.Arguments)
            {
                if ((argument.EndsWith(".fmp") || argument.EndsWith(".fmpa")) && File.Exists(argument))
                {
                    var file = new FileInfo(argument);
                    fileList.Add(file);
                }
            }
            ImportFileList = fileList;


            app.InitializeComponent();
            return app.Run();
        }

        private static void SendNewInstanceStartedMessage(string[] arguments)
        {
            string appGuid = Program.Guid.ToString();
            string pipeId = $"{{{appGuid}}}";
            using (var client = new NamedPipeClientStream(".", pipeId, PipeDirection.Out, PipeOptions.Asynchronous))
            {
                client.Connect(10000);
                if (client.IsConnected)
                {
                    using (var writer = new BinaryWriter(client))
                    {
                        writer.Write(arguments.Length);

                        for (int i = 0; i < arguments.Length; i++)
                        {
                            byte[] argument = Encoding.UTF8.GetBytes(arguments[i]);
                            writer.Write(argument.Length);
                            writer.Write(argument);
                        }
                    }

                    client.WaitForPipeDrain();
                }
            }
        }

        private static void ListenInner(IAsyncResult result)
        {
            try
            {
                var tuple = (Tuple<NamedPipeServerStream, ManualResetEvent>)result.AsyncState;
                var server = tuple.Item1;
                var resetEvent = tuple.Item2;

                server.EndWaitForConnection(result);

                using (var reader = new BinaryReader(server))
                {
                    int argumentCount = reader.ReadInt32();
                    string[] arguments = new string[argumentCount];

                    for (int i = 0; i < argumentCount; i++)
                    {
                        int argumentLength = reader.ReadInt32();
                        byte[] buffer = new byte[argumentLength];
                        reader.Read(buffer, 0, argumentLength);

                        string argument = Encoding.UTF8.GetString(buffer);
                        arguments[i] = argument;
                    }

                    bool gameStarted = false;
                    if (arguments.Contains(NewInstanceGameStartedSpecifier))
                    {
                        gameStarted = true;
                        arguments = arguments.Where(arg => arg != NewInstanceGameStartedSpecifier).ToArray();
                    }
                    NewInstanceStarted?.Invoke(null, new InstanceStartedEventArgs(new CommandLine(arguments), gameStarted));
                }

                resetEvent.Set();
            }
            catch (ObjectDisposedException)
            { }
        }

        private static Task ListenForNewInstanceStarted(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.Register(() =>
                {
                    lock (SyncRoot)
                    {
                        server?.Close();
                        resetEvent?.Set();
                    }
                });

                string appGuid = Program.Guid.ToString();
                string pipeId = $"{{{appGuid}}}";

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        lock (SyncRoot)
                        {
                            server = new NamedPipeServerStream(pipeId, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                            resetEvent = new ManualResetEvent(false);
                        }

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            IAsyncResult result = server.BeginWaitForConnection(ListenInner, new Tuple<NamedPipeServerStream, ManualResetEvent>(server, resetEvent));
                            result.AsyncWaitHandle.WaitOne();

                            resetEvent.WaitOne();
                        }
                    }
                    finally
                    {
                        lock (SyncRoot)
                        {
                            resetEvent?.Close();
                            resetEvent = null;

                            server?.Close();
                            server = null;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            var commandLine = new CommandLine(args, 'n', 'f', 'p', 's', 'c');

            // Only display help.
            if (commandLine.IsSet('h', "help"))
            {
                Program.DisplayHelp();
                return 0;
            }

            // Wait for updater to exit.
            if (commandLine.TryGetArgument(null, "update-complete", out string pidStr))
            {
                if (int.TryParse(pidStr, out int pid))
                {
                    try
                    {
                        var updaterProcess = Process.GetProcessById(pid);
                        updaterProcess.WaitForExit();
                    }
                    catch
                    { }
                }
            }


            string appGuid = Program.Guid.ToString();
            string mutexId = $"{{{appGuid}}}";

            bool createdNew;
            using (var mutex = new Mutex(false, mutexId, out createdNew))
            {
                var hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(100, false);
                        if (!hasHandle) // App already running.
                        {
                            bool gameStarted = StartGameIfSpecified(commandLine, true);

                            var sendArgs = args;
                            if (gameStarted) sendArgs = sendArgs.Append(NewInstanceGameStartedSpecifier).ToArray();
                            SendNewInstanceStartedMessage(sendArgs);

                            return 0;
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        hasHandle = true;
                    }


                    // App not running.
                    App app = CreateApp(commandLine);

                    if (StartGameIfSpecified(commandLine, false)) return 0;


                    var cancellationSource = new CancellationTokenSource();
                    Task listenTask = ListenForNewInstanceStarted(cancellationSource.Token);

                    int result = Program.Run(commandLine, app);

                    cancellationSource.Cancel();
                    listenTask.Wait();
                    
                    return result;
                }
                finally
                {
                    if (hasHandle) mutex.ReleaseMutex();
                }
            }
        }
    }
}
