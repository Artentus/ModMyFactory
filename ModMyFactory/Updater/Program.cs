using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;

namespace ModMyFactory.Updater
{
    class Program
    {
        static void WaitAndExit(int exitCode = 0)
        {
            Console.WriteLine("Exiting in 5 seconds...");
            Thread.Sleep(5000);
            Environment.Exit(exitCode);
        }

        static Process GetMMFProess(string[] args)
        {
            int pid = -1;
            if (!int.TryParse(args[0], out pid))
            {
                Console.WriteLine($"'{args[0]}' is not a number.");
                WaitAndExit(2);
            }

            try
            {
                return Process.GetProcessById(pid);
            }
            catch
            {
                Console.WriteLine($"Process id {pid} is invalid.");
                WaitAndExit(2);
                return null;
            }
        }

        static void Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"ModMyFactory Updater v{version.ToString(2)}");
            Console.WriteLine();

            if (args.Length != 1)
            {
                Console.WriteLine("Incorrect number of arguments.");
                WaitAndExit(1);
            }

            var mmfProcess = GetMMFProess(args);
            var mmfAssembly = new FileInfo(mmfProcess.MainModule.FileName);
            if (mmfAssembly.Name != "ModMyFactory.exe")
            {
                Console.WriteLine("Incorrect process specified.");
                WaitAndExit(2);
            }

            Console.WriteLine("Waiting for ModMyFactory to exit...");
            mmfProcess.WaitForExit();

            var updateFile = new FileInfo("update.zip");
            if (!updateFile.Exists)
            {
                Console.WriteLine("Update file not found.");
                WaitAndExit(3);
            }

            Console.WriteLine("Backing up old files...");
            var installDir = mmfAssembly.Directory;
            var backupDir = new DirectoryInfo(Path.Combine(installDir.FullName, "backup"));
            backupDir.Create();
            foreach (var file in installDir.EnumerateFiles())
            {
                if (file.Extension != ".json")
                    file.MoveTo(Path.Combine(backupDir.FullName, file.Name));
            }

            Console.WriteLine("Extracting update package...");
            try
            {
                ZipFile.ExtractToDirectory(updateFile.FullName, installDir.FullName);
            }
            catch
            {
                Console.WriteLine("Failed to extract update package, restoring backup...");
                foreach (var file in installDir.EnumerateFiles())
                {
                    if (file.Extension != ".json")
                        file.Delete();
                }
                foreach (var file in backupDir.EnumerateFiles())
                    file.MoveTo(Path.Combine(installDir.FullName, file.Name));
                backupDir.Delete();
                Console.WriteLine("Backup restored.");
                WaitAndExit(3);
            }
            Console.WriteLine("Update extracted successfully.");

            Console.WriteLine("Deleting backup...");
            backupDir.Delete(true);
            
            Console.WriteLine($"Starting ModMyFactory...");
            var startInfo = new ProcessStartInfo(mmfAssembly.FullName, $"--update-complete={Process.GetCurrentProcess().Id}");
            startInfo.WorkingDirectory = installDir.FullName;
            Process.Start(startInfo);

            Thread.Sleep(1000);
            Environment.Exit(0);
        }
    }
}
