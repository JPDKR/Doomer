using Doomer.Services;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Doomer.Options
{
    public static class AppConfiguration
    {
        private const string FileName = "appsettings.json";

        private static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Doomer");

        public static readonly string FilePath = Path.Combine(DirectoryPath, FileName);

        // A copy of the settings as they were before being moved to the current layout.
        public static readonly string LegacyBackupFilePath = Path.Combine(DirectoryPath, "appsettings.old.json");

        // Favorites, play history and sort order (see LibraryService), next to the settings.
        public static readonly string LibraryFilePath = Path.Combine(DirectoryPath, "library.json");

        // The update version the user chose to skip. Not in the settings, since saving those rewrites the file.
        public static readonly string SkippedUpdateFilePath = Path.Combine(DirectoryPath, "skipped-update.txt");

        public static PortsSettings Ports { get; private set; } = default!;
        public static FoldersSettings Folders { get; private set; } = default!;
        public static InterfaceSettings Interface { get; private set; } = default!;

        static AppConfiguration() => Load();

        public static void Load()
        {
            EnsureConfigFile();

            var builder = new ConfigurationBuilder().SetBasePath(DirectoryPath);

            // Settings that can't be rewritten in the new layout are still read in it, from memory.
            if (SettingsFile.MigrateLegacyFile(FilePath, LegacyBackupFilePath) is { } unsaved)
                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(unsaved)));
            else
                builder.AddJsonFile(FileName, optional: false, reloadOnChange: false);

            // The values are copied out below and the app reloads them itself after saving the settings,
            // so nothing watches the file and the configuration is let go once read.
            var config = builder.Build();

            try
            {
                Ports = config.GetSection("Ports").Get<PortsSettings>() ?? new();
                Folders = config.GetSection("Folders").Get<FoldersSettings>() ?? new();
                Interface = config.GetSection("Interface").Get<InterfaceSettings>() ?? new();
            }
            finally
            {
                (config as IDisposable)?.Dispose();
            }

            // Settings saves only valid extensions, but the file can be edited by hand.
            Folders.Batches.Extension = SettingsFile.NormalizeExtension(Folders.Batches.Extension, new BatchSettings().Extension);
            Folders.Images.Extension = SettingsFile.NormalizeExtension(Folders.Images.Extension, new ImageSettings().Extension);
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
                File.WriteAllText(FilePath, SettingsFile.Serialize(new AppSettingsRoot()));
        }
    }

    // Reads and writes the settings JSON. Kept apart from AppConfiguration, which loads the user's
    // settings as soon as it's used, so this can be called (and tested) without touching them.
    public static class SettingsFile
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private static readonly JsonSerializerOptions LegacyReadOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static string Serialize(AppSettingsRoot settings) => JsonSerializer.Serialize(settings, JsonOptions);

        // An extension the way the settings keep it, e.g. ".bat", adding the dot when it's left out ("bat").
        // False when it can't end a file name: empty, just ".", ending in a dot, or with spaces or characters
        // file names can't have (which include the * and ? that would turn it into a wildcard).
        public static bool TryNormalizeExtension(string? text, out string extension)
        {
            extension = text?.Trim() ?? string.Empty;

            if (extension.Length > 0 && extension[0] != '.')
                extension = "." + extension;

            return extension.Length > 1 && !extension.EndsWith('.') &&
                   !extension.Any(char.IsWhiteSpace) && extension.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        // The normalized extension, or fallback when it isn't a valid one.
        public static string NormalizeExtension(string? text, string fallback) =>
            TryNormalizeExtension(text, out var extension) ? extension : fallback;

        // Settings from 1.5 and earlier kept the executables and folders under "GZDoom" and "DSDADoom".
        // Returns the same settings in the current layout, or null when the JSON already uses it or can't be read.
        public static string? MigrateLegacyJson(string json)
        {
            JsonObject? root;

            try
            {
                root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                }) as JsonObject;
            }
            catch (JsonException)
            {
                return null;
            }

            if (root is null || root.ContainsKey("Ports") || root["GZDoom"] is not JsonObject gzdoom)
                return null;

            var defaults = new AppSettingsRoot();

            var settings = new AppSettingsRoot
            {
                Ports = new PortsSettings
                {
                    GZDoom = new GZDoomSettings
                    {
                        Path = Text(gzdoom["Location"]),
                        Plugins = Text(gzdoom["Plugins"], defaults.Ports.GZDoom.Plugins)
                    },
                    DSDADoom = new DSDADoomSettings { Path = Text(root["DSDADoom"]?["Location"]) }
                },
                Folders = new FoldersSettings
                {
                    Batches = new BatchSettings
                    {
                        Path = Text(gzdoom["Batchs"]?["Location"]),
                        Extension = Text(gzdoom["Batchs"]?["Extension"], defaults.Folders.Batches.Extension)
                    },
                    Images = new ImageSettings
                    {
                        Path = Text(gzdoom["Images"]?["Location"]),
                        Extension = Text(gzdoom["Images"]?["Extension"], defaults.Folders.Images.Extension)
                    },
                    Wads = new WadSettings { Path = Text(gzdoom["Wads"]?["Location"]) }
                },
                Interface = new InterfaceSettings
                {
                    Language = root["General"]?.Deserialize<LegacyGeneralSettings>(LegacyReadOptions)?.Language ?? string.Empty,
                    Icons = root["Icons"]?.Deserialize<IconsSettings>(LegacyReadOptions) ?? new()
                }
            };

            return Serialize(settings);
        }

        // Rewrites a file in the old layout in place, keeping a copy of it. Returns the migrated JSON only
        // when it couldn't be written, so it's still used this time.
        public static string? MigrateLegacyFile(string filePath, string backupPath)
        {
            string? migrated;

            try
            {
                migrated = MigrateLegacyJson(File.ReadAllText(filePath));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return null;
            }

            if (migrated is null)
                return null;

            try
            {
                File.Copy(filePath, backupPath, overwrite: true);
                AtomicFile.WriteAllText(filePath, migrated);
                return null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return migrated;
            }
        }

        private static string Text(JsonNode? node, string fallback = "") =>
            node is JsonValue value && value.TryGetValue<string>(out var text) ? text : fallback;
    }

    public class AppSettingsRoot
    {
        public PortsSettings Ports { get; set; } = new();
        public FoldersSettings Folders { get; set; } = new();
        public InterfaceSettings Interface { get; set; } = new();
    }

    // The source ports. At least one executable has to be set.
    public class PortsSettings
    {
        public GZDoomSettings GZDoom { get; set; } = new();
        public DSDADoomSettings DSDADoom { get; set; } = new();
    }

    public class GZDoomSettings
    {
        public string Path { get; set; } = string.Empty;
        // GZDoom's plugins folder: relative to the GZDoom install, or a full path.
        public string Plugins { get; set; } = "plugins";
    }

    public class DSDADoomSettings
    {
        public string Path { get; set; } = string.Empty;
    }

    public class FoldersSettings
    {
        public BatchSettings Batches { get; set; } = new();
        public ImageSettings Images { get; set; } = new();
        public WadSettings Wads { get; set; } = new();
    }

    public class BatchSettings
    {
        public string Path { get; set; } = string.Empty;
        public string Extension { get; set; } = ".bat";
    }

    public class ImageSettings
    {
        public string Path { get; set; } = string.Empty;
        public string Extension { get; set; } = ".png";
    }

    public class WadSettings
    {
        public string Path { get; set; } = string.Empty;
    }

    public class InterfaceSettings
    {
        // A culture name like "es", or empty to follow Windows' language.
        public string Language { get; set; } = string.Empty;
        public IconsSettings Icons { get; set; } = new();
    }

    public class IconsSettings
    {
        public int Width { get; set; } = 120;
        public int Height { get; set; } = 100;
        public int Padding { get; set; } = 5;
    }

    // The "General" section of settings from 1.5 and earlier, only read when migrating them.
    internal class LegacyGeneralSettings
    {
        public string Language { get; set; } = string.Empty;
    }
}
