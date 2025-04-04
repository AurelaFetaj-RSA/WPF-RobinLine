using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WPF_RobinLine.Configurations
{
    public static class ConfigurationManager
    {
        //private static readonly string ConfigPath = Path.Combine(
        //Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        //"RobinLine",
        //"config.json");

        private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "config.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static RobinConfiguration LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<RobinConfiguration>(json, JsonOptions)
                           ?? new RobinConfiguration();
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error loading config: {ex.Message}");
            }

            return new RobinConfiguration(); // Return defaults if file doesn't exist
        }

        public static void SaveConfig(RobinConfiguration config)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                var json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
}
