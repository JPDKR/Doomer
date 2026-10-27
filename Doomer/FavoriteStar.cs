using System.Drawing.Drawing2D;

namespace Doomer
{
    // The star in a WAD button's top-left corner: filled on a favorite, and on the rest only while the mouse
    // is over the button. Clicking it marks or unmarks the WAD instead of launching it.
    internal sealed class FavoriteStar(Button button, Func<bool> isFavorite) : CornerIcon(button, Corner.TopLeft)
    {
        protected override bool IsShown(bool hovered) => hovered || isFavorite();

        protected override void Draw(Graphics g, Rectangle bounds, bool overIcon)
        {
            using var path = new GraphicsPath();
            path.AddPolygon(StarPoints(bounds));

            // The dark outline keeps it visible over light icons.
            using (var outline = new Pen(Theme.Background, 3) { LineJoin = LineJoin.Round })
                g.DrawPath(outline, path);

            if (isFavorite())
            {
                using var fill = new SolidBrush(overIcon ? Theme.FavoriteHover : Theme.Favorite);
                g.FillPath(fill, path);
            }
            else
            {
                using var fill = new SolidBrush(Color.FromArgb(160, Theme.Background));
                g.FillPath(fill, path);
                using var pen = new Pen(overIcon ? Theme.Favorite : Theme.MutedText, 1.5f) { LineJoin = LineJoin.Round };
                g.DrawPath(pen, path);
            }
        }

        // A five-pointed star filling the square, pointing up.
        private static PointF[] StarPoints(Rectangle bounds)
        {
            var points = new PointF[10];
            var center = new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f + 0.8f);
            var outer = bounds.Width / 2f;
            var inner = outer * 0.45f;

            for (var i = 0; i < points.Length; i++)
            {
                var radius = i % 2 == 0 ? outer : inner;
                var angle = Math.PI * (i / 5.0 - 0.5);
                points[i] = new PointF(center.X + (float)(radius * Math.Cos(angle)),
                    center.Y + (float)(radius * Math.Sin(angle)));
            }

            return points;
        }
    }
}
