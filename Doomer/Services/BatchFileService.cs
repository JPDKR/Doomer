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

        // ZDoom-format packages, which DSDA Doom can't load.
        private static readonly string[] GZDoomOnlyExtensions = [".pk3", ".pk7"];

        // "<exe>" -iwad "<iwad>" -file "<wad>" ["<extra file>"...] ["<plugin>"]
        private static readonly Regex GZDoomPattern = new(
            "-iwad \"(?<iwad>[^\"]*)\" -file \"(?<wad>[^\"]*)\"(?: \"(?<file>[^\"]*)\")*",
            RegexOptions.Compiled);

        // <exe> "<iwad>" "<wad>" ["<extra file>"...] — the executable may or may not be quoted.
        private static readonly Regex DSDADoomPattern = new(
            "^\\s*(?:\"[^\"]*\"|\\S+)\\s+\"(?<iwad>[^\"]*)\"\\s+\"(?<wad>[^\"]*)\"(?:\\s+\"(?<file>[^\"]*)\")*",
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

        // Whether the batch loads a .pk3/.pk7, so only GZDoom can launch it.
        public static bool RequiresGZDoom(string wad, IEnumerable<string> extraFiles) =>
            extraFiles.Prepend(wad).Any(f =>
                GZDoomOnlyExtensions.Contains(Path.GetExtension(f.Trim()), StringComparer.OrdinalIgnoreCase));

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

        // A batch's icon is the image named exactly like the batch file. Both the main window and the
        // batch form go through here, so a saved image is always the one the button looks for.
        public static string GetImagePath(string batchPath, ImageSettings images) =>
            Path.Combine(images.Location, Path.GetFileNameWithoutExtension(batchPath) + images.Extension);

        public static bool IsValidFileName(string fileName, out string error)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                error = Strings.FileNameEmpty;
                return false;
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                error = Strings.FileNameInvalidChars;
                return false;
            }

            error = string.Empty;
            return true;
        }

        // Extra files (DEH/BEX patches, other WADs...) are loaded right after the WAD, before the plugin.
        public static string BuildCommand(GZDoomSettings settings, string iwad, string wad, string plugins, IReadOnlyList<string>? extraFiles = null)
        {
            extraFiles ??= [];

            if (ContainsQuote(settings.Location) || ContainsQuote(settings.Plugins) ||
                ContainsQuote(iwad) || ContainsQuote(wad) || ContainsQuote(plugins) || extraFiles.Any(ContainsQuote))
                throw new ArgumentException(Strings.GZDoomPathsWithQuotes);

            var command = $"\"{settings.Location}\" -iwad \"{EnsureWadExtension(iwad)}\" -file \"{EnsureWadExtension(wad)}\"{QuoteFiles(extraFiles)}";

            if (!string.IsNullOrWhiteSpace(plugins))
                command += $" \"{settings.Plugins}/{plugins}\"";

            return command;
        }

        // DSDA Doom tells loose files apart by extension, so DEH/BEX patches can be passed like WADs.
        public static string BuildCommand(DSDADoomSettings settings, string iwad, string wad, IReadOnlyList<string>? extraFiles = null)
        {
            extraFiles ??= [];

            if (ContainsQuote(settings.Location) || ContainsQuote(iwad) || ContainsQuote(wad) || extraFiles.Any(ContainsQuote))
                throw new ArgumentException(Strings.DSDADoomPathsWithQuotes);

            return $"\"{settings.Location}\" \"{EnsureWadExtension(iwad)}\" \"{EnsureWadExtension(wad)}\"{QuoteFiles(extraFiles)}";
        }

        private static string QuoteFiles(IReadOnlyList<string> files) =>
            string.Concat(files.Select(f => $" \"{EnsureWadExtension(f)}\""));

        // pluginsFolder tells the plugin apart from the extra files: it's the last path, under that folder.
        public static bool TryParseCommand(string command, string pluginsFolder, out SourcePort port, out string iwad,
            out string wad, out List<string> extraFiles, out string plugins)
        {
            var match = GZDoomPattern.Match(command);

            if (match.Success)
            {
                port = SourcePort.GZDoom;
                iwad = StripWadExtension(match.Groups["iwad"].Value);
                wad = StripWadExtension(match.Groups["wad"].Value);
                extraFiles = [.. match.Groups["file"].Captures.Select(c => c.Value)];
                plugins = string.Empty;

                if (extraFiles.Count > 0 && extraFiles[^1].StartsWith(pluginsFolder + "/", StringComparison.OrdinalIgnoreCase))
                {
                    plugins = Path.GetFileName(extraFiles[^1]);
                    extraFiles.RemoveAt(extraFiles.Count - 1);
                }

                return true;
            }

            match = DSDADoomPattern.Match(command);

            if (match.Success)
            {
                port = SourcePort.DSDADoom;
                iwad = StripWadExtension(match.Groups["iwad"].Value);
                wad = StripWadExtension(match.Groups["wad"].Value);
                extraFiles = [.. match.Groups["file"].Captures.Select(c => c.Value)];
                plugins = string.Empty;
                return true;
            }

            port = SourcePort.GZDoom;
            iwad = wad = plugins = string.Empty;
            extraFiles = [];
            return false;
        }

        private static bool ContainsQuote(string value) => value.Contains('"');
    }
}
