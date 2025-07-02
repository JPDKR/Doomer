namespace Doomer.Options
{
    public class GZDoomSettings
    {
        public string Location { get; set; } = default!;
        public string Plugins { get; set; } = default!;
        public BatchSettings Batchs { get; set; } = new();
        public ImageSettings Images { get; set; } = new();
    }

    public class BatchSettings
    {
        public string Location { get; set; } = default!;
        public string Extension { get; set; } = default!;
    }

    public class ImageSettings
    {
        public string Location { get; set; } = default!;
        public string Extension { get; set; } = default!;
    }

    public class IconsSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Padding { get; set; }
    }
}
