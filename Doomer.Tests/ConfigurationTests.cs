using Doomer.Options;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json.Nodes;

namespace Doomer.Tests
{
    public class ConfigurationTests
    {
        private const string LegacyJson = """
            {
              "GZDoom": {
                "Location": "D:\\gzdoom\\gzdoom.exe",
                "Plugins": "mods",
                "Batchs": { "Location": "D:\\GZDoom\\batchs", "Extension": ".cmd" },
                "Images": { "Location": "D:\\GZDoom\\pics", "Extension": ".jpg" },
                "Wads": { "Location": "D:\\GZDoom\\wads" }
              },
              "DSDADoom": { "Location": "E:\\DSDA\\dsda-doom.exe" },
              "Icons": { "Width": 140, "Height": 110, "Padding": 8 },
              "General": { "Language": "es" }
            }
            """;

        // Reads the JSON the way AppConfiguration does, so the test checks what the app would get.
        private static AppSettingsRoot Bind(string json)
        {
            var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json))).Build();

            return new AppSettingsRoot
            {
                Ports = config.GetSection("Ports").Get<PortsSettings>() ?? new(),
                Folders = config.GetSection("Folders").Get<FoldersSettings>() ?? new(),
                Interface = config.GetSection("Interface").Get<InterfaceSettings>() ?? new()
            };
        }

        [Fact]
        public void MigrateLegacyJson_MovesEverySettingToTheNewLayout()
        {
            var settings = Bind(SettingsFile.MigrateLegacyJson(LegacyJson)!);

            Assert.Equal("D:\\gzdoom\\gzdoom.exe", settings.Ports.GZDoom.Path);
            Assert.Equal("mods", settings.Ports.GZDoom.Plugins);
            Assert.Equal("E:\\DSDA\\dsda-doom.exe", settings.Ports.DSDADoom.Path);
            Assert.Equal("D:\\GZDoom\\batchs", settings.Folders.Batches.Path);
            Assert.Equal(".cmd", settings.Folders.Batches.Extension);
            Assert.Equal("D:\\GZDoom\\pics", settings.Folders.Images.Path);
            Assert.Equal(".jpg", settings.Folders.Images.Extension);
            Assert.Equal("D:\\GZDoom\\wads", settings.Folders.Wads.Path);
            var icons = settings.Interface.Icons;
            Assert.Equal((140, 110, 8), (icons.Width, icons.Height, icons.Padding));
            Assert.Equal("es", settings.Interface.Language);
        }

        [Fact]
        public void MigrateLegacyJson_FillsInWhatOlderFilesDontHave()
        {
            // Like a file from before the WADs folder, DSDA Doom and the language setting existed.
            const string old = """
                {
                  "GZDoom": {
                    "Location": "D:\\gzdoom\\gzdoom.exe",
                    "Batchs": { "Location": "D:\\GZDoom\\batchs" },
                    "Images": { "Location": "D:\\GZDoom\\pics" }
                  },
                  "Icons": { "Width": 120, "Height": 100, "Padding": 5 }
                }
                """;

            var settings = Bind(SettingsFile.MigrateLegacyJson(old)!);

            Assert.Equal("", settings.Ports.DSDADoom.Path);
            Assert.Equal("plugins", settings.Ports.GZDoom.Plugins);
            Assert.Equal(".bat", settings.Folders.Batches.Extension);
            Assert.Equal(".png", settings.Folders.Images.Extension);
            Assert.Equal("", settings.Folders.Wads.Path);
            Assert.Equal("", settings.Interface.Language);
        }

        [Theory]
        [InlineData("""{ "Ports": { "GZDoom": { "Path": "" } }, "GZDoom": { "Location": "x" } }""")]
        [InlineData("""{ "Icons": { "Width": 120 } }""")]
        [InlineData("not json")]
        [InlineData("[1, 2]")]
        [InlineData("")]
        public void MigrateLegacyJson_LeavesOtherFilesAlone(string json)
        {
            Assert.Null(SettingsFile.MigrateLegacyJson(json));
        }

        [Fact]
        public void MigrateLegacyFile_RewritesTheFileAndKeepsACopyOfTheOldOne()
        {
            var folder = Directory.CreateTempSubdirectory("Doomer settings ").FullName;

            try
            {
                var file = Path.Combine(folder, "appsettings.json");
                var backup = Path.Combine(folder, "appsettings.old.json");
                File.WriteAllText(file, LegacyJson);

                Assert.Null(SettingsFile.MigrateLegacyFile(file, backup));

                Assert.Equal(LegacyJson, File.ReadAllText(backup));
                Assert.Equal("D:\\gzdoom\\gzdoom.exe", Bind(File.ReadAllText(file)).Ports.GZDoom.Path);

                // Already migrated: nothing changes the second time.
                var migrated = File.ReadAllText(file);
                File.Delete(backup);
                Assert.Null(SettingsFile.MigrateLegacyFile(file, backup));
                Assert.Equal(migrated, File.ReadAllText(file));
                Assert.False(File.Exists(backup));
            }
            finally
            {
                Directory.Delete(folder, recursive: true);
            }
        }

        [Fact]
        public void Serialize_WritesTheSectionsInOrder()
        {
            var root = JsonNode.Parse(SettingsFile.Serialize(new AppSettingsRoot()))!.AsObject();

            Assert.Equal(["Ports", "Folders", "Interface"], root.Select(p => p.Key));
            Assert.Equal(["GZDoom", "DSDADoom"], root["Ports"]!.AsObject().Select(p => p.Key));
            Assert.Equal(["Path", "Plugins"], root["Ports"]!["GZDoom"]!.AsObject().Select(p => p.Key));
            Assert.Equal(["Path"], root["Ports"]!["DSDADoom"]!.AsObject().Select(p => p.Key));
            Assert.Equal(["Batches", "Images", "Wads"], root["Folders"]!.AsObject().Select(p => p.Key));
            Assert.Equal(["Path", "Extension"], root["Folders"]!["Batches"]!.AsObject().Select(p => p.Key));
            Assert.Equal(["Path"], root["Folders"]!["Wads"]!.AsObject().Select(p => p.Key));
            Assert.Equal(["Language", "Icons"], root["Interface"]!.AsObject().Select(p => p.Key));
        }

        [Fact]
        public void TheBundledSettingsUseTheNewLayout()
        {
            var bundled = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var json = File.Exists(bundled)
                ? File.ReadAllText(bundled)
                : File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Doomer", "appsettings.json"));

            Assert.Null(SettingsFile.MigrateLegacyJson(json));
            Assert.Equal(".bat", Bind(json).Folders.Batches.Extension);
        }

        [Theory]
        [InlineData(".bat", ".bat")]
        [InlineData("bat", ".bat")]
        [InlineData("  .PNG  ", ".PNG")]
        [InlineData("cmd", ".cmd")]
        [InlineData("tar.gz", ".tar.gz")]
        public void TryNormalizeExtension_AddsTheMissingDot(string text, string expected)
        {
            Assert.True(SettingsFile.TryNormalizeExtension(text, out var extension));
            Assert.Equal(expected, extension);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(".")]
        [InlineData("bat.")]
        [InlineData("*")]
        [InlineData(".b?t")]
        [InlineData("b at")]
        [InlineData("..\\bat")]
        public void TryNormalizeExtension_RejectsExtensionsThatCantEndAFileName(string? text)
        {
            Assert.False(SettingsFile.TryNormalizeExtension(text, out _));
        }

        [Theory]
        [InlineData("png", ".png")]
        [InlineData("", ".bat")]
        [InlineData("*", ".bat")]
        public void NormalizeExtension_FallsBackForInvalidOnes(string text, string expected)
        {
            Assert.Equal(expected, SettingsFile.NormalizeExtension(text, ".bat"));
        }
    }
}
