using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyRaid
{
    class Configuration
    {
        public static readonly string DEFALT_CONFIGURATION_FILE_PATH = Path.Combine(GetConfigDirectory(), ".easyraid.conf");

        public static void CreateConfigurationFile(string source, string destination)
        {
            ConfigurationFile configFile = new(source, destination);
            string directoryPath = Path.GetDirectoryName(DEFALT_CONFIGURATION_FILE_PATH)!;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string json = JsonSerializer.Serialize(configFile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(DEFALT_CONFIGURATION_FILE_PATH, json);
            Console.WriteLine(DEFALT_CONFIGURATION_FILE_PATH);
        }

        private static string GetConfigDirectory()
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyRaid");
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return Path.Combine(homePath, $".{"EasyRaid"}");
            }

            throw new PlatformNotSupportedException("Unsupported operating system.");
        }
    }

    class ConfigurationFile
    {
        public string Source { get; set; } = String.Empty;

        public string Destination { get; set; } = String.Empty;

        public ConfigurationFile(string source, string destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
