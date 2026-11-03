using System.Drawing.Drawing2D;

namespace Doomer
{
    internal enum Corner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    // A small clickable icon drawn in a corner of a WAD button. It tracks the mouse so it can light up while
    // pointed at, and tells the button's Click whether the click was on it rather than on the rest of the button.
    internal abstract class CornerIcon
    {
        private const int Size = 16;
        private const int Inset = 4;

        private readonly Button _button;
        private readonly Corner _corner;
        private bool _hovered;
        private bool _overIcon;
        private bool _pressed;

        protected CornerIcon(Button button, Corner corner)
        {
            _button = button;
            _corner = corner;

            _button.Paint += Button_Paint;
            _button.MouseEnter += (s, e) => SetHover(true, _overIcon);
            _button.MouseLeave += (s, e) => SetHover(false, false);
            _button.MouseMove += (s, e) => SetHover(true, Bounds.Contains(e.Location));
            _button.MouseDown += (s, e) => _pressed = e.Button == MouseButtons.Left && Bounds.Contains(e.Location);
            // Raised after Click, so a press doesn't carry over to a later click from the keyboard.
            _button.MouseUp += (s, e) => _pressed = false;
        }

        // Whether the click being handled went down and up on the icon while it was shown.
        public bool Pressed => _pressed && _overIcon && IsShown(_hovered);

        // Whether the click being handled went down on the icon, wherever it came up. One that came up elsewhere
        // does nothing, rather than launching the game the user didn't mean to.
        public bool WentDownOnIcon => _pressed && IsShown(true);

        // Wider than it's tall for an icon that holds text.
        protected virtual int Width => Size;

        // Whether it's drawn, given whether the mouse is over the button.
        protected abstract bool IsShown(bool hovered);

        protected abstract void Draw(Graphics g, Rectangle bounds, bool overIcon);

        private Rectangle Bounds => _corner switch
        {
            Corner.TopLeft => new(Inset, Inset, Size, Size),
            Corner.TopRight => new(_button.ClientSize.Width - Inset - Width, Inset, Width, Size),
            Corner.BottomLeft => new(Inset, _button.ClientSize.Height - Inset - Size, Size, Size),
            _ => new(_button.ClientSize.Width - Inset - Size, _button.ClientSize.Height - Inset - Size, Size, Size)
        };

        public void Refresh() => _button.Invalidate(Rectangle.Inflate(Bounds, 2, 2));

        private void SetHover(bool hovered, bool overIcon)
        {
            if (hovered == _hovered && overIcon == _overIcon)
                return;

            _hovered = hovered;
            _overIcon = overIcon;
            Refresh();
        }

        private void Button_Paint(object? sender, PaintEventArgs e)
        {
            if (!IsShown(_hovered))
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Draw(e.Graphics, Bounds, _overIcon);
        }
    }
}
