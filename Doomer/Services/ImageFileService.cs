using System.Drawing.Imaging;

namespace Doomer.Services
{
    public static class ImageFileService
    {
        public static string FileFilter => $"{Strings.FileFilterImages}|*.png;*.jpg;*.jpeg;*.bmp;*.gif|{Strings.FileFilterAll}|*.*";

        public static bool IsValidImage(string path)
        {
            try
            {
                using var _ = Image.FromFile(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Puts the image in the images folder named after the WAD, replacing the one there. If its format
        // differs from the configured extension, it's re-encoded so the icon loads correctly. A moved
        // image is gone from where it was; a copied one (e.g. picked from the user's own files) stays.
        public static void Save(string source, string destination, bool move)
        {
            if (string.Equals(Path.GetFullPath(source), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
                return;

            var sourceExt = Path.GetExtension(source);
            var destExt = Path.GetExtension(destination);

            if (string.Equals(sourceExt, destExt, StringComparison.OrdinalIgnoreCase))
            {
                if (move)
                    File.Move(source, destination, overwrite: true);
                else
                    File.Copy(source, destination, overwrite: true);
                return;
            }

            using (var img = Image.FromFile(source))
                img.Save(destination, GetImageFormat(destExt));

            if (move)
                File.Delete(source);
        }

        private static ImageFormat GetImageFormat(string extension) => extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            ".gif" => ImageFormat.Gif,
            _ => ImageFormat.Png
        };
    }
}
