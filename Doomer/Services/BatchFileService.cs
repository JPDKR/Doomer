using Doomer.Options;
using System.Text.RegularExpressions;

namespace Doomer.Services
{
    public enum SourcePort
    {
        GZDoom,
        DSDADoom
    }

    public static class BatchFileService
    {
        private const string WadExtension = ".wad";

        // "<exe>" -iwad "<iwad>" -file "<wad>" ["<plugin>"]
        private static readonly Regex GZDoomPattern = new(
            "-iwad \"(?<iwad>[^\"]*)\" -file \"(?<wad>[^\"]*)\"(?: \"(?<pluginsPath>[^\"]*)\")?",
            RegexOptions.Compiled);

        // <exe> "<iwad>" "<wad>" — the executable may or may not be quoted.
        private static readonly Regex DSDADoomPattern = new(
            "^\\s*(?:\"[^\"]*\"|\\S+)\\s+\"(?<iwad>[^\"]*)\"\\s+\"(?<wad>[^\"]*)\"",
            RegexOptions.Compiled);

        public static string GetDisplayName(SourcePort port) => port switch
        {
            SourcePort.DSDADoom => "DSDA Doom",
            _ => "GZDoom"
        };

        // Appends ".wad" only when the path has no extension, so .pk3/.zip/etc. are kept as-is.
        public static string EnsureWadExtension(string path) =>
            Path.HasExtension(path) ? path : path + WadExtension;

        public static string StripWadExtension(string path) =>
            path.EndsWith(WadExtension, StringComparison.OrdinalIgnoreCase) ? path[..^WadExtension.Length] : path;

        // Where a WAD should be moved inside the WADs directory, or null when it's already in it
        // (subfolders included) or isn't an absolute path we can locate.
        public static string? GetWadDestination(string wadFile, string wadsDirectory)
        {
            if (!Path.IsPathRooted(wadFile) || string.IsNullOrWhiteSpace(wadsDirectory))
                return null;

            var source = Path.GetFullPath(wadFile);
            var directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(wadsDirectory)) + Path.DirectorySeparatorChar;

            return source.StartsWith(directory, StringComparison.OrdinalIgnoreCase)
                ? null
                : Path.Combine(directory, Path.GetFileName(source));
        }

        public static bool IsValidFileName(string fileName, out string error)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                error = "File name cannot be empty.";
                return false;
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                error = "File name contains invalid characters.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static string BuildCommand(GZDoomSettings settings, string iwad, string wad, string plugins)
        {
            if (ContainsQuote(settings.Location) || ContainsQuote(settings.Plugins) ||
                ContainsQuote(iwad) || ContainsQuote(wad) || ContainsQuote(plugins))
                throw new ArgumentException("GZDoom location, plugins folder, IWAD, WAD and plugin paths cannot contain quote characters.");

            var command = $"\"{settings.Location}\" -iwad \"{EnsureWadExtension(iwad)}\" -file \"{EnsureWadExtension(wad)}\"";

            if (!string.IsNullOrWhiteSpace(plugins))
                command += $" \"{settings.Plugins}/{plugins}\"";

            return command;
        }

        public static string BuildCommand(DSDADoomSettings settings, string iwad, string wad)
        {
            if (ContainsQuote(settings.Location) || ContainsQuote(iwad) || ContainsQuote(wad))
                throw new ArgumentException("DSDA Doom location, IWAD and WAD paths cannot contain quote characters.");

            return $"\"{settings.Location}\" \"{EnsureWadExtension(iwad)}\" \"{EnsureWadExtension(wad)}\"";
        }

        public static bool TryParseCommand(string command, out SourcePort port, out string iwad, out string wad, out string plugins)
        {
            var match = GZDoomPattern.Match(command);

            if (match.Success)
            {
                port = SourcePort.GZDoom;
                iwad = StripWadExtension(match.Groups["iwad"].Value);
                wad = StripWadExtension(match.Groups["wad"].Value);
                plugins = match.Groups["pluginsPath"].Success
                    ? Path.GetFileName(match.Groups["pluginsPath"].Value)
                    : string.Empty;
                return true;
            }

            match = DSDADoomPattern.Match(command);

            if (match.Success)
            {
                port = SourcePort.DSDADoom;
                iwad = StripWadExtension(match.Groups["iwad"].Value);
                wad = StripWadExtension(match.Groups["wad"].Value);
                plugins = string.Empty;
                return true;
            }

            port = SourcePort.GZDoom;
            iwad = wad = plugins = string.Empty;
            return false;
        }

        private static bool ContainsQuote(string value) => value.Contains('"');
    }
}
