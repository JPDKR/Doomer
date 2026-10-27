namespace Doomer
{
    // Paints menus, context menus and the status bar in the app's dark theme.
    internal class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            // A checked item on the bar itself (the favorites filter) has no check mark to show it's on, so it turns gold.
            e.TextColor = !e.Item.Enabled ? Theme.DisabledText
                : e.Item is ToolStripMenuItem { Checked: true, IsOnDropDown: false } ? Theme.Favorite
                : Theme.Text;
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
            private static readonly Color Bg = Theme.Bar;
            private static readonly Color Accent = Theme.MenuHighlight;
            private static readonly Color Border = Theme.Border;

            public override Color MenuItemSelected => Accent;
            public override Color MenuItemBorder => Accent;
            public override Color MenuBorder => Border;
            public override Color MenuItemSelectedGradientBegin => Accent;
            public override Color MenuItemSelectedGradientEnd => Accent;
            public override Color MenuItemPressedGradientBegin => Accent;
            public override Color MenuItemPressedGradientEnd => Accent;
            public override Color MenuItemPressedGradientMiddle => Accent;
            public override Color ToolStripDropDownBackground => Theme.DropDown;
            public override Color ImageMarginGradientBegin => Theme.DropDown;
            public override Color ImageMarginGradientMiddle => Theme.DropDown;
            public override Color ImageMarginGradientEnd => Theme.DropDown;
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
