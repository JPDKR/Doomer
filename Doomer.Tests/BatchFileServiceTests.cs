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

            Assert.True(BatchFileService.TryParseCommand(command, out var port, out var iwad, out var wad, out var plugins));
            Assert.Equal(SourcePort.GZDoom, port);
            Assert.Equal("wads/doom2", iwad);
            Assert.Equal("wads/Ancient Aliens/aaliens", wad);
            Assert.Equal("smoothed", plugins);
        }

        [Fact]
        public void TryParseCommand_StripsExtensionRegardlessOfHowItWasEntered()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2.wad", "aaliens.wad", "");

            Assert.True(BatchFileService.TryParseCommand(command, out _, out var iwad, out var wad, out _));
            Assert.Equal("wads/doom2", iwad);
            Assert.Equal("aaliens", wad);
        }

        [Fact]
        public void TryParseCommand_WithoutPlugins_ReturnsEmptyPlugins()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "aaliens", "");

            Assert.True(BatchFileService.TryParseCommand(command, out _, out _, out _, out var plugins));
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_UnrecognizedFormat_ReturnsFalse()
        {
            Assert.False(BatchFileService.TryParseCommand("not a gzdoom command", out _, out var iwad, out var wad, out var plugins));
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

            Assert.True(BatchFileService.TryParseCommand(command, out var port, out var iwad, out var wad, out var plugins));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("WadSmoosh/source_wads/tnt", iwad);
            Assert.Equal("wads/D.O.O.M\\DrakeRC2", wad);
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_DSDADoom_AcceptsUnquotedExecutable()
        {
            const string command = "E:\\DSDA\\dsda-doom \"WadSmoosh/source_wads/tnt.wad\" \"wads/D.O.O.M\\DrakeRC2.wad\"";

            Assert.True(BatchFileService.TryParseCommand(command, out var port, out var iwad, out var wad, out _));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("WadSmoosh/source_wads/tnt", iwad);
            Assert.Equal("wads/D.O.O.M\\DrakeRC2", wad);
        }

        [Fact]
        public void TryParseCommand_GZDoom_AcceptsUnquotedExecutableAndPk3()
        {
            const string command = "E:\\GZDoom\\gzdoom -iwad \"E:\\dsda\\wads\\doom2.wad\" -file \"E:\\dsda\\wads\\My House\\myhouse.pk3\"";

            Assert.True(BatchFileService.TryParseCommand(command, out var port, out var iwad, out var wad, out _));
            Assert.Equal(SourcePort.GZDoom, port);
            Assert.Equal("E:\\dsda\\wads\\doom2", iwad);
            Assert.Equal("E:\\dsda\\wads\\My House\\myhouse.pk3", wad);
        }
    }
}
