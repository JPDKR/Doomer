using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace Doomer.Services
{
    public record UpdateInfo(Version Version, string InstallerUrl, string InstallerName);

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
                    return new UpdateInfo(latest, asset.GetProperty("browser_download_url").GetString()!, name);
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

        public static async Task<string> DownloadInstallerAsync(UpdateInfo update)
        {
            var path = Path.Combine(Path.GetTempPath(), update.InstallerName);

            await using var source = await Http.GetStreamAsync(update.InstallerUrl);
            await using var target = File.Create(path);
            await source.CopyToAsync(target);

            return path;
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
