namespace Doomer
{
    // Paints menus, context menus and the status bar in the app's dark theme.
    internal class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        private static readonly Color TextColor = Color.FromArgb(220, 220, 220);
        private static readonly Color DisabledTextColor = Color.FromArgb(120, 120, 120);

        public DarkMenuRenderer() : base(new DarkColorTable())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? TextColor : DisabledTextColor;
            base.OnRenderItemText(e);
        }

        // Bars blend into the window, but a dropdown or context menu needs its edge to stand out from it.
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripDropDown)
                base.OnRenderToolStripBorder(e);
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            private static readonly Color Bg = Color.FromArgb(30, 30, 30);
            private static readonly Color Accent = Color.FromArgb(140, 15, 15);
            private static readonly Color Border = Color.FromArgb(55, 55, 55);

            public override Color MenuItemSelected => Accent;
            public override Color MenuItemBorder => Accent;
            public override Color MenuBorder => Border;
            public override Color MenuItemSelectedGradientBegin => Accent;
            public override Color MenuItemSelectedGradientEnd => Accent;
            public override Color MenuItemPressedGradientBegin => Accent;
            public override Color MenuItemPressedGradientEnd => Accent;
            public override Color MenuItemPressedGradientMiddle => Accent;
            public override Color ToolStripDropDownBackground => Color.FromArgb(28, 28, 28);
            public override Color ImageMarginGradientBegin => Color.FromArgb(28, 28, 28);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(28, 28, 28);
            public override Color ImageMarginGradientEnd => Color.FromArgb(28, 28, 28);
            public override Color MenuStripGradientBegin => Bg;
            public override Color MenuStripGradientEnd => Bg;
            public override Color ToolStripGradientBegin => Bg;
            public override Color ToolStripGradientMiddle => Bg;
            public override Color ToolStripGradientEnd => Bg;
            public override Color SeparatorLight => Border;
            public override Color SeparatorDark => Border;
            public override Color CheckBackground => Accent;
            public override Color CheckSelectedBackground => Accent;
            public override Color CheckPressedBackground => Accent;
            public override Color ButtonSelectedHighlight => Accent;
            public override Color ButtonPressedHighlight => Accent;
            public override Color ButtonCheckedHighlight => Accent;
            public override Color StatusStripGradientBegin => Bg;
            public override Color StatusStripGradientEnd => Bg;
        }
    }
}
