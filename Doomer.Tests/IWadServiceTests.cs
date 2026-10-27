using Doomer.Options;
using Doomer.Services;
using System.Text;

namespace Doomer.Tests
{
    public class IWadServiceTests : IDisposable
    {
        private readonly string _folder = Directory.CreateTempSubdirectory("Doomer iwads ").FullName;

        public void Dispose() => Directory.Delete(_folder, recursive: true);

        private string Write(string relativePath, string content)
        {
            var path = Path.Combine(_folder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content, Encoding.ASCII);
            return path;
        }

        [Fact]
        public void FindUsedIWads_KeepsEachBatchsPath()
        {
            Write("batchs\\Sigil.bat", "\"D:\\gzdoom\\gzdoom\" -iwad \"D:\\DSDA\\doom.wad\" -file \"wads/sigil.wad\"");
            Write("batchs\\Eviternity.bat", "\"E:\\DSDA\\dsda-doom\" \"d:\\DSDA\\plutonia.wad\" \"wads/eviternity.wad\"");
            Write("batchs\\Relative.bat", "\"D:\\gzdoom\\gzdoom\" -iwad \"wads/doom2\" -file \"wads/aaliens\"");
            Write("batchs\\Broken.bat", "echo not a launcher");
            Write("batchs\\notes.txt", "\"D:\\gzdoom\\gzdoom\" -iwad \"D:\\other\\tnt.wad\" -file \"x\"");

            var iwads = IWadService.FindUsedIWads(Path.Combine(_folder, "batchs"), ".bat", "plugins");

            Assert.Equal(["D:\\DSDA\\doom.wad", "d:\\DSDA\\plutonia.wad", "wads/doom2.wad"], iwads.Order());
        }

        [Fact]
        public void FindIWadFiles_ListsOnlyIWadsIncludingSubfolders()
        {
            var doom2 = Write("doom2.wad", "IWAD....");
            var tnt = Write("Final Doom\\tnt.WAD", "IWAD....");
            Write("mods\\aaliens.wad", "PWAD....");
            Write("tiny.wad", "IW");
            Write("notawad.pk3", "IWAD....");

            Assert.Equal([doom2, tnt], IWadService.FindIWadFiles(_folder).Order());
        }

        [Fact]
        public void FindersReturnNoneForMissingFolders()
        {
            var missing = Path.Combine(_folder, "missing");

            Assert.Empty(IWadService.FindUsedIWads(missing, ".bat", "plugins"));
            Assert.Empty(IWadService.FindIWadFiles(missing));
            Assert.Empty(IWadService.FindIWadFiles(null));
        }

        private static readonly List<NamedIWad> Named = IWadService.Configured(new IWadsSettings
        {
            Doom2 = " D:\\DSDA\\DOOM2.WAD ",
            Plutonia = "D:\\DSDA\\plutonia.wad"
        });

        [Fact]
        public void Configured_ListsOnlyTheGamesWithAPath()
        {
            Assert.Equal([new("Doom II", "D:\\DSDA\\DOOM2.WAD"), new("The Plutonia Experiment", "D:\\DSDA\\plutonia.wad")], Named);
            Assert.Equal("Doom II (D:\\DSDA\\DOOM2.WAD)", Named[0].ToString());
        }

        [Fact]
        public void ResolveAndDescribe_SwapANamedIWadForItsPath()
        {
            Assert.Equal("D:\\DSDA\\DOOM2.WAD", IWadService.Resolve("Doom II (D:\\DSDA\\DOOM2.WAD)", Named));
            Assert.Equal("wads/tnt.wad", IWadService.Resolve("wads/tnt.wad", Named));

            Assert.Equal("Doom II (D:\\DSDA\\DOOM2.WAD)", IWadService.Describe("d:/dsda/doom2.wad", Named));
            Assert.Equal("D:\\DSDA\\tnt.wad", IWadService.Describe("D:\\DSDA\\tnt.wad", Named));
        }

        [Fact]
        public void ExceptNamed_DropsTheIWadsListedByName()
        {
            Assert.Equal(["D:\\DSDA\\doom.wad"], IWadService.ExceptNamed(["D:\\DSDA\\doom.wad", "d:/DSDA/Doom2.wad"], Named));
        }

        [Fact]
        public void Merge_DropsDuplicatesAndSortsByFileName()
        {
            var merged = IWadService.Merge([
                "D:\\DSDA\\tnt.wad",
                "D:\\DSDA\\doom.wad",
                "d:\\dsda\\DOOM.wad",
                "D:/DSDA/doom.wad",
                "wads/doom2.wad",
                "C:\\iwads\\doom.wad"]);

            Assert.Equal(["C:\\iwads\\doom.wad", "D:\\DSDA\\doom.wad", "wads/doom2.wad", "D:\\DSDA\\tnt.wad"], merged);
        }
    }
}
