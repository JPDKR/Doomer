using Doomer.Options;
using Doomer.Services;

namespace Doomer.Tests
{
    public class WadMoveServiceTests : IDisposable
    {
        private readonly string _root = Directory.CreateTempSubdirectory("Doomer moves ").FullName;

        public void Dispose() => Directory.Delete(_root, recursive: true);

        private string Wads => Path.Combine(_root, "wads");

        private string CreateFile(string relativePath)
        {
            var path = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, relativePath);
            return path;
        }

        [Fact]
        public void Plan_MovesTheWadAndTheExtrasInItsFolder()
        {
            var wad = CreateFile("Downloads\\sunlust\\sunlust.wad");
            var deh = CreateFile("Downloads\\sunlust\\sunlust.deh");
            var music = CreateFile("Downloads\\sunlust\\music\\sunmusic.wad");
            Directory.CreateDirectory(Wads);

            var moves = WadMoveService.Plan(wad[..^4], [deh, music[..^4]], Wads);

            Assert.Equal(
            [
                new FileMove(wad, Path.Combine(Wads, "sunlust.wad")),
                new FileMove(deh, Path.Combine(Wads, "sunlust.deh")),
                new FileMove(music, Path.Combine(Wads, "music\\sunmusic.wad"))
            ], moves);
        }

        [Fact]
        public void Plan_LeavesExtrasOutsideTheWadsFolderOrAlreadyInTheWadsDirectory()
        {
            var wad = CreateFile("Downloads\\sunlust\\sunlust.wad");
            var shared = CreateFile("Music\\shared.wad");
            var inWads = CreateFile("wads\\sunlust.deh");

            var moves = WadMoveService.Plan(wad, [shared, inWads, "wads/relative.deh", Path.Combine(_root, "Downloads\\sunlust\\missing.deh")], Wads);

            Assert.Equal([new FileMove(wad, Path.Combine(Wads, "sunlust.wad"))], moves);
        }

        [Fact]
        public void Plan_MovesNothingWhenTheWadIsAlreadyInTheWadsDirectory()
        {
            var wad = CreateFile("wads\\Sunlust\\sunlust.wad");
            var deh = CreateFile("wads\\Sunlust\\sunlust.deh");

            Assert.Empty(WadMoveService.Plan(wad, [deh], Wads));
            Assert.Empty(WadMoveService.Plan("wads/sunlust", [], Wads));
            Assert.Empty(WadMoveService.Plan(Path.Combine(_root, "missing.wad"), [], Wads));
        }

        [Fact]
        public void GetDestination_FindsMovedPathsWithOrWithoutTheWadExtension()
        {
            List<FileMove> moves = [new("C:\\Downloads\\sigil.wad", "D:\\wads\\sigil.wad")];

            Assert.Equal("D:\\wads\\sigil.wad", WadMoveService.GetDestination("C:\\Downloads\\sigil", moves));
            Assert.Equal("D:\\wads\\sigil.wad", WadMoveService.GetDestination("c:\\downloads\\SIGIL.wad", moves));
            Assert.Null(WadMoveService.GetDestination("C:\\Downloads\\other.wad", moves));
            Assert.Null(WadMoveService.GetDestination("Downloads/sigil", moves));
        }

        [Fact]
        public void UpdateCommand_PointsMovedPathsToTheirNewPlace()
        {
            List<FileMove> moves =
            [
                new("C:\\Downloads\\sigil.wad", "D:\\wads\\sigil.wad"),
                new("D:\\gzdoom\\old\\sigil.deh", "D:\\wads\\sigil.deh")
            ];
            const string command = "rem Doomer plugins folder: plugins\r\n" +
                "\"D:\\gzdoom\\gzdoom\" -iwad \"doom.wad\" -file \"C:\\Downloads\\sigil\" \"old/sigil.deh\" \"plugins/smoothed.wad\" -skill 4\r\n";

            Assert.Equal("rem Doomer plugins folder: plugins\r\n" +
                "\"D:\\gzdoom\\gzdoom\" -iwad \"doom.wad\" -file \"D:\\wads\\sigil.wad\" \"D:\\wads\\sigil.deh\" \"plugins/smoothed.wad\" -skill 4\r\n",
                WadMoveService.UpdateCommand(command, "D:\\gzdoom\\gzdoom", moves));
        }

        [Fact]
        public void UpdateCommand_ReturnsNullWhenNothingMoved()
        {
            List<FileMove> moves = [new("C:\\Downloads\\sigil.wad", "D:\\wads\\sigil.wad")];

            Assert.Null(WadMoveService.UpdateCommand("\"E:\\dsda\\dsda-doom\" \"doom.wad\" \"sigil.wad\"", "E:\\dsda\\dsda-doom", moves));
            // Without an executable, relative paths can't be told apart from the moved ones.
            Assert.Null(WadMoveService.UpdateCommand("\"gzdoom\" -iwad \"doom.wad\" -file \"sigil.wad\"", null, moves));
        }

        [Fact]
        public void ApplyAndUndo_MoveTheFilesAndBack()
        {
            var wad = CreateFile("Downloads\\sigil.wad");
            var deh = CreateFile("Downloads\\extra\\sigil.deh");
            List<FileMove> moves = [new(wad, Path.Combine(Wads, "sigil.wad")), new(deh, Path.Combine(Wads, "extra\\sigil.deh"))];

            WadMoveService.Apply(moves);

            Assert.True(moves.All(m => File.Exists(m.Destination) && !File.Exists(m.Source)));

            WadMoveService.Undo(moves);

            Assert.True(moves.All(m => File.Exists(m.Source) && !File.Exists(m.Destination)));
        }

        [Fact]
        public void Apply_PutsTheMovedFilesBackWhenOneFails()
        {
            var wad = CreateFile("Downloads\\sigil.wad");
            var missing = Path.Combine(_root, "Downloads\\missing.deh");
            List<FileMove> moves = [new(wad, Path.Combine(Wads, "sigil.wad")), new(missing, Path.Combine(Wads, "missing.deh"))];

            Assert.Throws<FileNotFoundException>(() => WadMoveService.Apply(moves));
            Assert.True(File.Exists(wad));
            Assert.False(File.Exists(Path.Combine(Wads, "sigil.wad")));
        }

        [Fact]
        public void UpdateBatches_UpdatesTheOtherBatchesThatLoadAMovedFile()
        {
            var batches = new BatchSettings { Path = Path.Combine(_root, "batchs"), Extension = ".bat" };
            var ports = new PortsSettings
            {
                GZDoom = new GZDoomSettings { Path = Path.Combine(_root, "gzdoom\\gzdoom") },
                DSDADoom = new DSDADoomSettings { Path = Path.Combine(_root, "dsda\\dsda-doom") }
            };
            var source = Path.Combine(_root, "dsda\\sigil.wad");
            List<FileMove> moves = [new(source, Path.Combine(Wads, "sigil.wad"))];

            var saved = CreateFile("batchs\\Sigil.bat");
            var relative = CreateFile("batchs\\Sigil DSDA.bat");
            File.WriteAllText(relative, $"\"{ports.DSDADoom.Path}\" \"doom.wad\" \"sigil.wad\" -complevel 3");
            var rooted = CreateFile("batchs\\Sigil GZDoom.bat");
            File.WriteAllText(rooted, $"\"{ports.GZDoom.Path}\" -iwad \"doom.wad\" -file \"{source}\"");
            var other = CreateFile("batchs\\Other.bat");
            File.WriteAllText(other, $"\"{ports.GZDoom.Path}\" -iwad \"doom.wad\" -file \"wads/other.wad\"");
            File.WriteAllText(saved, $"\"{ports.GZDoom.Path}\" -iwad \"doom.wad\" -file \"{source}\"");

            var result = WadMoveService.UpdateBatches(batches, ports, saved, moves);

            Assert.Equal(["Sigil DSDA", "Sigil GZDoom"], result.Updated.Order());
            Assert.Empty(result.Failed);
            Assert.Equal($"\"{ports.DSDADoom.Path}\" \"doom.wad\" \"{Path.Combine(Wads, "sigil.wad")}\" -complevel 3", File.ReadAllText(relative));
            Assert.Contains(Path.Combine(Wads, "sigil.wad"), File.ReadAllText(rooted));
            Assert.Contains(source, File.ReadAllText(saved));
            Assert.Contains("wads/other.wad", File.ReadAllText(other));
        }
    }
}
