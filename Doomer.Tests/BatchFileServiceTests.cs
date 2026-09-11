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
                "\"D:\\gzdoom\\gzdoom\" -iwad \"wads/doom2\" -file \"wads/Ancient Aliens/aaliens.wad\"",
                command);
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
        public void TryParseCommand_RoundTripsBuildCommand()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "wads/Ancient Aliens/aaliens", "smoothed");

            Assert.True(BatchFileService.TryParseCommand(command, out var iwad, out var wad, out var plugins));
            Assert.Equal("wads/doom2", iwad);
            Assert.Equal("wads/Ancient Aliens/aaliens", wad);
            Assert.Equal("smoothed", plugins);
        }

        [Fact]
        public void TryParseCommand_WithoutPlugins_ReturnsEmptyPlugins()
        {
            var command = BatchFileService.BuildCommand(Settings, "wads/doom2", "aaliens", "");

            Assert.True(BatchFileService.TryParseCommand(command, out _, out _, out var plugins));
            Assert.Empty(plugins);
        }

        [Fact]
        public void TryParseCommand_UnrecognizedFormat_ReturnsFalse()
        {
            Assert.False(BatchFileService.TryParseCommand("not a gzdoom command", out var iwad, out var wad, out var plugins));
            Assert.Empty(iwad);
            Assert.Empty(wad);
            Assert.Empty(plugins);
        }
    }
}
