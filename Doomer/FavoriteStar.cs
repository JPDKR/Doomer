using System.Drawing.Drawing2D;

namespace Doomer
{
    // The star in a WAD button's top-left corner: filled on a favorite, and on the rest only while the mouse
    // is over the button. Clicking it marks or unmarks the WAD instead of launching it.
    internal sealed class FavoriteStar
    {
        private const int Size = 16;
        private const int Inset = 4;

        private readonly Button _button;
        private readonly Func<bool> _isFavorite;
        private bool _hovered;
        private bool _overStar;
        private bool _pressed;

        // Whether the click being handled went down and up on the star, so it toggles the favorite.
        public bool Pressed => _pressed && _overStar;

        public FavoriteStar(Button button, Func<bool> isFavorite)
        {
            _button = button;
            _isFavorite = isFavorite;

            _button.Paint += Button_Paint;
            _button.MouseEnter += (s, e) => SetHover(true, _overStar);
            _button.MouseLeave += (s, e) => SetHover(false, false);
            _button.MouseMove += (s, e) => SetHover(true, Bounds.Contains(e.Location));
            _button.MouseDown += (s, e) => _pressed = e.Button == MouseButtons.Left && Bounds.Contains(e.Location);
            // Raised after Click, so a press doesn't carry over to a later click from the keyboard.
            _button.MouseUp += (s, e) => _pressed = false;
        }

        private static Rectangle Bounds => new(Inset, Inset, Size, Size);

        private void SetHover(bool hovered, bool overStar)
        {
            if (hovered == _hovered && overStar == _overStar)
                return;

            _hovered = hovered;
            _overStar = overStar;
            _button.Invalidate(Rectangle.Inflate(Bounds, 2, 2));
        }

        private void Button_Paint(object? sender, PaintEventArgs e)
        {
            var favorite = _isFavorite();
            if (!favorite && !_hovered)
                return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = new GraphicsPath();
            path.AddPolygon(StarPoints(Bounds));

            // The dark outline keeps it visible over light icons.
            using (var outline = new Pen(Theme.Background, 3) { LineJoin = LineJoin.Round })
                g.DrawPath(outline, path);

            if (favorite)
            {
                using var fill = new SolidBrush(_overStar ? Theme.FavoriteHover : Theme.Favorite);
                g.FillPath(fill, path);
            }
            else
            {
                using var fill = new SolidBrush(Color.FromArgb(160, Theme.Background));
                g.FillPath(fill, path);
                using var pen = new Pen(_overStar ? Theme.Favorite : Theme.MutedText, 1.5f) { LineJoin = LineJoin.Round };
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
