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

        // The same kinds of files the WAD pickers offer.
        private static readonly string[] PluginExtensions = [".wad", ".pk3", ".pk7", ".zip", ".deh", ".bex"];

        // "<exe>" -iwad "<iwad>" -file "<wad>" ["<extra file>"...] ["<plugin>"]
        private static readonly Regex GZDoomPattern = new(
            "-iwad \"(?<iwad>[^\"]*)\" -file \"(?<wad>[^\"]*)\"(?: \"(?<file>[^\"]*)\")*",
            RegexOptions.Compiled);

        // <exe> "<iwad>" "<wad>" ["<extra file>"...] — the executable may or may not be quoted.
        private static readonly Regex DSDADoomPattern = new(
            "^\\s*(?:\"[^\"]*\"|\\S+)\\s+\"(?<iwad>[^\"]*)\"\\s+\"(?<wad>[^\"]*)\"(?:\\s+\"(?<file>[^\"]*)\")*",
            RegexOptions.Compiled);

        private static readonly Regex DuplicateNamePattern = new("^(?<base>.+) \\((?<n>\\d{1,6})\\)$", RegexOptions.Compiled);

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

        // GZDoom also looks for relative -file paths in its own folder, so a relative plugins folder
        // lives in the GZDoom install. Null when there's no plugins folder or GZDoom location to go by.
        public static string? GetPluginsDirectory(GZDoomSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Plugins))
                return null;

            if (Path.IsPathRooted(settings.Plugins))
                return settings.Plugins;

            if (string.IsNullOrWhiteSpace(settings.Location) || !Path.IsPathRooted(settings.Location))
                return null;

            return Path.Combine(Path.GetDirectoryName(settings.Location)!, settings.Plugins);
        }

        // The file names of the plugins in the plugins folder, or none if it doesn't exist.
        public static List<string> FindPlugins(string? pluginsDirectory)
        {
            if (!Directory.Exists(pluginsDirectory))
                return [];

            return [.. Directory.EnumerateFiles(pluginsDirectory)
                .Where(f => PluginExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .Select(f => Path.GetFileName(f))
                .Order(StringComparer.OrdinalIgnoreCase)];
        }

        // Where a batch's WAD is on disk. Source ports look for relative paths in their own folder,
        // so those resolve from the executable's. Null when it's relative and there's no executable to go by.
        public static string? ResolveWadPath(string wad, string? executable)
        {
            var path = EnsureWadExtension(wad);

            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);

            if (string.IsNullOrWhiteSpace(executable) || !Path.IsPathRooted(executable))
                return null;

            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(executable)!, path));
        }

        // "Sigil" -> "Sigil (2)", "Sigil (2)" -> "Sigil (3)", skipping names already taken.
        public static string GetDuplicateName(string name, Func<string, bool> isTaken)
        {
            var match = DuplicateNamePattern.Match(name);
            var baseName = match.Success ? match.Groups["base"].Value : name;
            var number = match.Success ? int.Parse(match.Groups["n"].Value) + 1 : 2;

            while (isTaken($"{baseName} ({number})"))
                number++;

            return $"{baseName} ({number})";
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
