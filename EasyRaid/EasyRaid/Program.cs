using CommandLine;
using CommandLine.Text;
using System.Text.Json;

namespace EasyRaid
{
    internal class Program
    {
        static int Main(string[] args)
        {
            CommandLineOptions options = Utils.HandleCommandLineOptions(args);
            ConfigurationFile? configuration = JsonSerializer.Deserialize<ConfigurationFile>(File.ReadAllText(options.Config));
            if (configuration == null)
            {
                Console.WriteLine("Error! No configuration file found. Type easyraid -h for more info");
                return 0;
            }

            using FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.IncludeSubdirectories = true;
            watcher.Path = configuration.Source;
            watcher.Filter = "*"; // Monitor everything (TODO: maybe allow to specify which files to include/exclude ?)

            // Watch for changes
            watcher.NotifyFilter = NotifyFilters.LastWrite |
                                   NotifyFilters.LastAccess |
                                   NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName;

            watcher.Changed += (sender, e) =>
            {
                OnChanged(sender, e, configuration);
            };

            watcher.Created += (sender, e) =>
            {
                OnCreated(sender, e, configuration);
            };
            watcher.Deleted += (sender, e) =>
            {
                OnDeleted(sender, e, configuration);
            };
            watcher.Renamed += (sender, e) =>
            {
                OnRenamed(sender, e, configuration);
            }; ;

            // Begin watching
            watcher.EnableRaisingEvents = true;

            while (Console.Read() != 'q') ;
            return 0;
        }

        private static void OnChanged(object sender, FileSystemEventArgs e, ConfigurationFile configuration)
        {
            if (e.Name == null)
            {
                return;
            }

            string destinationPath = Path.Combine(configuration.Destination, e.Name);
            if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? ".");
            }
            FileAttributes attr = File.GetAttributes(e.FullPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                CopyDirectory(e.FullPath, Path.Combine(destinationPath, e.Name), true);
            }
            else
            {
                File.Copy(e.FullPath, destinationPath, true);
            }
        }

        private static void OnCreated(object sender, FileSystemEventArgs e, ConfigurationFile configuration)
        {
            if (e.Name == null)
            {
                return;
            }

            string destinationPath = Path.Combine(configuration.Destination, e.Name);
            if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? ".");
            }
            FileAttributes attr = File.GetAttributes(e.FullPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                CopyDirectory(e.FullPath, Path.Combine(destinationPath, e.Name), true);
            }
            else
            {
                File.Copy(e.FullPath, destinationPath, true);
            }
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e, ConfigurationFile configuration)
        {
            if (e.Name == null)
            {
                return;
            }
            string destinationPath = Path.Combine(configuration.Destination, e.Name);
            FileAttributes attr = File.GetAttributes(e.FullPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                Directory.Delete(destinationPath, true);
            }
            else
            {
                File.Delete(destinationPath);
            }
        }

        private static void OnRenamed(object sender, RenamedEventArgs e, ConfigurationFile configuration)
        {
            if (e.OldName == null || e.Name == null)
            {
                return;
            }
            string oldPath = Path.Combine(configuration.Destination, e.OldName);
            string newPath = Path.Combine(configuration.Destination, e.Name);
            Directory.Move(oldPath, newPath);
        }

        /// <summary>
        /// Copy recursively a directory
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        /// <param name="overwrite"></param>
        static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite)
        {
            Directory.CreateDirectory(destinationDir);

            // Copia tutti i file nella cartella corrente
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destinationDir, fileName);
                File.Copy(filePath, destFilePath, overwrite);
            }

            // Copia ricorsivamente le sottocartelle
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string folderName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(destinationDir, folderName);
                CopyDirectory(subDir, destSubDir, overwrite);
            }
        }
    }
}
