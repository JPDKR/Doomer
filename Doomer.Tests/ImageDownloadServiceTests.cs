using Doomer.Services;

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
    }
}
