namespace Doomer
{
    // The app's dark theme: every color it uses, and the styling its dialogs share.
    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(18, 18, 18);
        // The menu and status bars.
        public static readonly Color Bar = Color.FromArgb(30, 30, 30);
        // Dropdowns and context menus.
        public static readonly Color DropDown = Color.FromArgb(28, 28, 28);
        // Text boxes, lists and combo boxes, and the WAD buttons in the main window.
        public static readonly Color Input = Color.FromArgb(38, 38, 38);
        public static readonly Color SearchBox = Color.FromArgb(48, 48, 48);
        // Menu borders and separators.
        public static readonly Color Border = Color.FromArgb(55, 55, 55);
        // The line under "From archive..." in the batch form.
        public static readonly Color Separator = Color.FromArgb(60, 60, 60);

        public static readonly Color Text = Color.FromArgb(220, 220, 220);
        // Labels and group box titles in the dialogs.
        public static readonly Color LabelText = Color.FromArgb(170, 170, 170);
        // The main window's secondary text: status bar, loading message and version.
        public static readonly Color MutedText = Color.FromArgb(140, 140, 140);
        public static readonly Color DisabledText = Color.FromArgb(120, 120, 120);

        public static readonly Color Accent = Color.FromArgb(160, 15, 15);
        public static readonly Color AccentHover = Color.FromArgb(200, 25, 25);
        // A selected, pressed or checked menu item.
        public static readonly Color MenuHighlight = Color.FromArgb(140, 15, 15);
        public static readonly Color WadButtonHover = Color.FromArgb(65, 10, 10);
        // A favorite's button gets a gold border and star so it stands out from the rest.
        public static readonly Color Favorite = Color.FromArgb(212, 160, 23);
        public static readonly Color FavoriteHover = Color.FromArgb(240, 190, 50);
        // A completed WAD's check, and the shade laid over its button.
        public static readonly Color Completed = Color.FromArgb(70, 180, 80);
        public static readonly Color CompletedHover = Color.FromArgb(100, 210, 110);
        public static readonly Color CompletedShade = Color.FromArgb(130, 0, 0, 0);

        // Buttons other than a dialog's main action.
        public static readonly Color NeutralButton = Color.FromArgb(50, 50, 50);
        public static readonly Color NeutralButtonHover = Color.FromArgb(70, 70, 70);
        public static readonly Color NeutralButtonBorder = Color.FromArgb(75, 75, 75);

        // Styles a dialog and everything in it, group boxes included. The primary button (its main action)
        // gets the accent color; the rest are neutral.
        public static void ApplyToDialog(Form form, Button primary)
        {
            form.BackColor = Background;
            form.ForeColor = Text;
            ApplyToControls(form.Controls, primary);
        }

        private static void ApplyToControls(Control.ControlCollection controls, Button primary)
        {
            foreach (Control control in controls)
            {
                switch (control)
                {
                    case GroupBox groupBox:
                        groupBox.BackColor = Background;
                        groupBox.ForeColor = LabelText;
                        ApplyToControls(groupBox.Controls, primary);
                        break;
                    case TextBox textBox:
                        StyleInput(textBox);
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case ListBox listBox:
                        StyleInput(listBox);
                        listBox.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case ComboBox comboBox:
                        StyleInput(comboBox);
                        comboBox.FlatStyle = FlatStyle.Flat;
                        break;
                    case NumericUpDown numericUpDown:
                        StyleInput(numericUpDown);
                        break;
                    case Label label:
                        label.ForeColor = LabelText;
                        break;
                    case Button button when button == primary:
                        StyleButton(button, Accent, Color.White, AccentHover, AccentHover);
                        break;
                    case Button button:
                        StyleButton(button, NeutralButton, Text, NeutralButtonBorder, NeutralButtonHover);
                        break;
                }
            }
        }

        private static void StyleInput(Control control)
        {
            control.BackColor = Input;
            control.ForeColor = Text;
        }

        private static void StyleButton(Button button, Color back, Color fore, Color border, Color hover)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = back;
            button.ForeColor = fore;
            button.FlatAppearance.BorderColor = border;
            button.FlatAppearance.MouseOverBackColor = hover;
            button.Cursor = Cursors.Hand;
        }
    }
}
