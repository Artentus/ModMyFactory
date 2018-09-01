using ModMyFactory.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ModMyFactory.Helpers;
using ModMyFactory.Win32;

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

            Console.WriteLine(@"Usage:");
            Console.WriteLine(@"  modmyfactory.exe -h | --help");
            Console.WriteLine(@"  modmyfactory.exe [options] [<modpack-file>...]");
            Console.WriteLine(@"  modmyfactory.exe [options] -f <version> | --factorio-version=<version> [(-p <name> | --modpack=<name>) (-s <name> | --savegame=<name>) (-c <commandline> | --commands=<commandline>)]");
            Console.WriteLine();
            Console.WriteLine(@"Options:");
            Console.WriteLine(@"  -h, --help                                 Display this help message.");
            Console.WriteLine(@"  MODPACK-FILE                               Imports the specified modpack file.");
            Console.WriteLine(@"  -l, --no-logs                              Don't create crash logs.");
            Console.WriteLine(@"  -a PATH, --appdata-path=PATH               Override the default application data path.");
            Console.WriteLine(@"  -u, --no-update                            Don't search for update on startup.");
            Console.WriteLine(@"  -f VERSION, --factorio-version=VERSION     Start the specified version of Factorio.");
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

            // Custom AppData path for debugging purposes only.
            string appDataPath;
            bool hasCustomAppDataPath = commandLine.TryGetArgument('a', "appdata-path", out appDataPath);


            if (hasCustomAppDataPath)
                return new App(createCrashLog, appDataPath);
            else
                return new App(createCrashLog);
        }

        /// <summary>
        /// Starts Factorio if the command line specifies to do so.
        /// </summary>
        /// <param name="commandLine">The programs command line.</param>
        /// <param name="createApp">Specifies whether an app should be created.</param>
        /// <returns>Returns true if the game was started, otherwise false.</returns>
        private static bool StartGameIfSpecified(CommandLine commandLine, bool createApp)
        {
            string versionString;
            if (commandLine.TryGetArgument('f', "factorio-version", out versionString))
            {
                // Variable not used but sets 'Application.Current'.
                App app = null;
                if (createApp) app = CreateApp(commandLine);

                var versions = FactorioVersion.GetInstalledVersions();
                FactorioVersion steamVersion;
                if (FactorioSteamVersion.TryLoad(out steamVersion)) versions.Add(steamVersion);

                FactorioVersion factorioVersion = null;
                if (Regex.IsMatch(versionString, @"^[0-9]+\.[0-9]+$")) // Search for main version
                {
                    var v = Version.Parse(versionString);
                    factorioVersion = versions.Where(item => !item.IsSpecialVersion && item.Version.Major == v.Major && item.Version.Minor == v.Minor).MaxBy(item => item.Version, new VersionComparer());
                }
                else // Search for specific version
                {
                    if (string.Equals(versionString, FactorioVersion.LatestKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        factorioVersion = versions.MaxBy(item => item.Version, new VersionComparer());
                    }
                    else
                    {
                        factorioVersion = versions.Find(item => string.Equals(item.VersionString, versionString, StringComparison.InvariantCultureIgnoreCase));
                    }
                }

                if (factorioVersion != null)
                {
                    var startInfo = new ProcessStartInfo(factorioVersion.ExecutablePath);

                    var mods = new List<Mod>();
                    var modpacks = new List<Modpack>();

                    ModManager.BeginUpdateTemplates();
                    Mod.LoadMods(mods, modpacks);
                    ModpackTemplateList.Instance.PopulateModpackList(mods, modpacks, null);

                    mods.ForEach(mod => mod.Active = false);

                    string modpackName;
                    if (commandLine.TryGetArgument('p', "modpack", out modpackName))
                    {
                        Modpack modpack = modpacks.FirstOrDefault(item => item.Name == modpackName);
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

                    string savegameName;
                    if (commandLine.TryGetArgument('s', "savegame", out savegameName))
                    {
                        startInfo.Arguments = $"--load-game \"{savegameName}\"";
                    }

                    string factorioCommandline;
                    if (commandLine.TryGetArgument('c', "commands", out factorioCommandline))
                    {
                        if (!string.IsNullOrEmpty(startInfo.Arguments)) startInfo.Arguments += " ";
                        startInfo.Arguments += factorioCommandline.Replace('\'', '"');
                    }

                    ModManager.EndUpdateTemplates(true);
                    ModManager.SaveTemplates();
                    
                    Process.Start(startInfo);
                }
                else
                {
                    MessageBox.Show(
                        $"Factorio version {versionString} is not available.\nCheck your installed Factorio versions.",
                        "Error starting game!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return true;
            }

            return false;
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
                        arguments = arguments.Take(arguments.Length - 1).ToArray();
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
            var commandLine = new CommandLine(args);

            // Only display help.
            if (commandLine.IsSet('h', "help"))
            {
                Program.DisplayHelp();
                return 0;
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
                        if (!hasHandle)
                        {
                            // App already running.
                            StartGameIfSpecified(commandLine, true);
                            SendNewInstanceStartedMessage(args.Concat(new[] { NewInstanceGameStartedSpecifier }).ToArray());

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
