using CommandLine;
using System.Text.Json;

namespace EasyRaid
{
    internal class Program
    {
        static int Main(string[] args)
        {
            string configurationFilePath = Configuration.DEFALT_CONFIGURATION_FILE_PATH;

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(o =>
                {
                    if (o.Help)
                    {
                        Console.WriteLine("EasyRaid is a file based Raid1-like application. WARNING! This is an alpha-state program. Use with caution");
                        Console.WriteLine("Usage:");
                        Console.WriteLine("EasyRaid [OPTIONS]\n");
                        Console.WriteLine("OPTIONS:\n" +
                            "-v, --version\n" +
                            "\tPrints the current version and exits immediately.\n\n" +
                            "-h, --help\n" +
                            "\tDisplays this help information.\n\n" +
                            "-n, --new-config <SOURCE_PATH> <DESTINATION_PATH>\n" +
                            "\tCreates a new configuration file.\n" +
                            "\tBoth source and destination paths are required\n\n" +
                            "-c, --config <CONFIG_FILE_PATH>\n" +
                            "\tLoads an existing configuration file.");
                        Environment.Exit(0); // Exit immediately
                    }

                    if (o.Version)
                    {
                        Console.WriteLine("EasyRaid version 0.1");
                        Environment.Exit(0); // Exit immediately
                    }

                    // Create a new configuration file
                    if (o.NewConfig != null && o.NewConfig.Any())
                    {
                        if (o.NewConfig.Count() == 2)
                        {
                            string source = o.NewConfig.ElementAt(0);
                            string destination = o.NewConfig.ElementAt(1);
                            Configuration.CreateConfigurationFile(source, destination);
                        }
                        else
                        {
                            Console.WriteLine("In order to create a new configuration file, you should specify two arguments. Example: EasyRaid <source> <destination>");
                        }

                        return;
                    }

                    if (!string.IsNullOrEmpty(o.Config))
                    {
                        if (File.Exists(o.Config))
                        {
                            Console.WriteLine($"Loaded configuration from {o.Config}");
                            string content = File.ReadAllText(o.Config);
                            Console.WriteLine(content);
                        }
                        else
                        {
                            Console.WriteLine($"Configuration file not found: {o.Config}");
                        }
                        return;
                    }

                    // Default here

                });

            ConfigurationFile? configuration = JsonSerializer.Deserialize<ConfigurationFile>(File.ReadAllText(configurationFilePath));
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
