using Doomer.Services;
using System.Drawing;
using System.Drawing.Imaging;

namespace Doomer.Tests
{
    public class ImageFileServiceTests : IDisposable
    {
        private readonly string _folder = Directory.CreateTempSubdirectory("Doomer images ").FullName;

        public void Dispose() => Directory.Delete(_folder, recursive: true);

        private string CreateImage(string name)
        {
            var path = Path.Combine(_folder, name);
            using var bmp = new Bitmap(4, 4);
            bmp.Save(path, path.EndsWith(".jpg") ? ImageFormat.Jpeg : ImageFormat.Png);
            return path;
        }

        [Fact]
        public void Save_Copy_KeepsTheSourceAndReplacesTheDestination()
        {
            var source = CreateImage("new.png");
            var destination = Path.Combine(_folder, "Sigil.png");
            File.WriteAllText(destination, "old icon");

            ImageFileService.Save(source, destination, move: false);

            Assert.True(File.Exists(source));
            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(destination));
        }

        [Fact]
        public void Save_Move_ReencodesToTheDestinationsFormat()
        {
            var source = CreateImage("picked.jpg");
            var destination = Path.Combine(_folder, "Sigil.png");

            ImageFileService.Save(source, destination, move: true);

            Assert.False(File.Exists(source));
            using var saved = Image.FromFile(destination);
            Assert.Equal(ImageFormat.Png, saved.RawFormat);
        }

        [Fact]
        public void IsValidImage_RejectsNonImages()
        {
            var text = Path.Combine(_folder, "notes.png");
            File.WriteAllText(text, "not an image");

            Assert.True(ImageFileService.IsValidImage(CreateImage("ok.png")));
            Assert.False(ImageFileService.IsValidImage(text));
        }
    }
}
