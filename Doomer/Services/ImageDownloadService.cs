namespace Doomer.Services
{
    public static class ImageDownloadService
    {
        // Before Http, which is created with it.
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private static readonly HttpClient Http = CreateClient();

        // Far more than any WAD's cover, so a link to something else (a video, the WAD itself) isn't downloaded whole.
        public const int MaxImageMegabytes = 20;
        private const long MaxImageBytes = MaxImageMegabytes * 1024L * 1024;

        public static bool IsUrl(string text) =>
            Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        // Downloads the image to a temp file and returns its path. The file has no image extension
        // on purpose, so moving it into the images folder always re-encodes it to the configured format.
        public static async Task<string> DownloadAsync(string url)
        {
            // The client's timeout only covers the headers here, since the body is read as it arrives.
            using var cts = new CancellationTokenSource(Timeout);
            using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var blocked = response.StatusCode == System.Net.HttpStatusCode.Forbidden &&
                              response.Content.Headers.ContentType?.MediaType == "text/html";

                throw new HttpRequestException(blocked
                    ? Strings.DownloadBlocked
                    : string.Format(Strings.DownloadHttpError, (int)response.StatusCode, response.ReasonPhrase));
            }

            if (response.Content.Headers.ContentLength > MaxImageBytes)
                throw new HttpRequestException(string.Format(Strings.DownloadTooLarge, MaxImageMegabytes));

            var path = TempFileService.NewDownloadPath();

            try
            {
                await using var source = await response.Content.ReadAsStreamAsync(cts.Token);
                await using var file = File.Create(path);
                await CopyAtMostAsync(source, file, MaxImageBytes, cts.Token);
            }
            catch
            {
                TempFileService.TryDelete(path);
                throw;
            }

            return path;
        }

        // A server can leave out the length or send more than it said, so the limit is checked while copying too.
        private static async Task CopyAtMostAsync(Stream source, Stream target, long maxBytes, CancellationToken token)
        {
            var buffer = new byte[81920];
            long total = 0;
            int read;

            while ((read = await source.ReadAsync(buffer, token)) > 0)
            {
                total += read;

                if (total > maxBytes)
                    throw new HttpRequestException(string.Format(Strings.DownloadTooLarge, MaxImageMegabytes));

                await target.WriteAsync(buffer.AsMemory(0, read), token);
            }
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = Timeout };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Doomer");
            return client;
        }
    }
}
