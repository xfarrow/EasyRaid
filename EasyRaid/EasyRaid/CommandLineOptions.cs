using CommandLine;
using CommandLine.Text;

namespace EasyRaid
{
    public class CommandLineOptions
    {
        [Option('n', "new-config", HelpText = "Creates a new configuration file. Requires <SOURCE_PATH> and <DESTINATION_PATH>.", Min = 2, Max = 2)]
        public IEnumerable<string> NewConfig { get; set; } = new List<string>();

        [Option('c', "config", HelpText = "Loads an existing configuration file.")]
        public string Config { get; set; } = Configuration.DEFALT_CONFIGURATION_FILE_PATH;

        [Option("verbose", HelpText = "Enables verbose mode.")]
        public bool Verbose { get; set; }
    }

    public class Utils
    {
        public static CommandLineOptions HandleCommandLineOptions(string[] args)
        {
            var parser = new Parser(with =>
            {
                with.HelpWriter = null; // Set to Console.Out if you want automatic help text
            });

            var result = parser.ParseArguments<CommandLineOptions>(args);

            result
                // The library successfully parsed the command line
                .WithParsed(o =>
                {
                    if (o.NewConfig != null && o.NewConfig.Any())
                    {
                        var paths = o.NewConfig.ToArray();
                        if (paths.Length == 2)
                        {
                            string source = paths[0];
                            string destination = paths[1];
                            Configuration.CreateConfigurationFile(source, destination);
                        }
                        else
                        {
                            Console.WriteLine("You must specify both source and destination paths.");
                            Environment.Exit(1);
                        }
                        return;
                    }

                    if (!string.IsNullOrEmpty(o.Config))
                    {
                        if (!File.Exists(o.Config))
                        {
                            Console.WriteLine($"Configuration file not found: {o.Config}");
                            Environment.Exit(1);
                        }
                        return;
                    }
                })

            // The library did not successfully parse the command line
            .WithNotParsed(errors =>
            {
                if (errors.IsVersion())
                {
                    Console.WriteLine("EasyRaid version 0.1");
                    Environment.Exit(0);
                }

                else if (errors.IsHelp())
                {
                    var helpText = HelpText.AutoBuild(result, h =>
                    {
                        h.Heading = "EasyRaid Help";
                        h.AddPreOptionsLine("EasyRaid is a file-based RAID1-like application.");
                        return h;
                    }, e => e);
                    Console.WriteLine(helpText);
                    Environment.Exit(0);
                }

                else
                {
                    Console.WriteLine("Command not recognized. Use --help for more info");
                }
                Environment.Exit(1);
            });

            return result.Value;
        }
    }
}
