namespace Doomer.Services
{
    public static class ImageDownloadService
    {
        private static readonly HttpClient Http = CreateClient();

        public static bool IsUrl(string text) =>
            Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        // Downloads the image to a temp file and returns its path. The file has no image extension
        // on purpose, so moving it into the images folder always re-encodes it to the configured format.
        public static async Task<string> DownloadAsync(string url)
        {
            using var response = await Http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var blocked = response.StatusCode == System.Net.HttpStatusCode.Forbidden &&
                              response.Content.Headers.ContentType?.MediaType == "text/html";

                throw new HttpRequestException(blocked
                    ? Strings.DownloadBlocked
                    : string.Format(Strings.DownloadHttpError, (int)response.StatusCode, response.ReasonPhrase));
            }

            var path = TempFileService.NewDownloadPath();

            await using (var file = File.Create(path))
                await response.Content.CopyToAsync(file);

            return path;
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Doomer");
            return client;
        }
    }
}
