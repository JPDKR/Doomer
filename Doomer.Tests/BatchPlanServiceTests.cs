using Doomer.Options;
using Doomer.Services;
using System.Drawing;
using System.Drawing.Imaging;
using System.Resources;

namespace Doomer.Tests
{
    public class BatchPlanServiceTests : IDisposable
    {
        private readonly string _root = Directory.CreateTempSubdirectory("Doomer plan ").FullName;
        private readonly PortsSettings _ports;
        private readonly FoldersSettings _folders;

        public BatchPlanServiceTests()
        {
            _ports = new PortsSettings
            {
                GZDoom = new GZDoomSettings { Path = Path.Combine(_root, "gzdoom\\gzdoom"), Plugins = "plugins" },
                DSDADoom = new DSDADoomSettings { Path = Path.Combine(_root, "dsda\\dsda-doom") }
            };
            _folders = new FoldersSettings
            {
                Batches = new BatchSettings { Path = Path.Combine(_root, "batchs"), Extension = ".bat" },
                Images = new ImageSettings { Path = Path.Combine(_root, "pics"), Extension = ".png" },
                Wads = new WadSettings { Path = Path.Combine(_root, "wads") }
            };

            Directory.CreateDirectory(_folders.Batches.Path);
            Directory.CreateDirectory(_folders.Images.Path);
            Directory.CreateDirectory(_folders.Wads.Path);
            Directory.CreateDirectory(Path.Combine(_root, "gzdoom\\plugins"));
        }

        public void Dispose() => Directory.Delete(_root, recursive: true);

        // The app's texts are internal, so they're read the way LocalizationTests does.
        private static readonly ResourceManager Resources = new("Doomer.Strings", typeof(BatchFileService).Assembly);

        private static string Text(string name, params object[] args) =>
            string.Format(Resources.GetString(name)!, args);

        private string CreateFile(string relativePath)
        {
            var path = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, relativePath);
            return path;
        }

        private string CreateImage(string relativePath)
        {
            var path = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var image = new Bitmap(4, 4);
            image.Save(path, ImageFormat.Png);
            return path;
        }

        private static BatchDraft Draft(string fileName = "Sigil", string iwad = "doom", string wad = "wads/sigil",
            SourcePort port = SourcePort.GZDoom, IReadOnlyList<string>? extraFiles = null, IReadOnlyList<string>? plugins = null,
            string image = "", string? editingPath = null, string? existingImagePath = null) =>
            new(port, fileName, iwad, wad, extraFiles ?? [], plugins ?? [], "", image, editingPath, existingImagePath);

        private bool TryPlan(BatchDraft draft, out BatchPlan? plan, out string error) =>
            BatchPlanService.TryPlan(draft, _ports, _folders, out plan, out error);

        [Fact]
        public void TryPlan_WorksOutANewBatch()
        {
            Assert.True(TryPlan(Draft(fileName: "  Sigil "), out var plan, out _));

            Assert.Equal(Path.Combine(_folders.Batches.Path, "Sigil.bat"), plan!.Path);
            Assert.False(plan.Renaming);
            Assert.Equal($"\"{_ports.GZDoom.Path}\" -iwad \"doom.wad\" -file \"wads/sigil.wad\"", plan.Command);
            Assert.Empty(plan.Moves);
            Assert.Empty(plan.Confirmations);
            Assert.Null(plan.ImageDestination);
        }

        [Theory]
        [InlineData("", "doom", "sigil")]
        [InlineData("Sigil", " ", "sigil")]
        [InlineData("Sigil", "doom", "")]
        public void TryPlan_RequiresNameIWadAndWad(string fileName, string iwad, string wad)
        {
            Assert.False(TryPlan(Draft(fileName, iwad, wad), out var plan, out var error));
            Assert.Null(plan);
            Assert.Equal(Text("MissingRequiredFields"), error);
        }

        [Fact]
        public void TryPlan_RequiresTheSourcePortsExecutable()
        {
            _ports.DSDADoom.Path = "";

            Assert.False(TryPlan(Draft(port: SourcePort.DSDADoom), out _, out var error));
            Assert.Equal(Text("ExecutableNotConfigured", "DSDA Doom"), error);
        }

        [Fact]
        public void TryPlan_RejectsAnInvalidFileName()
        {
            Assert.False(TryPlan(Draft(fileName: "evil*name"), out _, out var error));
            Assert.Equal(Text("FileNameInvalidChars"), error);
        }

        [Fact]
        public void TryPlan_RejectsPackagesForDSDADoom()
        {
            Assert.False(TryPlan(Draft(port: SourcePort.DSDADoom, wad: "mods/myhouse.pk3"), out _, out var error));
            Assert.Equal(Text("DSDADoomNoPackages"), error);
        }

        [Fact]
        public void TryPlan_LeavesPluginsOutForDSDADoom()
        {
            Assert.True(TryPlan(Draft(port: SourcePort.DSDADoom, plugins: ["smoothed.wad"]), out var plan, out _));
            Assert.DoesNotContain("smoothed", plan!.Command);
            Assert.Empty(plan.Confirmations);
        }

        [Fact]
        public void TryPlan_AsksBeforeOverwritingAnotherBatch()
        {
            CreateFile("batchs\\Sigil.bat");

            Assert.True(TryPlan(Draft(), out var plan, out _));
            Assert.Equal([Text("BatchExistsOverwrite", "Sigil")], plan!.Confirmations);
        }

        [Fact]
        public void TryPlan_Rename_NeverTakesAnotherBatchsName()
        {
            var editing = CreateFile("batchs\\Sigil.bat");
            CreateFile("batchs\\Sigil II.bat");

            Assert.False(TryPlan(Draft(fileName: "Sigil II", editingPath: editing), out _, out var error));
            Assert.Equal(Text("BatchNameTaken", "Sigil II"), error);
        }

        [Theory]
        [InlineData("Sigil", false)]
        [InlineData("sigil", true)]
        [InlineData("Sigil 1993", true)]
        public void TryPlan_Rename_KeepsTheEditedBatchsFolderAndExtension(string fileName, bool renaming)
        {
            var editing = CreateFile("other\\Sigil.cmd");

            Assert.True(TryPlan(Draft(fileName: fileName, editingPath: editing), out var plan, out _));
            Assert.Equal(Path.Combine(_root, "other", fileName + ".cmd"), plan!.Path);
            Assert.Equal(renaming, plan.Renaming);
            Assert.Empty(plan.Confirmations);
        }

        [Fact]
        public void TryPlan_AsksAboutEveryMissingFullPath()
        {
            var iwad = Path.Combine(_root, "iwads\\doom");
            var deh = Path.Combine(_root, "missing\\sigil.deh");

            Assert.True(TryPlan(Draft(iwad: iwad, extraFiles: [deh, "relative/music"], plugins: ["smoothed"]), out var plan, out _));
            Assert.Equal(
            [
                Text("FileNotFoundCreateAnyway", "IWAD", iwad + ".wad"),
                Text("FileNotFoundCreateAnyway", Text("ExtraFileLabel"), deh),
                Text("FileNotFoundCreateAnyway", Text("PluginFileLabel"),
                    Path.Combine(_root, "gzdoom\\plugins\\smoothed.wad"))
            ], plan!.Confirmations);
        }

        [Fact]
        public void TryPlan_PointsTheBatchToTheMovedWadAndAsksBeforeReplacing()
        {
            var wad = CreateFile("Downloads\\sigil.wad");
            var deh = CreateFile("Downloads\\sigil.deh");
            CreateFile("wads\\sigil.deh");

            Assert.True(TryPlan(Draft(wad: wad, extraFiles: [deh]), out var plan, out _));
            Assert.Equal(2, plan!.Moves.Count);
            Assert.EndsWith($"-file \"{Path.Combine(_folders.Wads.Path, "sigil.wad")}\" \"{Path.Combine(_folders.Wads.Path, "sigil.deh")}\"",
                plan.Command);
            Assert.Equal([Text("WadExistsReplace", "sigil.deh")], plan.Confirmations);
        }

        [Fact]
        public void TryPlan_NeedsTheWadsFolderToMoveAWadIntoIt()
        {
            var wad = CreateFile("Downloads\\sigil.wad");
            Directory.Delete(_folders.Wads.Path);

            Assert.False(TryPlan(Draft(wad: wad), out _, out var error));
            Assert.Equal(Text("WadsDirNotFoundFixInSettings", _folders.Wads.Path), error);
        }

        [Fact]
        public void TryPlan_RejectsPathsWithQuotes()
        {
            Assert.False(TryPlan(Draft(wad: "evil\" -exec calc"), out _, out var error));
            Assert.Equal(Text("GZDoomPathsWithQuotes"), error);
        }

        [Fact]
        public void TryPlan_NamesTheNewImageAfterTheBatch()
        {
            var image = CreateImage("Downloads\\cover.png");

            Assert.True(TryPlan(Draft(image: image), out var plan, out _));
            Assert.Equal(image, plan!.ImageSource);
            Assert.Equal(Path.Combine(_folders.Images.Path, "Sigil.png"), plan.ImageDestination);
        }

        [Fact]
        public void TryPlan_KeepsTheCurrentImageLeftInTheBox()
        {
            var editing = CreateFile("batchs\\Sigil.bat");
            var current = CreateImage("pics\\Sigil.png");

            Assert.True(TryPlan(Draft(image: current, editingPath: editing, existingImagePath: current), out var plan, out _));
            Assert.Empty(plan!.ImageSource);
            Assert.Null(plan.ImageDestination);
        }

        [Fact]
        public void TryPlan_LeavesAnImageUrlToBeDownloadedLater()
        {
            Assert.True(TryPlan(Draft(image: "https://example.com/sigil.png"), out var plan, out _));
            Assert.Equal("https://example.com/sigil.png", plan!.ImageSource);
            Assert.Equal(Path.Combine(_folders.Images.Path, "Sigil.png"), plan.ImageDestination);
        }

        [Fact]
        public void TryPlan_RejectsAMissingOrInvalidImage()
        {
            var missing = Path.Combine(_root, "missing.png");
            var notAnImage = CreateFile("Downloads\\readme.png");

            Assert.False(TryPlan(Draft(image: missing), out _, out var error));
            Assert.Equal(Text("ImageFileNotFound", missing), error);

            Assert.False(TryPlan(Draft(image: notAnImage), out _, out error));
            Assert.Equal(Text("ImageFileInvalid", notAnImage), error);
        }

        [Fact]
        public void TryPlan_NeedsTheImagesFolderForANewImage()
        {
            _folders.Images.Path = "";

            Assert.False(TryPlan(Draft(image: "https://example.com/sigil.png"), out _, out var error));
            Assert.Equal(Text("ImagesDirNotFoundFixInSettings", ""), error);
        }

        // Nothing is asked when the batch can't be saved anyway.
        [Fact]
        public void TryPlan_ChecksEverythingBeforeAskingAnything()
        {
            CreateFile("batchs\\Sigil.bat");

            Assert.False(TryPlan(Draft(iwad: Path.Combine(_root, "missing\\doom"), image: Path.Combine(_root, "missing.png")),
                out var plan, out _));
            Assert.Null(plan);
        }
    }
}
