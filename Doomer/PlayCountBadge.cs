using System.Drawing.Drawing2D;

namespace Doomer
{
    // How many times a WAD was played, as "×3" on a small pill in its button's top-right corner. It stays on a WAD
    // played more than once, and on the rest shows up only while the mouse is over the button. Clicking it lets
    // the user enter the count, such as for the times played before Doomer counted them.
    internal sealed class PlayCountBadge(Button button, Func<int> playCount) : CornerIcon(button, Corner.TopRight)
    {
        private const int Padding = 5;

        private static readonly Font BadgeFont = new("Segoe UI", 7.5f, FontStyle.Bold);

        private string Text => "×" + playCount();

        protected override int Width =>
            Math.Max(16, TextRenderer.MeasureText(Text, BadgeFont, Size.Empty, TextFormatFlags.NoPadding).Width + Padding * 2);

        protected override bool IsShown(bool hovered) => hovered || playCount() > 1;

        protected override void Draw(Graphics g, Rectangle bounds, bool overIcon)
        {
            // Dark like the other corner icons' discs, so it reads over light icons too.
            using (var path = Pill(bounds))
            using (var back = new SolidBrush(Color.FromArgb(200, Theme.Background)))
            using (var ring = new Pen(overIcon ? Theme.AccentHover : Theme.MutedText, overIcon ? 1.5f : 1f))
            {
                g.FillPath(back, path);
                g.DrawPath(ring, path);
            }

            TextRenderer.DrawText(g, Text, BadgeFont, bounds, overIcon || playCount() > 1 ? Theme.Text : Theme.MutedText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private static GraphicsPath Pill(Rectangle r)
        {
            var path = new GraphicsPath();
            var d = r.Height;
            path.AddArc(r.Left, r.Top, d, d, 90, 180);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 180);
            path.CloseFigure();
            return path;
        }
    }
}
