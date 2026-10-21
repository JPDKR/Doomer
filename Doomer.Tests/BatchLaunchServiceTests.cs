using Doomer.Services;

namespace Doomer.Tests
{
    public class BatchLaunchServiceTests : IDisposable
    {
        private readonly string _folder = Path.Combine(Path.GetTempPath(), "Doomer tests " + Guid.NewGuid().ToString("N"));

        public BatchLaunchServiceTests() => Directory.CreateDirectory(_folder);

        public void Dispose() => Directory.Delete(_folder, recursive: true);

        private string WriteBatch(string name, string command)
        {
            var path = Path.Combine(_folder, name);
            File.WriteAllText(path, command);
            return path;
        }

        [Fact]
        public void CreateStartInfo_HidesTheConsole()
        {
            var info = BatchLaunchService.CreateStartInfo("C:\\Batchs\\Ancient Aliens.bat");

            Assert.Equal("cmd.exe", info.FileName);
            Assert.Equal("/d /c \"\"C:\\Batchs\\Ancient Aliens.bat\"\"", info.Arguments);
            Assert.True(info.CreateNoWindow);
            Assert.False(info.UseShellExecute);
        }

        [Fact]
        public async Task RunAsync_ReturnsNull_WhenTheBatchSucceeds()
        {
            var path = WriteBatch("ok & fine.bat", "exit /b 0");

            Assert.Null(await BatchLaunchService.RunAsync(path));
        }

        [Fact]
        public async Task RunAsync_ReturnsTheErrors_WhenTheExecutableIsMissing()
        {
            var path = WriteBatch("missing.bat", "\"C:\\nope\\gzdoom-missing.exe\" -iwad \"doom2.wad\"");

            var errors = await BatchLaunchService.RunAsync(path);

            // cmd's message depends on Windows' language, and doesn't always name the file.
            Assert.False(string.IsNullOrWhiteSpace(errors));
        }
    }
}
