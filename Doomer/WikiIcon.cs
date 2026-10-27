namespace Doomer
{
    // The globe in a WAD button's bottom-right corner, shown when the WAD has a Doom Wiki page.
    // Clicking it opens the page instead of launching the WAD.
    internal sealed class WikiIcon(Button button, Func<bool> hasWikiPage) : CornerIcon(button, Corner.BottomRight)
    {
        protected override bool IsShown(bool hovered) => hasWikiPage();

        protected override void Draw(Graphics g, Rectangle bounds, bool overIcon)
        {
            // A dark disc behind it keeps it visible over light icons.
            using (var back = new SolidBrush(Color.FromArgb(200, Theme.Background)))
                g.FillEllipse(back, Rectangle.Inflate(bounds, 1, 1));

            var globe = Rectangle.Inflate(bounds, -2, -2);
            using var pen = new Pen(overIcon ? Theme.Favorite : Theme.Text, 1.3f);

            g.DrawEllipse(pen, globe);
            g.DrawEllipse(pen, Rectangle.Inflate(globe, -globe.Width / 4, 0));
            var middle = globe.Top + globe.Height / 2f;
            g.DrawLine(pen, globe.Left, middle, globe.Right, middle);
        }
    }
}
