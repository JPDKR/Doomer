using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace Doomer.Services
{
    // Sha256 is the installer's hash as GitHub publishes it (lowercase hex), or null when the release has none.
    public record UpdateInfo(Version Version, string InstallerUrl, string InstallerName, string? Sha256);

    public static class UpdateService
    {
        private const string LatestReleaseUrl = "https://api.github.com/repos/JPDKR/Doomer/releases/latest";
        private const string InstallerPrefix = "DoomerSetup-";

        private static readonly HttpClient Http = CreateClient();

        public static Version CurrentVersion =>
            Normalize(Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0));

        // Returns null when there's no newer release, or when GitHub can't be reached —
        // an update check should never get in the way of launching the app.
        public static async Task<UpdateInfo?> CheckForUpdateAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await using var stream = await Http.GetStreamAsync(LatestReleaseUrl, cts.Token);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cts.Token);

                return ParseRelease(doc.RootElement, CurrentVersion);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                return null;
            }
        }

        public static UpdateInfo? ParseRelease(JsonElement release, Version currentVersion)
        {
            if (!release.TryGetProperty("tag_name", out var tag) || !TryParseVersion(tag.GetString(), out var latest))
                return null;

            if (latest <= currentVersion || !release.TryGetProperty("assets", out var assets))
                return null;

            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString() ?? string.Empty;
                if (name.StartsWith(InstallerPrefix, StringComparison.OrdinalIgnoreCase) &&
                    name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    var digest = asset.TryGetProperty("digest", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : null;
                    return new UpdateInfo(latest, asset.GetProperty("browser_download_url").GetString()!, name, ParseSha256Digest(digest));
                }
            }

            return null;
        }

        public static bool TryParseVersion(string? tag, out Version version)
        {
            version = default!;
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            if (!Version.TryParse(tag.Trim().TrimStart('v', 'V'), out var parsed))
                return false;

            version = Normalize(parsed);
            return true;
        }

        // GitHub publishes each asset's hash as "sha256:<64 hex digits>"; anything else counts as no hash.
        public static string? ParseSha256Digest(string? digest)
        {
            const string prefix = "sha256:";

            if (digest is null || !digest.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var hex = digest[prefix.Length..];
            return hex.Length == 64 && hex.All(char.IsAsciiHexDigit) ? hex.ToLowerInvariant() : null;
        }

        public static bool HasSha256(string path, string expectedSha256)
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexStringLower(SHA256.HashData(stream)) == expectedSha256;
        }

        // The installer runs silently with the user's rights, so it's only handed over once its hash matches
        // the one GitHub published for it; a release without one isn't installed at all. This catches a
        // corrupted or tampered download, not a bad release uploaded to the repository itself.
        public static async Task<string> DownloadInstallerAsync(UpdateInfo update)
        {
            if (update.Sha256 is null)
                throw new InvalidDataException(Strings.UpdateChecksumMissing);

            // A folder of its own, so a file left over in the temp folder with the same name is never run.
            var folder = Directory.CreateTempSubdirectory("Doomer-update-").FullName;
            var path = Path.Combine(folder, Path.GetFileName(update.InstallerName));

            await using (var source = await Http.GetStreamAsync(update.InstallerUrl))
            await using (var target = File.Create(path))
                await source.CopyToAsync(target);

            if (HasSha256(path, update.Sha256))
                return path;

            Directory.Delete(folder, recursive: true);
            throw new InvalidDataException(Strings.UpdateChecksumMismatch);
        }

        // The installer closes this app, replaces the files and relaunches it (see installer/Doomer.iss).
        public static void RunInstaller(string installerPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = "/SILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS",
                UseShellExecute = true
            });
        }

        // "1.2" and "1.2.0.0" should compare as equal.
        private static Version Normalize(Version v) =>
            new(v.Major, v.Minor, Math.Max(v.Build, 0));

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            // GitHub's API rejects requests without a User-Agent.
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Doomer", CurrentVersion.ToString()));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            return client;
        }
    }
}
