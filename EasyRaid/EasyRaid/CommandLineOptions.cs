using CommandLine;

namespace EasyRaid
{
    public class CommandLineOptions
    {
        [Option('v', "name", Required = false, HelpText = "Print the current version")]
        public bool Version { get; set; }

        [Option('h', "help", Required = false, HelpText = "Get help for this program")]
        public bool Help { get; set; }

        [Option('n', "new-config", Required = false, HelpText = "Create a new configuration file")]
        public IEnumerable<string>? NewConfig { get; set; }

        [Option('c', "config", Required = false, HelpText = "Set an existing configuration file")]
        public string? Config { get; set; }
    }
}
