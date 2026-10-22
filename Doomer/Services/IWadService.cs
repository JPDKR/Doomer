using System.Text;

namespace Doomer.Services
{
    public static class IWadService
    {
        // The IWADs the existing batches launch, exactly as each batch has them (e.g. D:\DSDA\doom.wad
        // or wads/doom2.wad), so picking one keeps its path. Batches that can't be read are skipped.
        public static List<string> FindUsedIWads(string? batchsDirectory, string batchExtension, string pluginsFolder)
        {
            if (!Directory.Exists(batchsDirectory))
                return [];

            var iwads = new List<string>();

            foreach (var file in Directory.EnumerateFiles(batchsDirectory, $"*{batchExtension}"))
            {
                string command;

                try
                {
                    command = File.ReadAllText(file);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    continue;
                }

                if (BatchFileService.TryParseCommand(command, pluginsFolder, out _, out var iwad, out _, out _, out _, out _) &&
                    !string.IsNullOrWhiteSpace(iwad))
                {
                    iwads.Add(BatchFileService.EnsureWadExtension(iwad));
                }
            }

            return iwads;
        }

        // The .wad files in the WADs folder (subfolders included) whose header marks them as an IWAD.
        public static List<string> FindIWadFiles(string? wadsDirectory)
        {
            if (!Directory.Exists(wadsDirectory))
                return [];

            var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };

            return [.. Directory.EnumerateFiles(wadsDirectory, "*.wad", options).Where(IsIWad)];
        }

        // A WAD file starts with "IWAD" or "PWAD"; only IWADs can be passed as -iwad.
        public static bool IsIWad(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                Span<byte> header = stackalloc byte[4];

                return stream.ReadAtLeast(header, 4, throwOnEndOfStream: false) == 4 &&
                       Encoding.ASCII.GetString(header) == "IWAD";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return false;
            }
        }

        // One entry per IWAD, ignoring case and the "/" or "\" used, sorted by file name and then by path.
        public static List<string> Merge(IEnumerable<string> iwads) =>
            [.. iwads
                .DistinctBy(i => i.Replace('/', '\\'), StringComparer.OrdinalIgnoreCase)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(i => i, StringComparer.OrdinalIgnoreCase)];
    }
}
