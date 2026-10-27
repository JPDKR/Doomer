using System.Drawing.Drawing2D;

namespace Doomer
{
    // Darkens a control and draws a spinner with a message over it, until disposed.
    internal sealed class LoadingOverlay : IDisposable
    {
        private const int SpinnerStep = 12;
        private const int ArcLength = 90;

        private readonly Control _target;
        private readonly string _text;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 40 };
        private int _angle;

        public LoadingOverlay(Control target, string text)
        {
            _target = target;
            _text = text;

            _target.Paint += Target_Paint;
            _timer.Tick += Timer_Tick;
            _timer.Start();
            _target.Invalidate();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // The buttons are rebuilt when the list refreshes, which can happen while a game starts.
            if (_target.IsDisposed)
            {
                Dispose();
                return;
            }

            _angle = (_angle + SpinnerStep) % 360;
            _target.Invalidate();
        }

        private void Target_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = _target.ClientRectangle;

            using (var shade = new SolidBrush(Color.FromArgb(215, Theme.Background)))
                g.FillRectangle(shade, bounds);

            var textHeight = TextRenderer.MeasureText(_text, _target.Font).Height;
            var size = Math.Clamp(Math.Min(bounds.Width, bounds.Height - textHeight) / 3, 12, 32);
            var top = bounds.Top + (bounds.Height - size - 6 - textHeight) / 2;
            var circle = new Rectangle(bounds.Left + (bounds.Width - size) / 2, top, size, size);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var track = new Pen(Theme.Border, 3))
                g.DrawEllipse(track, circle);

            using (var arc = new Pen(Theme.AccentHover, 3) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(arc, circle, _angle, ArcLength);

            TextRenderer.DrawText(g, _text, _target.Font,
                new Rectangle(bounds.Left, circle.Bottom + 6, bounds.Width, textHeight), Theme.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
        }

        public void Dispose()
        {
            _timer.Dispose();
            _target.Paint -= Target_Paint;

            if (!_target.IsDisposed)
                _target.Invalidate();
        }
    }
}
