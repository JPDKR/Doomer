using Doomer.Services;

namespace Doomer.Tests
{
    public class AtomicFileTests : IDisposable
    {
        private readonly string _folder = Directory.CreateTempSubdirectory("Doomer atomic ").FullName;

        public void Dispose() => Directory.Delete(_folder, recursive: true);

        [Fact]
        public void WriteAllText_ReplacesTheFileAndLeavesNoTempFile()
        {
            var path = Path.Combine(_folder, "appsettings.json");
            File.WriteAllText(path, "old");

            AtomicFile.WriteAllText(path, "new");

            Assert.Equal("new", File.ReadAllText(path));
            Assert.Equal([path], Directory.GetFiles(_folder));
        }

        [Fact]
        public void WriteAllText_CreatesTheMissingFolder()
        {
            var path = Path.Combine(_folder, "Doomer", "library.json");

            AtomicFile.WriteAllText(path, "{}");

            Assert.Equal("{}", File.ReadAllText(path));
        }

        [Fact]
        public void WriteAllText_OverwritesATempFileLeftByAnEarlierCrash()
        {
            var path = Path.Combine(_folder, "library.json");
            File.WriteAllText(path + ".tmp", "half-written");

            AtomicFile.WriteAllText(path, "{}");

            Assert.Equal("{}", File.ReadAllText(path));
            Assert.False(File.Exists(path + ".tmp"));
        }

        // A failed save leaves the previous file as it was, and cleans up after itself.
        [Fact]
        public void WriteAllText_KeepsTheOldFileWhenTheNewOneCantBeSwappedIn()
        {
            var path = Path.Combine(_folder, "appsettings.json");
            File.WriteAllText(path, "old");

            using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                // Windows reports a file in use that can't be replaced as access denied.
                Assert.ThrowsAny<UnauthorizedAccessException>(() => AtomicFile.WriteAllText(path, "new"));

            Assert.Equal("old", File.ReadAllText(path));
            Assert.False(File.Exists(path + ".tmp"));
        }
    }
}
