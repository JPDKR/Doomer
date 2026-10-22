using Doomer.Services;

namespace Doomer.Tests
{
    public class TempFileServiceTests : IDisposable
    {
        private readonly string _folder = Directory.CreateTempSubdirectory("Doomer temp ").FullName;

        public void Dispose() => Directory.Delete(_folder, recursive: true);

        private string CreateFile(string name, DateTime lastWriteUtc)
        {
            var path = Path.Combine(_folder, name);
            File.WriteAllText(path, name);
            File.SetLastWriteTimeUtc(path, lastWriteUtc);
            return path;
        }

        [Fact]
        public void NewPaths_AreUniqueAndInTheTempFolder()
        {
            var download = TempFileService.NewDownloadPath();
            var pasted = TempFileService.NewPastedImagePath();

            Assert.Equal(Path.GetTempPath(), Path.GetDirectoryName(download) + Path.DirectorySeparatorChar);
            Assert.EndsWith(".download", download);
            Assert.StartsWith("Doomer-pasted-", Path.GetFileName(pasted));
            Assert.EndsWith(".png", pasted);
            Assert.NotEqual(download, TempFileService.NewDownloadPath());
        }

        [Fact]
        public void DeleteLeftovers_RemovesOnlyTheAppsOldTempFiles()
        {
            var now = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
            var oldDownload = CreateFile("Doomer-0a1b.download", now.AddDays(-2));
            var oldPasted = CreateFile("Doomer-pasted-0a1b.png", now.AddDays(-2));
            var recentPasted = CreateFile("Doomer-pasted-2c3d.png", now.AddHours(-1));
            var otherApp = CreateFile("Other-0a1b.download", now.AddDays(-2));
            var ownImage = CreateFile("Doomer-icon.png", now.AddDays(-2));

            TempFileService.DeleteLeftovers(_folder, TimeSpan.FromDays(1), now);

            Assert.False(File.Exists(oldDownload));
            Assert.False(File.Exists(oldPasted));
            Assert.True(File.Exists(recentPasted));
            Assert.True(File.Exists(otherApp));
            Assert.True(File.Exists(ownImage));
        }

        [Fact]
        public void TryDelete_IgnoresAFileThatIsAlreadyGone()
        {
            TempFileService.TryDelete(Path.Combine(_folder, "missing.png"));
        }

        [Fact]
        public void DeleteLeftovers_IgnoresAMissingFolder()
        {
            TempFileService.DeleteLeftovers(Path.Combine(_folder, "missing"), TimeSpan.Zero, DateTime.UtcNow);
        }
    }
}
