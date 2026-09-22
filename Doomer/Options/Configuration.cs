using Microsoft.Extensions.Configuration;

namespace Doomer.Options
{
    public static class AppConfiguration
    {
        private const string FileName = "appsettings.json";

        private static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Doomer");

        public static readonly string FilePath = Path.Combine(DirectoryPath, FileName);

        public static GZDoomSettings GZDoom { get; private set; } = default!;
        public static IconsSettings Icons { get; private set; } = default!;

        static AppConfiguration() => Load();

        public static void Load()
        {
            EnsureConfigFile();

            var config = new ConfigurationBuilder()
                .SetBasePath(DirectoryPath)
                .AddJsonFile(FileName, optional: false, reloadOnChange: true)
                .Build();

            GZDoom = config.GetSection("GZDoom").Get<GZDoomSettings>()!;
            Icons = config.GetSection("Icons").Get<IconsSettings>()!;
        }

        // Seeds the per-user config on first run, since the app may be installed
        // to a location (e.g. Program Files) that standard users can't write to.
        private static void EnsureConfigFile()
        {
            if (File.Exists(FilePath))
                return;

            Directory.CreateDirectory(DirectoryPath);

            var bundledPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
            if (File.Exists(bundledPath))
                File.Copy(bundledPath, FilePath);
            else
                File.WriteAllText(FilePath, DefaultJson);
        }

        private const string DefaultJson = """
            {
              "GZDoom": {
                "Location": "",
                "Plugins": "plugins",
                "Batchs": {
                  "Location": "",
                  "Extension": ".bat"
                },
                "Images": {
                  "Location": "",
                  "Extension": ".png"
                }
              },
              "Icons": {
                "Width": 120,
                "Height": 100,
                "Padding": 5
              }
            }
            """;
    }

    public class AppSettingsRoot
    {
        public GZDoomSettings GZDoom { get; set; } = new();
        public IconsSettings Icons { get; set; } = new();
    }

    public class GZDoomSettings
    {
        public string Location { get; set; } = default!;
        public string Plugins { get; set; } = default!;
        public BatchSettings Batchs { get; set; } = new();
        public ImageSettings Images { get; set; } = new();
    }

    public class BatchSettings
    {
        public string Location { get; set; } = default!;
        public string Extension { get; set; } = default!;
    }

    public class ImageSettings
    {
        public string Location { get; set; } = default!;
        public string Extension { get; set; } = default!;
    }

    public class IconsSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Padding { get; set; }
    }
}
