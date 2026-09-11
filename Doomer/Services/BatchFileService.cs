using Doomer.Options;
using System.Text.RegularExpressions;

namespace Doomer.Services
{
    public static class BatchFileService
    {
        private static readonly Regex CommandPattern = new(
            "-iwad \"(?<iwad>.*?)\\.wad\" -file \"(?<wad>.*?)\\.wad\"(?: \"(?<pluginsPath>.*?)\")?",
            RegexOptions.Compiled);

        public static string EnsureWadExtension(string path) =>
            path.EndsWith(".wad", StringComparison.OrdinalIgnoreCase) ? path : path + ".wad";

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
            if (ContainsQuote(iwad) || ContainsQuote(wad) || ContainsQuote(plugins))
                throw new ArgumentException("IWAD, WAD and plugin paths cannot contain quote characters.");

            var command = $"\"{settings.Location}\" -iwad \"{EnsureWadExtension(iwad)}\" -file \"{EnsureWadExtension(wad)}\"";

            if (!string.IsNullOrWhiteSpace(plugins))
                command += $" \"{settings.Plugins}/{plugins}\"";

            return command;
        }

        public static bool TryParseCommand(string command, out string iwad, out string wad, out string plugins)
        {
            var match = CommandPattern.Match(command);

            if (!match.Success)
            {
                iwad = wad = plugins = string.Empty;
                return false;
            }

            iwad = match.Groups["iwad"].Value;
            wad = match.Groups["wad"].Value;
            plugins = match.Groups["pluginsPath"].Success
                ? Path.GetFileName(match.Groups["pluginsPath"].Value)
                : string.Empty;

            return true;
        }

        private static bool ContainsQuote(string value) => value.Contains('"');
    }
}
