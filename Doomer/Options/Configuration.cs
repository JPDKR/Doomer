using Microsoft.Extensions.Configuration;

namespace Doomer.Options
{
    public static class AppConfiguration
    {
        public static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        public static GZDoomSettings GZDoom { get; private set; } = default!;
        public static IconsSettings Icons { get; private set; } = default!;

        static AppConfiguration() => Load();

        public static void Load()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            GZDoom = config.GetSection("GZDoom").Get<GZDoomSettings>()!;
            Icons = config.GetSection("Icons").Get<IconsSettings>()!;
        }
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
