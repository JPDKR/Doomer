namespace Doomer.Services
{
    // The app's own files in the temp folder: downloaded and pasted images, until they're moved into the
    // images folder. Their names follow these patterns, so the ones left behind can be found and removed.
    public static class TempFileService
    {
        private const string DownloadPattern = "Doomer-*.download";
        private const string PastedImagePattern = "Doomer-pasted-*.png";

        // Downloads have no image extension on purpose (see ImageDownloadService.DownloadAsync).
        public static string NewDownloadPath() => NewPath(DownloadPattern);

        public static string NewPastedImagePath() => NewPath(PastedImagePattern);

        private static string NewPath(string pattern) =>
            Path.Combine(Path.GetTempPath(), pattern.Replace("*", Guid.NewGuid().ToString("N")));

        // Nothing to do when it's already gone, e.g. moved into the images folder.
        public static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }

        // Removes the files a crash or a failed save left behind. Only the ones older than maxAge,
        // since a newer one may still be in use by a window of another running copy of the app.
        public static void DeleteLeftovers(string folder, TimeSpan maxAge, DateTime utcNow)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(folder, DownloadPattern)
                             .Concat(Directory.EnumerateFiles(folder, PastedImagePattern)))
                {
                    if (utcNow - File.GetLastWriteTimeUtc(file) > maxAge)
                        TryDelete(file);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }
    }
}
