using Doomer.Services;
using System.Text.Json;

namespace Doomer.Tests
{
    public class UpdateServiceTests
    {
        private const string Sha = "5eb0062e0c6d62ec39d873936f743b574f880262fb37f7088d715af3a98e8d10";

        private static JsonElement Release(string tag, params string[] assetNames) => ReleaseWithDigest(tag, $"sha256:{Sha}", assetNames);

        private static JsonElement ReleaseWithDigest(string tag, string? digest, params string[] assetNames)
        {
            var assets = assetNames.Select(n => new { name = n, browser_download_url = $"https://example.com/{n}", digest });
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
            Assert.Equal(Sha, update.Sha256);
        }

        [Fact]
        public void ParseRelease_StillOffersAnInstallerWithoutDigest()
        {
            var release = ReleaseWithDigest("v1.1.0", null, "DoomerSetup-1.1.0.exe");

            var update = UpdateService.ParseRelease(release, new Version(1, 0, 0));

            // Offered, so the user is told why it can't be installed instead of never hearing of it.
            Assert.NotNull(update);
            Assert.Null(update.Sha256);
        }

        [Theory]
        [InlineData("sha256:5EB0062E0C6D62EC39D873936F743B574F880262FB37F7088D715AF3A98E8D10", Sha)]
        [InlineData("SHA256:" + Sha, Sha)]
        [InlineData("sha1:da39a3ee5e6b4b0d3255bfef95601890afd80709", null)]
        [InlineData("sha256:5eb0062e", null)]
        [InlineData("sha256:" + "zz" + "b0062e0c6d62ec39d873936f743b574f880262fb37f7088d715af3a98e8d10", null)]
        [InlineData(Sha, null)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void ParseSha256Digest_AcceptsOnlyFullSha256Hashes(string? digest, string? expected)
        {
            Assert.Equal(expected, UpdateService.ParseSha256Digest(digest));
        }

        [Fact]
        public void HasSha256_ComparesTheFilesHash()
        {
            var path = Path.GetTempFileName();

            try
            {
                File.WriteAllText(path, "abc");

                // SHA-256 of "abc", from FIPS 180-2.
                Assert.True(UpdateService.HasSha256(path, "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"));
                Assert.False(UpdateService.HasSha256(path, Sha));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task DownloadInstallerAsync_RefusesAReleaseWithoutHash()
        {
            var update = new UpdateInfo(new Version(9, 0, 0), "https://example.invalid/DoomerSetup-9.0.0.exe", "DoomerSetup-9.0.0.exe", null);

            await Assert.ThrowsAsync<InvalidDataException>(() => UpdateService.DownloadInstallerAsync(update));
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
