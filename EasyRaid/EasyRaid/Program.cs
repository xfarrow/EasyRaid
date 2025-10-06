using CommandLine;
using System.IO;
using System.Text.Json;
using static EasyRaid.CommandLineOptions;

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
                            "\tCreates a new configuration file called 'easyraid-config.json'\n" +
                            "\tin the specified destination folder. Both source and destination\n" +
                            "\tpaths are required\n\n" +
                            "-c, --config <CONFIG_FILE_PATH>\n" +
                            "\tLoads and prints an existing configuration file.");
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

            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = configuration.Source;
                watcher.Filter = "*"; // Monitor all

                // Watch for changes
                watcher.NotifyFilter = NotifyFilters.LastWrite |
                                       NotifyFilters.LastAccess |
                                       NotifyFilters.FileName |
                                       NotifyFilters.DirectoryName;

                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;

                // Begin watching
                watcher.EnableRaisingEvents = true;

                while (Console.Read() != 'q');
                return 0;
            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File changed: {e.FullPath} {e.ChangeType}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File created: {e.FullPath}");
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File deleted: {e.FullPath}");
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"File renamed from {e.OldFullPath} to {e.FullPath}");
        }
    }
}
