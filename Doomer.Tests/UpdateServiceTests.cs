using Doomer.Services;
using System.Text.Json;

namespace Doomer.Tests
{
    public class UpdateServiceTests
    {
        private static JsonElement Release(string tag, params string[] assetNames)
        {
            var assets = assetNames.Select(n => new { name = n, browser_download_url = $"https://example.com/{n}" });
            return JsonSerializer.SerializeToElement(new { tag_name = tag, assets });
        }

        [Theory]
        [InlineData("v1.2.3", "1.2.3")]
        [InlineData("1.2", "1.2.0")]
        [InlineData("V2.0.0.0", "2.0.0")]
        public void TryParseVersion_AcceptsTags(string tag, string expected)
        {
            Assert.True(UpdateService.TryParseVersion(tag, out var version));
            Assert.Equal(Version.Parse(expected), version);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("latest")]
        public void TryParseVersion_RejectsInvalidTags(string? tag)
        {
            Assert.False(UpdateService.TryParseVersion(tag, out _));
        }

        [Fact]
        public void ParseRelease_ReturnsInstallerForNewerVersion()
        {
            var release = Release("v1.1.0", "notes.txt", "DoomerSetup-1.1.0.exe");

            var update = UpdateService.ParseRelease(release, new Version(1, 0, 0));

            Assert.NotNull(update);
            Assert.Equal(new Version(1, 1, 0), update.Version);
            Assert.Equal("DoomerSetup-1.1.0.exe", update.InstallerName);
            Assert.Equal("https://example.com/DoomerSetup-1.1.0.exe", update.InstallerUrl);
        }

        [Theory]
        [InlineData("v1.0.0")]
        [InlineData("v0.9.0")]
        public void ParseRelease_IgnoresSameOrOlderVersion(string tag)
        {
            var release = Release(tag, "DoomerSetup-1.0.0.exe");

            Assert.Null(UpdateService.ParseRelease(release, new Version(1, 0, 0)));
        }

        [Fact]
        public void ParseRelease_IgnoresReleaseWithoutInstaller()
        {
            var release = Release("v2.0.0", "source.zip");

            Assert.Null(UpdateService.ParseRelease(release, new Version(1, 0, 0)));
        }
    }
}
