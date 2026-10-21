using Doomer.Options;
using Doomer.Services;

namespace Doomer.Tests
{
    public class BatchFileServiceTests
    {
        private static readonly GZDoomSettings Settings = new()
        {
            Location = "D:\\gzdoom\\gzdoom",
            Plugins = "plugins",
        };

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void IsValidFileName_RejectsEmpty(string fileName)
        {
            Assert.False(BatchFileService.IsValidFileName(fileName, out var error));
            Assert.NotEmpty(error);
        }

        [Theory]
        [InlineData("../evil")]
        [InlineData("..\\evil")]
        [InlineData("evil:name")]
        [InlineData("evil*name")]
        public void IsValidFileName_RejectsPathTraversalAndInvalidChars(string fileName)
        {
            Assert.False(BatchFileService.IsValidFileName(fileName, out var error));
            Assert.NotEmpty(error);
        }

        [Theory]
        [InlineData("D:\\batchs\\Sunlust.bat", "D:\\pics\\Sunlust.png")]
        [InlineData("D:\\batchs\\D.O.O.M.bat", "D:\\pics\\D.O.O.M.png")]
        [InlineData("D:\\batchs\\Ancient Aliens .bat", "D:\\pics\\Ancient Aliens .png")]
        public void GetImagePath_UsesTheBatchFileNameAsIs(string batchPath, string expected)
        {
            var images = new ImageSettings { Location = "D:\\pics", Extension = ".png" };

            Assert.Equal(expected, BatchFileService.GetImagePath(batchPath, images));
        }

        [Fact]
        public void IsValidFileName_AcceptsNormalName()
        {
            Assert.True(BatchFileService.IsValidFileName("Ancient Aliens", out var error));
            Assert.Empty(error);
        }

        [Fact]
        public void BuildCommand_QuotesEachPathSegment()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "wads/Ancient Aliens/aaliens", "");

            Assert.Equal(
                "\"D:\\gzdoom\\gzdoom\" -iwad \"wads/doom2.wad\" -file \"wads/Ancient Aliens/aaliens.wad\"",
                command);
        }

        [Theory]
        [InlineData("wads/doom2", "wads/doom2.wad")]
        [InlineData("wads/doom2.wad", "wads/doom2.wad")]
        [InlineData("wads/doom2.WAD", "wads/doom2.WAD")]
        public void EnsureWadExtension_AppendsOnlyWhenMissing(string input, string expected)
        {
            Assert.Equal(expected, BatchFileService.EnsureWadExtension(input));
        }

        [Theory]
        [InlineData("C:\\Downloads\\aaliens.wad", "E:\\dsda\\wads", "E:\\dsda\\wads\\aaliens.wad")]
        [InlineData("C:\\Downloads\\myhouse.pk3", "E:\\dsda\\wads\\", "E:\\dsda\\wads\\myhouse.pk3")]
        [InlineData("E:\\dsda\\wads-old\\aaliens.wad", "E:\\dsda\\wads", "E:\\dsda\\wads\\aaliens.wad")]
        public void GetWadDestination_MovesIntoWadsDirectory(string wad, string wadsDirectory, string expected)
        {
            Assert.Equal(expected, BatchFileService.GetWadDestination(wad, wadsDirectory));
        }

        [Theory]
        [InlineData("E:\\dsda\\wads\\aaliens.wad", "E:\\dsda\\wads")]
        [InlineData("E:\\DSDA\\Wads\\Ancient Aliens\\aaliens.wad", "E:\\dsda\\wads")]
        [InlineData("wads/aaliens.wad", "E:\\dsda\\wads")]
        [InlineData("C:\\Downloads\\aaliens.wad", "")]
        public void GetWadDestination_ReturnsNullWhenNothingToMove(string wad, string wadsDirectory)
        {
            Assert.Null(BatchFileService.GetWadDestination(wad, wadsDirectory));
        }

        [Fact]
        public void BuildCommand_AppendsPluginsWhenProvided()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "aaliens", "smoothed");

            Assert.EndsWith("\"plugins/smoothed\"", command);
        }

        [Theory]
        [InlineData("evil\" -exec calc", "wad", "")]
        [InlineData("iwad", "evil\" -exec calc", "")]
        [InlineData("iwad", "wad", "evil\"plugin")]
        public void BuildCommand_RejectsQuoteCharacters(string iwad, string wad, string plugins)
        {
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildCommand(Settings, iwad, wad, plugins));
        }

        [Fact]
        public void BuildCommand_RejectsQuoteCharactersInSettings()
        {
            var settings = new GZDoomSettings { Location = "D:\\gzdoom\\evil\" -exec calc", Plugins = "plugins" };

            Assert.Throws<ArgumentException>(() => BatchFileService.BuildCommand(settings, "iwad", "wad", ""));
        }

        [Fact]
        public void TryParseCommand_RoundTripsBuildCommand()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "wads/Ancient Aliens/aaliens", "smoothed");

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out var iwad, out var wad, out _, out var plugins));
            Assert.Equal(SourcePort.GZDoom, port);
            Assert.Equal("wads/doom2", iwad);
            Assert.Equal("wads/Ancient Aliens/aaliens", wad);
            Assert.Equal("smoothed", plugins);
        }

        [Fact]
        public void TryParseCommand_StripsExtensionRegardlessOfHowItWasEntered()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2.wad", "aaliens.wad", "");

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out var iwad, out var wad, out _, out _));
            Assert.Equal("wads/doom2", iwad);
            Assert.Equal("aaliens", wad);
        }

        [Fact]
        public void TryParseCommand_WithoutPlugins_ReturnsEmptyPlugins()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "aaliens", "");

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out _, out var plugins));
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_UnrecognizedFormat_ReturnsFalse()
        {
            Assert.False(BatchFileService.TryParseCommand("not a gzdoom command", "plugins", out _, out var iwad, out var wad, out _, out var plugins));
            Assert.Empty(iwad);
            Assert.Empty(wad);
            Assert.Empty(plugins);
        }

        private static readonly DSDADoomSettings DsdaSettings = new() { Location = "E:\\DSDA\\dsda-doom" };

        [Theory]
        [InlineData("E:\\dsda\\wads\\My House\\myhouse.pk3", "E:\\dsda\\wads\\My House\\myhouse.pk3")]
        [InlineData("mods/brutal.pk7", "mods/brutal.pk7")]
        public void EnsureWadExtension_KeepsOtherExtensions(string input, string expected)
        {
            Assert.Equal(expected, BatchFileService.EnsureWadExtension(input));
        }

        [Fact]
        public void BuildCommand_GZDoom_KeepsPk3Extension()
        {
            var settings = new GZDoomSettings { Location = "E:\\GZDoom\\gzdoom", Plugins = "plugins" };

            var command = BatchFileService.BuildCommand(settings, "E:\\dsda\\wads\\doom2", "E:\\dsda\\wads\\My House\\myhouse.pk3", "");

            Assert.Equal(
                "\"E:\\GZDoom\\gzdoom\" -iwad \"E:\\dsda\\wads\\doom2.wad\" -file \"E:\\dsda\\wads\\My House\\myhouse.pk3\"",
                command);
        }

        [Fact]
        public void BuildCommand_DSDADoom_PassesIWadAndWadPositionally()
        {
            var command = BatchFileService.BuildCommand(DsdaSettings, "WadSmoosh/source_wads/tnt", "wads/D.O.O.M\\DrakeRC2");

            Assert.Equal(
                "\"E:\\DSDA\\dsda-doom\" \"WadSmoosh/source_wads/tnt.wad\" \"wads/D.O.O.M\\DrakeRC2.wad\"",
                command);
        }

        [Theory]
        [InlineData("wads/My House/myhouse.pk3", new string[0], true)]
        [InlineData("mods/brutal.PK7", new string[0], true)]
        [InlineData("wads/Sunlust/sunlust", new[] { "wads/Sunlust/gfx.pk3" }, true)]
        [InlineData("wads/Sunlust/sunlust", new[] { "wads/Sunlust/sunlust.deh", "music.wad" }, false)]
        [InlineData("wads/D.O.O.M\\DrakeRC2", new string[0], false)]
        public void RequiresGZDoom_WhenAnyFileIsAPk3OrPk7(string wad, string[] extraFiles, bool expected)
        {
            Assert.Equal(expected, BatchFileService.RequiresGZDoom(wad, extraFiles));
        }

        [Theory]
        [InlineData("evil\" -exec calc", "wad")]
        [InlineData("iwad", "evil\" -exec calc")]
        public void BuildCommand_DSDADoom_RejectsQuoteCharacters(string iwad, string wad)
        {
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildCommand(DsdaSettings, iwad, wad));
        }

        [Fact]
        public void TryParseCommand_DSDADoom_RoundTripsBuildCommand()
        {
            var command = BatchFileService.BuildCommand(DsdaSettings, "WadSmoosh/source_wads/tnt", "wads/D.O.O.M\\DrakeRC2");

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out var iwad, out var wad, out _, out var plugins));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("WadSmoosh/source_wads/tnt", iwad);
            Assert.Equal("wads/D.O.O.M\\DrakeRC2", wad);
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_DSDADoom_AcceptsUnquotedExecutable()
        {
            const string command = "E:\\DSDA\\dsda-doom \"WadSmoosh/source_wads/tnt.wad\" \"wads/D.O.O.M\\DrakeRC2.wad\"";

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out var iwad, out var wad, out _, out _));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("WadSmoosh/source_wads/tnt", iwad);
            Assert.Equal("wads/D.O.O.M\\DrakeRC2", wad);
        }

        [Fact]
        public void TryParseCommand_GZDoom_RoundTripsExtraFilesAndPlugin()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "wads/Sunlust/sunlust", "smoothed",
                ["wads/Sunlust/music.wad", "wads/Sunlust/sunlust.deh"]);

            Assert.Equal(
                "\"D:\\gzdoom\\gzdoom\" -iwad \"wads/doom2.wad\" -file \"wads/Sunlust/sunlust.wad\" " +
                "\"wads/Sunlust/music.wad\" \"wads/Sunlust/sunlust.deh\" \"plugins/smoothed\"",
                command);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out var wad, out var extraFiles, out var plugins));
            Assert.Equal("wads/Sunlust/sunlust", wad);
            Assert.Equal(["wads/Sunlust/music.wad", "wads/Sunlust/sunlust.deh"], extraFiles);
            Assert.Equal("smoothed", plugins);
        }

        [Fact]
        public void TryParseCommand_GZDoom_ExtraFileWithoutPluginIsNotTakenAsPlugin()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "sunlust", "", ["sunlust.deh"]);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out var extraFiles, out var plugins));
            Assert.Equal(["sunlust.deh"], extraFiles);
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_DSDADoom_RoundTripsExtraFiles()
        {
            var command = BatchFileService.BuildCommand(DsdaSettings, "tnt", "wads/D.O.O.M\\DrakeRC2", ["wads/D.O.O.M\\drake.deh"]);

            Assert.EndsWith("\"wads/D.O.O.M\\DrakeRC2.wad\" \"wads/D.O.O.M\\drake.deh\"", command);
            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out _, out var wad, out var extraFiles, out _));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("wads/D.O.O.M\\DrakeRC2", wad);
            Assert.Equal(["wads/D.O.O.M\\drake.deh"], extraFiles);
        }

        [Fact]
        public void BuildCommand_RejectsQuoteCharactersInExtraFiles()
        {
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildCommand(Settings, "iwad", "wad", "", ["evil\" -exec calc"]));
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildCommand(DsdaSettings, "iwad", "wad", ["evil\" -exec calc"]));
        }

        [Fact]
        public void TryParseCommand_GZDoom_AcceptsUnquotedExecutableAndPk3()
        {
            const string command = "E:\\GZDoom\\gzdoom -iwad \"E:\\dsda\\wads\\doom2.wad\" -file \"E:\\dsda\\wads\\My House\\myhouse.pk3\"";

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out var iwad, out var wad, out _, out _));
            Assert.Equal(SourcePort.GZDoom, port);
            Assert.Equal("E:\\dsda\\wads\\doom2", iwad);
            Assert.Equal("E:\\dsda\\wads\\My House\\myhouse.pk3", wad);
        }

        [Theory]
        [InlineData("D:\\gzdoom\\gzdoom", "plugins", "D:\\gzdoom\\plugins")]
        [InlineData("D:\\gzdoom\\gzdoom.exe", "mods\\plugins", "D:\\gzdoom\\mods\\plugins")]
        [InlineData("D:\\gzdoom\\gzdoom", "E:\\plugins", "E:\\plugins")]
        [InlineData("", "E:\\plugins", "E:\\plugins")]
        public void GetPluginsDirectory_ResolvesRelativeFoldersFromTheGZDoomInstall(string location, string plugins, string expected)
        {
            var settings = new GZDoomSettings { Location = location, Plugins = plugins };

            Assert.Equal(expected, BatchFileService.GetPluginsDirectory(settings));
        }

        [Theory]
        [InlineData("D:\\gzdoom\\gzdoom", "")]
        [InlineData("", "plugins")]
        [InlineData("gzdoom", "plugins")]
        public void GetPluginsDirectory_ReturnsNullWithoutAFolderToGoBy(string location, string plugins)
        {
            var settings = new GZDoomSettings { Location = location, Plugins = plugins };

            Assert.Null(BatchFileService.GetPluginsDirectory(settings));
        }

        [Fact]
        public void FindPlugins_ListsDoomFilesSortedByName()
        {
            var folder = Directory.CreateTempSubdirectory("Doomer plugins ").FullName;

            try
            {
                foreach (var name in new[] { "smoothed.wad", "IDClever.pk3", "readme.txt", "cards.PK3" })
                    File.WriteAllText(Path.Combine(folder, name), "");
                Directory.CreateDirectory(Path.Combine(folder, "sub.wad"));

                Assert.Equal(["cards.PK3", "IDClever.pk3", "smoothed.wad"], BatchFileService.FindPlugins(folder));
            }
            finally
            {
                Directory.Delete(folder, recursive: true);
            }
        }

        [Fact]
        public void FindPlugins_ReturnsNoneForAMissingFolder()
        {
            Assert.Empty(BatchFileService.FindPlugins("D:\\does\\not\\exist\\" + Guid.NewGuid()));
            Assert.Empty(BatchFileService.FindPlugins(null));
        }
    }
}
