using Doomer.Options;
using Doomer.Services;

namespace Doomer.Tests
{
    public class BatchFileServiceTests
    {
        private const string GZDoomPath = "D:\\gzdoom\\gzdoom";

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
            var images = new ImageSettings { Path = "D:\\pics", Extension = ".png" };

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
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "wads/Ancient Aliens/aaliens", []);

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
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "aaliens", ["smoothed"]);

            Assert.StartsWith("rem Doomer plugins folder: plugins\r\n\"D:", command);
            Assert.EndsWith("\"plugins/smoothed\"", command);
        }

        [Theory]
        [InlineData("evil\" -exec calc", "wad", "")]
        [InlineData("iwad", "evil\" -exec calc", "")]
        [InlineData("iwad", "wad", "evil\"plugin")]
        public void BuildCommand_RejectsQuoteCharacters(string iwad, string wad, string plugin)
        {
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", iwad, wad, [plugin]));
        }

        [Fact]
        public void BuildCommand_AppendsEveryPluginInOrderAfterTheExtras()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "sunlust", ["smoothed.wad", "IDClever.pk3", " "],
                ["sunlust.deh"]);

            Assert.EndsWith("\"sunlust.deh\" \"plugins/smoothed.wad\" \"plugins/IDClever.pk3\"", command);
        }

        [Fact]
        public void TryParseCommand_RoundTripsSeveralPlugins()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "sunlust", ["smoothed.wad", "cards/cards.pk3"],
                ["sunlust.deh"]);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out var extraFiles, out var plugins, out _));
            Assert.Equal(["sunlust.deh"], extraFiles);
            Assert.Equal(["smoothed.wad", "cards/cards.pk3"], plugins);
        }

        [Fact]
        public void BuildCommand_WithoutPlugins_WritesNoPluginsFolder()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "sunlust", [], ["sunlust.deh"]);

            Assert.DoesNotContain("rem", command);
        }

        [Fact]
        public void TryParseCommand_FindsThePluginsAfterThePluginsFolderChanged()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "sunlust", ["smoothed.wad", "IDClever.pk3"],
                ["sunlust.deh"]);

            Assert.True(BatchFileService.TryParseCommand(command, "mods", out _, out _, out _, out var extraFiles, out var plugins, out _));
            Assert.Equal(["sunlust.deh"], extraFiles);
            Assert.Equal(["smoothed.wad", "IDClever.pk3"], plugins);
        }

        [Fact]
        public void TryParseCommand_WithoutAWrittenFolder_UsesTheCurrentSetting()
        {
            const string command = "\"D:\\gzdoom\\gzdoom\" -iwad \"doom2.wad\" -file \"sunlust.wad\" \"plugins/smoothed.wad\"";

            Assert.True(BatchFileService.TryParseCommand(command, "mods", out _, out _, out _, out var extraFiles, out var plugins, out _));
            Assert.Equal(["plugins/smoothed.wad"], extraFiles);
            Assert.Empty(plugins);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out extraFiles, out plugins, out _));
            Assert.Empty(extraFiles);
            Assert.Equal(["smoothed.wad"], plugins);
        }

        [Fact]
        public void TryParseCommand_OnlyTakesTheTrailingFilesAsPlugins()
        {
            const string command = "\"D:\\gzdoom\\gzdoom\" -iwad \"doom2.wad\" -file \"sunlust.wad\" " +
                "\"plugins/early.wad\" \"sunlust.deh\" \"plugins/smoothed\"";

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out var extraFiles, out var plugins, out _));
            Assert.Equal(["plugins/early.wad", "sunlust.deh"], extraFiles);
            Assert.Equal(["smoothed"], plugins);
        }

        [Fact]
        public void BuildCommand_RejectsQuoteCharactersInSettings()
        {
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildGZDoomCommand("D:\\gzdoom\\evil\" -exec calc", "plugins", "iwad", "wad", []));
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildGZDoomCommand(GZDoomPath, "evil\" -exec calc", "iwad", "wad", ["x"]));
        }

        [Fact]
        public void TryParseCommand_RoundTripsBuildCommand()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "wads/Ancient Aliens/aaliens", ["smoothed"]);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out var iwad, out var wad, out _, out var plugins, out _));
            Assert.Equal(SourcePort.GZDoom, port);
            Assert.Equal("wads/doom2", iwad);
            Assert.Equal("wads/Ancient Aliens/aaliens", wad);
            Assert.Equal(["smoothed"], plugins);
        }

        [Fact]
        public void TryParseCommand_StripsExtensionRegardlessOfHowItWasEntered()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2.wad", "aaliens.wad", []);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out var iwad, out var wad, out _, out _, out _));
            Assert.Equal("wads/doom2", iwad);
            Assert.Equal("aaliens", wad);
        }

        [Fact]
        public void TryParseCommand_WithoutPlugins_ReturnsEmptyPlugins()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "aaliens", []);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out _, out var plugins, out _));
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_UnrecognizedFormat_ReturnsFalse()
        {
            Assert.False(BatchFileService.TryParseCommand("not a gzdoom command", "plugins", out _, out var iwad, out var wad, out _, out var plugins, out _));
            Assert.Empty(iwad);
            Assert.Empty(wad);
            Assert.Empty(plugins);
        }

        private const string DsdaPath = "E:\\DSDA\\dsda-doom";

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
            var command = BatchFileService.BuildGZDoomCommand("E:\\GZDoom\\gzdoom", "plugins", "E:\\dsda\\wads\\doom2", "E:\\dsda\\wads\\My House\\myhouse.pk3", []);

            Assert.Equal(
                "\"E:\\GZDoom\\gzdoom\" -iwad \"E:\\dsda\\wads\\doom2.wad\" -file \"E:\\dsda\\wads\\My House\\myhouse.pk3\"",
                command);
        }

        [Fact]
        public void BuildCommand_DSDADoom_PassesIWadAndWadPositionally()
        {
            var command = BatchFileService.BuildDSDADoomCommand(DsdaPath, "WadSmoosh/source_wads/tnt", "wads/D.O.O.M\\DrakeRC2");

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
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildDSDADoomCommand(DsdaPath, iwad, wad));
        }

        [Fact]
        public void TryParseCommand_DSDADoom_RoundTripsBuildCommand()
        {
            var command = BatchFileService.BuildDSDADoomCommand(DsdaPath, "WadSmoosh/source_wads/tnt", "wads/D.O.O.M\\DrakeRC2");

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out var iwad, out var wad, out _, out var plugins, out _));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("WadSmoosh/source_wads/tnt", iwad);
            Assert.Equal("wads/D.O.O.M\\DrakeRC2", wad);
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_DSDADoom_AcceptsUnquotedExecutable()
        {
            const string command = "E:\\DSDA\\dsda-doom \"WadSmoosh/source_wads/tnt.wad\" \"wads/D.O.O.M\\DrakeRC2.wad\"";

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out var iwad, out var wad, out _, out _, out _));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("WadSmoosh/source_wads/tnt", iwad);
            Assert.Equal("wads/D.O.O.M\\DrakeRC2", wad);
        }

        [Fact]
        public void TryParseCommand_GZDoom_RoundTripsExtraFilesAndPlugin()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "wads/Sunlust/sunlust", ["smoothed"],
                ["wads/Sunlust/music.wad", "wads/Sunlust/sunlust.deh"]);

            Assert.Equal(
                "rem Doomer plugins folder: plugins\r\n" +
                "\"D:\\gzdoom\\gzdoom\" -iwad \"wads/doom2.wad\" -file \"wads/Sunlust/sunlust.wad\" " +
                "\"wads/Sunlust/music.wad\" \"wads/Sunlust/sunlust.deh\" \"plugins/smoothed\"",
                command);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out var wad, out var extraFiles, out var plugins, out _));
            Assert.Equal("wads/Sunlust/sunlust", wad);
            Assert.Equal(["wads/Sunlust/music.wad", "wads/Sunlust/sunlust.deh"], extraFiles);
            Assert.Equal(["smoothed"], plugins);
        }

        [Fact]
        public void TryParseCommand_GZDoom_ExtraFileWithoutPluginIsNotTakenAsPlugin()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "wads/doom2", "sunlust", [], ["sunlust.deh"]);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out var extraFiles, out var plugins, out _));
            Assert.Equal(["sunlust.deh"], extraFiles);
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_DSDADoom_RoundTripsExtraFiles()
        {
            var command = BatchFileService.BuildDSDADoomCommand(DsdaPath, "tnt", "wads/D.O.O.M\\DrakeRC2", ["wads/D.O.O.M\\drake.deh"]);

            Assert.EndsWith("\"wads/D.O.O.M\\DrakeRC2.wad\" \"wads/D.O.O.M\\drake.deh\"", command);
            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out _, out var wad, out var extraFiles, out _, out _));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("wads/D.O.O.M\\DrakeRC2", wad);
            Assert.Equal(["wads/D.O.O.M\\drake.deh"], extraFiles);
        }

        [Fact]
        public void BuildCommand_RejectsQuoteCharactersInExtraFiles()
        {
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "iwad", "wad", [], ["evil\" -exec calc"]));
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildDSDADoomCommand(DsdaPath, "iwad", "wad", ["evil\" -exec calc"]));
        }

        [Fact]
        public void TryParseCommand_GZDoom_AcceptsUnquotedExecutableAndPk3()
        {
            const string command = "E:\\GZDoom\\gzdoom -iwad \"E:\\dsda\\wads\\doom2.wad\" -file \"E:\\dsda\\wads\\My House\\myhouse.pk3\"";

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out var iwad, out var wad, out _, out _, out _));
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
            Assert.Equal(expected, BatchFileService.GetPluginsDirectory(plugins, location));
        }

        [Theory]
        [InlineData("D:\\gzdoom\\gzdoom", "")]
        [InlineData("", "plugins")]
        [InlineData("gzdoom", "plugins")]
        public void GetPluginsDirectory_ReturnsNullWithoutAFolderToGoBy(string location, string plugins)
        {
            Assert.Null(BatchFileService.GetPluginsDirectory(plugins, location));
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

        [Theory]
        [InlineData("D:\\DSDA\\wads\\sigil", "E:\\GZDoom\\gzdoom", "D:\\DSDA\\wads\\sigil.wad")]
        [InlineData("wads/Ancient Aliens/aaliens", "D:\\gzdoom\\gzdoom", "D:\\gzdoom\\wads\\Ancient Aliens\\aaliens.wad")]
        [InlineData("mods/myhouse.pk3", "D:\\gzdoom\\gzdoom.exe", "D:\\gzdoom\\mods\\myhouse.pk3")]
        [InlineData("D:\\wads\\..\\other\\eviternity", "", "D:\\other\\eviternity.wad")]
        public void ResolveWadPath_ResolvesRelativePathsFromTheExecutablesFolder(string wad, string executable, string expected)
        {
            Assert.Equal(expected, BatchFileService.ResolveWadPath(wad, executable));
        }

        [Theory]
        [InlineData("")]
        [InlineData("gzdoom")]
        public void ResolveWadPath_ReturnsNullForARelativePathWithoutAnExecutable(string executable)
        {
            Assert.Null(BatchFileService.ResolveWadPath("wads/doom2", executable));
        }

        [Theory]
        [InlineData("Sigil", new string[0], "Sigil (2)")]
        [InlineData("Sigil", new[] { "Sigil (2)", "Sigil (3)" }, "Sigil (4)")]
        [InlineData("Sigil (2)", new string[0], "Sigil (3)")]
        [InlineData("Doom (1993)", new string[0], "Doom (1994)")]
        [InlineData("(2)", new string[0], "(2) (2)")]
        public void GetDuplicateName_PicksTheNextFreeNumber(string name, string[] taken, string expected)
        {
            Assert.Equal(expected, BatchFileService.GetDuplicateName(name, taken.Contains));
        }

        [Fact]
        public void BuildCommand_GZDoom_WritesTheArgumentsAfterThePlugins()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "doom2", "sunlust", ["smoothed"], ["sunlust.deh"],
                "  -skill 4 +set name \"Doom Guy\"  ");

            Assert.EndsWith("\"sunlust.deh\" \"plugins/smoothed\" -skill 4 +set name \"Doom Guy\"", command);
        }

        [Fact]
        public void BuildCommand_DSDADoom_WritesTheArgumentsLast()
        {
            var command = BatchFileService.BuildDSDADoomCommand(DsdaPath, "tnt", "sigil", ["sigil.deh"], "-complevel 9");

            Assert.Equal("\"E:\\DSDA\\dsda-doom\" \"tnt.wad\" \"sigil.wad\" \"sigil.deh\" -complevel 9", command);
        }

        [Theory]
        [InlineData("-skill 4\r\ndel *.*")]
        [InlineData("-skill 4\n")]
        public void BuildCommand_RejectsArgumentsWithLineBreaks(string arguments)
        {
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "doom2", "wad", [], [], arguments));
            Assert.Throws<ArgumentException>(() => BatchFileService.BuildDSDADoomCommand(DsdaPath, "doom2", "wad", [], arguments));
        }

        [Fact]
        public void TryParseCommand_GZDoom_RoundTripsArguments()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "doom2", "sunlust", ["smoothed"], ["sunlust.deh"],
                "-skill 4 +set name \"Doom Guy\"");

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out var extraFiles, out var plugins,
                out var arguments));
            Assert.Equal(["sunlust.deh"], extraFiles);
            Assert.Equal(["smoothed"], plugins);
            Assert.Equal("-skill 4 +set name \"Doom Guy\"", arguments);
        }

        [Fact]
        public void TryParseCommand_DSDADoom_RoundTripsArguments()
        {
            var command = BatchFileService.BuildDSDADoomCommand(DsdaPath, "tnt", "sigil", ["sigil.deh"], "-complevel 9 -skill 4");

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out var port, out _, out _, out var extraFiles, out _,
                out var arguments));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal(["sigil.deh"], extraFiles);
            Assert.Equal("-complevel 9 -skill 4", arguments);
        }

        // Batches edited by hand before arguments could be set from the app keep them when saved again.
        [Fact]
        public void TryParseCommand_KeepsArgumentsAddedByHand()
        {
            const string command = "\"D:\\gzdoom\\gzdoom\" -iwad \"doom2.wad\" -file \"sunlust.wad\" -warp 3 -skill 4\r\npause\r\n";

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out var wad, out var extraFiles, out _,
                out var arguments));
            Assert.Equal("sunlust", wad);
            Assert.Empty(extraFiles);
            Assert.Equal("-warp 3 -skill 4", arguments);
        }

        [Fact]
        public void TryParseCommand_WithoutArguments_ReturnsEmptyArguments()
        {
            var command = BatchFileService.BuildGZDoomCommand(GZDoomPath, "plugins", "doom2", "sunlust", ["smoothed"]);

            Assert.True(BatchFileService.TryParseCommand(command, "plugins", out _, out _, out _, out _, out _, out var arguments));
            Assert.Empty(arguments);
        }

        [Fact]
        public void FindPlugins_ReturnsNoneForAMissingFolder()
        {
            Assert.Empty(BatchFileService.FindPlugins("D:\\does\\not\\exist\\" + Guid.NewGuid()));
            Assert.Empty(BatchFileService.FindPlugins(null));
        }
    }
}
