using Doomer.Services;
using System.Text;

namespace Doomer.Tests
{
    public class BatchEncodingTests : IDisposable
    {
        private readonly string _folder = Directory.CreateTempSubdirectory("Doomer encoding ").FullName;

        public void Dispose() => Directory.Delete(_folder, recursive: true);

        [Fact]
        public void WriteBatch_SwitchesToUtf8OnlyWhenTheBatchNeedsIt()
        {
            var ascii = Path.Combine(_folder, "ascii.bat");
            var accented = Path.Combine(_folder, "accented.bat");

            BatchFileService.WriteBatch(ascii, "\"gzdoom\" -iwad \"doom2.wad\" -file \"sigil.wad\"");
            BatchFileService.WriteBatch(accented, "\"gzdoom\" -iwad \"doom2.wad\" -file \"Ñoño\\sigil.wad\"");

            Assert.DoesNotContain("chcp", File.ReadAllText(ascii));
            Assert.StartsWith("@chcp 65001>nul\r\n", File.ReadAllText(accented));
            Assert.False(File.ReadAllBytes(accented).AsSpan().StartsWith(Encoding.UTF8.Preamble));

            // Saving it again doesn't add the line twice, and parsing skips it.
            BatchFileService.WriteBatch(accented, BatchFileService.ReadBatch(accented));
            var batch = BatchFileService.ReadBatch(accented);

            Assert.Equal(2, batch.Split("chcp").Length);
            Assert.True(BatchFileService.TryParseCommand(batch, "plugins", out _, out _, out var wad, out _, out _, out _));
            Assert.Equal("Ñoño\\sigil", wad);
        }

        [Fact]
        public void ADsdaBatchWithAccents_StillParses()
        {
            var path = Path.Combine(_folder, "dsda.bat");
            BatchFileService.WriteBatch(path, BatchFileService.SetWikiUrl(
                BatchFileService.BuildDSDADoomCommand("E:\\dsda\\dsda-doom", "doom2", "Música\\sigil"), "https://doomwiki.org/wiki/Sigil"));

            var batch = BatchFileService.ReadBatch(path);

            Assert.True(BatchFileService.TryParseCommand(batch, "plugins", out var port, out _, out var wad, out _, out _, out _));
            Assert.Equal(SourcePort.DSDADoom, port);
            Assert.Equal("Música\\sigil", wad);
            Assert.Equal("https://doomwiki.org/wiki/Sigil", BatchFileService.GetWikiUrl(batch));
        }

        [Fact]
        public void ReadBatch_ReadsAHandWrittenBatchInTheOemCodePage()
        {
            var path = Path.Combine(_folder, "oem.bat");
            File.WriteAllBytes(path, ConsoleText.Oem.GetBytes("\"gzdoom\" -iwad \"doom2.wad\" -file \"Ñoño\\sigil.wad\""));

            Assert.Contains("Ñoño", BatchFileService.ReadBatch(path));
        }

        // The batch really runs in a hidden cmd, like a game launch, with "type" standing in for the source port.
        [Fact]
        public async Task ABatchWithAccentsAndPercentSigns_FindsItsFiles()
        {
            var folder = Directory.CreateDirectory(Path.Combine(_folder, "Ñoño 100%")).FullName;
            File.WriteAllText(Path.Combine(folder, "sigil.txt"), "ok");
            var path = Path.Combine(_folder, "run.bat");

            BatchFileService.WriteBatch(path, $"@type \"{BatchFileService.EscapePercent(Path.Combine(folder, "sigil.txt"))}\" >nul");

            Assert.Null(await BatchLaunchService.RunAsync(path));

            BatchFileService.WriteBatch(path, $"@type \"{BatchFileService.EscapePercent(Path.Combine(folder, "missing.txt"))}\" >nul");

            Assert.NotNull(await BatchLaunchService.RunAsync(path));
        }
    }
}
