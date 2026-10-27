using Doomer.Options;
using System.Text;

namespace Doomer.Services
{
    // An IWAD set up in the settings, listed by its game's name with the path it's at.
    public record NamedIWad(string Name, string Path)
    {
        public override string ToString() => $"{Name} ({Path})";
    }

    public static class IWadService
    {
        // The games' IWADs that have a path in the settings, in release order.
        public static List<NamedIWad> Configured(IWadsSettings settings) =>
            [.. new NamedIWad[]
                {
                    new("DOOM", settings.Doom),
                    new("Doom II", settings.Doom2),
                    new("TNT", settings.Tnt),
                    new("The Plutonia Experiment", settings.Plutonia)
                }
                .Where(i => !string.IsNullOrWhiteSpace(i.Path))
                .Select(i => i with { Path = i.Path.Trim() })];

        // The path to launch for what the IWAD box shows: a named IWAD's path, or the text as typed.
        public static string Resolve(string text, IEnumerable<NamedIWad> named) =>
            named.FirstOrDefault(i => i.ToString() == text.Trim())?.Path ?? text;

        // How the IWAD box shows a batch's IWAD: by name when it's one of the named ones.
        public static string Describe(string iwad, IEnumerable<NamedIWad> named) =>
            named.FirstOrDefault(i => SamePath(i.Path, iwad))?.ToString() ?? iwad;

        // The found IWADs that aren't already listed by name.
        public static List<string> ExceptNamed(IEnumerable<string> iwads, IEnumerable<NamedIWad> named) =>
            [.. iwads.Where(i => !named.Any(n => SamePath(n.Path, i)))];

        private static bool SamePath(string a, string b) =>
            string.Equals(a.Replace('/', '\\'), b.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase);

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
