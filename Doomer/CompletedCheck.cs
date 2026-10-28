using System.Drawing.Drawing2D;

namespace Doomer
{
    // The check in a WAD button's bottom-left corner: filled on a WAD the user finished, and on the rest only
    // while the mouse is over the button. Clicking it marks or unmarks the WAD instead of launching it.
    internal sealed class CompletedCheck(Button button, Func<bool> isCompleted) : CornerIcon(button, Corner.BottomLeft)
    {
        protected override bool IsShown(bool hovered) => hovered || isCompleted();

        protected override void Draw(Graphics g, Rectangle bounds, bool overIcon)
        {
            var disc = Rectangle.Inflate(bounds, 1, 1);
            var completed = isCompleted();

            // A dark disc behind it keeps it visible over light icons, like the wiki globe.
            using (var back = new SolidBrush(completed ? (overIcon ? Theme.CompletedHover : Theme.Completed)
                : Color.FromArgb(200, Theme.Background)))
                g.FillEllipse(back, disc);

            if (!completed)
            {
                using var ring = new Pen(overIcon ? Theme.Completed : Theme.MutedText, 1.3f);
                g.DrawEllipse(ring, disc);
            }

            using var pen = new Pen(completed ? Color.White : overIcon ? Theme.Completed : Theme.MutedText, 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            g.DrawLines(pen,
            [
                new PointF(bounds.Left + bounds.Width * 0.25f, bounds.Top + bounds.Height * 0.52f),
                new PointF(bounds.Left + bounds.Width * 0.43f, bounds.Top + bounds.Height * 0.70f),
                new PointF(bounds.Left + bounds.Width * 0.76f, bounds.Top + bounds.Height * 0.32f)
            ]);
        }
    }
}
