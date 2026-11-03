using Doomer.Services;
using System.Net;
using System.Net.Sockets;

namespace Doomer.Tests
{
    public class ImageDownloadServiceTests
    {
        [Theory]
        [InlineData("https://i.ibb.co/abc/icon.png", true)]
        [InlineData("http://example.com/icon.jpg", true)]
        [InlineData("C:\\Pics\\icon.png", false)]
        [InlineData("file:///C:/Pics/icon.png", false)]
        [InlineData("icon.png", false)]
        [InlineData("", false)]
        public void IsUrl_AcceptsOnlyHttpAndHttps(string text, bool expected)
        {
            Assert.Equal(expected, ImageDownloadService.IsUrl(text));
        }

        [Fact]
        public async Task DownloadAsync_SavesASmallFile()
        {
            using var server = new LocalServer(response => Write(response, 1024, sendLength: true));

            var path = await ImageDownloadService.DownloadAsync(server.Url);

            try
            {
                Assert.Equal(1024, new FileInfo(path).Length);
            }
            finally
            {
                File.Delete(path);
            }
        }

        // Refused from the length it announces, or while copying when it doesn't announce one.
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DownloadAsync_RefusesAFileTooLargeForAnImage(bool sendLength)
        {
            var size = (ImageDownloadService.MaxImageMegabytes + 1) * 1024 * 1024;
            using var server = new LocalServer(response => Write(response, size, sendLength));
            var before = Directory.GetFiles(Path.GetTempPath()).ToHashSet();

            await Assert.ThrowsAsync<HttpRequestException>(() => ImageDownloadService.DownloadAsync(server.Url));

            // The partial file isn't left in the temp folder.
            Assert.Empty(Directory.GetFiles(Path.GetTempPath()).Where(f => !before.Contains(f) && f.Contains("Doomer", StringComparison.OrdinalIgnoreCase)));
        }

        private static void Write(HttpListenerResponse response, int size, bool sendLength)
        {
            if (sendLength)
                response.ContentLength64 = size;
            else
                response.SendChunked = true;

            var buffer = new byte[64 * 1024];

            try
            {
                for (var sent = 0; sent < size; sent += buffer.Length)
                    response.OutputStream.Write(buffer, 0, Math.Min(buffer.Length, size - sent));

                response.Close();
            }
            catch (HttpListenerException)
            {
                // The client stopped reading, as it should once the file is too large.
            }
        }

        // Answers every request on a free localhost port with respond, on a background thread.
        private sealed class LocalServer : IDisposable
        {
            private readonly HttpListener _listener = new();

            public string Url { get; }

            public LocalServer(Action<HttpListenerResponse> respond)
            {
                Url = $"http://localhost:{FreePort()}/image/";
                _listener.Prefixes.Add(Url);
                _listener.Start();

                _ = Task.Run(async () =>
                {
                    while (_listener.IsListening)
                    {
                        try
                        {
                            var context = await _listener.GetContextAsync();
                            _ = Task.Run(() => respond(context.Response));
                        }
                        catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException)
                        {
                            return;
                        }
                    }
                });
            }

            private static int FreePort()
            {
                using var socket = new TcpListener(IPAddress.Loopback, 0);
                socket.Start();
                return ((IPEndPoint)socket.LocalEndpoint).Port;
            }

            public void Dispose() => _listener.Close();
        }
    }
}
