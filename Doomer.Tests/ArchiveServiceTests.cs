using Doomer.Services;
using System.IO.Compression;

namespace Doomer.Tests
{
    public class ArchiveServiceTests : IDisposable
    {
        private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "DoomerTests", Guid.NewGuid().ToString("N"));

        public ArchiveServiceTests() => Directory.CreateDirectory(_tempDir);

        public void Dispose() => Directory.Delete(_tempDir, recursive: true);

        [Theory]
        [InlineData("C:\\Downloads\\D.O.O.M..rar", "D.O.O.M")]
        [InlineData("C:\\Downloads\\Ancient Aliens.zip", "Ancient Aliens")]
        [InlineData("aaliens.7z", "Aaliens")]
        [InlineData("ancient aliens.zip", "Ancient Aliens")]
        [InlineData("my-house_v2.zip", "My-House_V2")]
        [InlineData("eviternity II.zip", "Eviternity II")]
        public void GetFolderName_DropsExtensionAndTrailingDotsAndCapitalizesWords(string archivePath, string expected)
        {
            Assert.Equal(expected, ArchiveService.GetFolderName(archivePath));
        }

        [Fact]
        public void Extract_KeepsFolderStructure()
        {
            var archive = CreateZip(("DrakeRC2.wad", "wad"), ("docs/readme.txt", "txt"));
            var destination = Path.Combine(_tempDir, "out");

            var files = ArchiveService.Extract(archive, destination);

            Assert.Equal(2, files.Count);
            Assert.Equal("wad", File.ReadAllText(Path.Combine(destination, "DrakeRC2.wad")));
            Assert.Equal("txt", File.ReadAllText(Path.Combine(destination, "docs", "readme.txt")));
        }

        [Fact]
        public void Extract_SkipsFolderSharedByEveryFile()
        {
            var archive = CreateZip(("sunlust/sunlust.wad", "wad"), ("sunlust/docs/sunlust.txt", "txt"));
            var destination = Path.Combine(_tempDir, "sunlust");

            ArchiveService.Extract(archive, destination);

            Assert.Equal("wad", File.ReadAllText(Path.Combine(destination, "sunlust.wad")));
            Assert.Equal("txt", File.ReadAllText(Path.Combine(destination, "docs", "sunlust.txt")));
            Assert.False(Directory.Exists(Path.Combine(destination, "sunlust")));
        }

        [Theory]
        [InlineData(new[] { "sunlust/sunlust.wad", "sunlust/sunlust.txt" }, "sunlust/")]
        [InlineData(new[] { "D.O.O.M\\DrakeRC2.wad", "d.o.o.m/sub/readme.txt" }, "D.O.O.M\\")]
        [InlineData(new[] { "sunlust.wad", "docs/sunlust.txt" }, "")]
        [InlineData(new[] { "a/one.wad", "b/two.wad" }, "")]
        [InlineData(new string[0], "")]
        public void GetSharedFolder_OnlyWhenEveryFileIsInTheSameFolder(string[] keys, string expected)
        {
            Assert.Equal(expected, ArchiveService.GetSharedFolder(keys));
        }

        [Fact]
        public void Extract_RejectsEntriesOutsideDestination()
        {
            var archive = CreateZip(("../evil.wad", "evil"));
            var destination = Path.Combine(_tempDir, "out");

            Assert.Throws<InvalidDataException>(() => ArchiveService.Extract(archive, destination));
            Assert.False(File.Exists(Path.Combine(_tempDir, "evil.wad")));
        }

        [Fact]
        public void FindWadCandidates_KeepsOnlyLoadableFiles()
        {
            var candidates = ArchiveService.FindWadCandidates(
                ["a\\DrakeRC2.WAD", "a\\readme.txt", "a\\music.pk3", "a\\shot.png", "a\\extra.pk7"]);

            Assert.Equal(["a\\DrakeRC2.WAD", "a\\music.pk3", "a\\extra.pk7"], candidates);
        }

        [Fact]
        public void FindExtraFiles_KeepsWadsAndPk3sFirstThenDehAndBexPatches()
        {
            var extra = ArchiveService.FindExtraFiles(
                ["C:\\w\\readme.TXT", "C:\\w\\a.deh", "C:\\w\\sunlust.wad", "C:\\w\\music.wad",
                 "C:\\w\\sub\\gfx.PK3", "C:\\w\\shot.png", "C:\\w\\b.bex"],
                "C:\\w\\SUNLUST.wad");

            Assert.Equal(["C:\\w\\music.wad", "C:\\w\\sub\\gfx.PK3", "C:\\w\\a.deh", "C:\\w\\b.bex"], extra);
        }

        private string CreateZip(params (string Name, string Content)[] entries)
        {
            var path = Path.Combine(_tempDir, Guid.NewGuid().ToString("N") + ".zip");

            using var zip = ZipFile.Open(path, ZipArchiveMode.Create);

            foreach (var (name, content) in entries)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name).Open());
                writer.Write(content);
            }

            return path;
        }
    }
}
