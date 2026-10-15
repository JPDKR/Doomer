namespace Doomer
{
    // A small dark-themed dialog asking for a single name, prefilled with a suggestion.
    public class NamePromptForm : Form
    {
        private readonly TextBox _txtName = new();

        public string EnteredName => _txtName.Text.Trim();

        public NamePromptForm(string title, string prompt, string suggestion)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 136);
            BackColor = Color.FromArgb(18, 18, 18);
            ForeColor = Color.FromArgb(220, 220, 220);

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(12, 12),
                Size = new Size(376, 24),
                ForeColor = Color.FromArgb(170, 170, 170)
            };

            _txtName.Text = suggestion;
            _txtName.Location = new Point(12, 40);
            _txtName.Size = new Size(376, 27);
            _txtName.BackColor = Color.FromArgb(38, 38, 38);
            _txtName.ForeColor = Color.FromArgb(220, 220, 220);
            _txtName.BorderStyle = BorderStyle.FixedSingle;

            var btnOk = CreateButton(Strings.OkButton, DialogResult.OK, new Point(182, 90));
            btnOk.BackColor = Color.FromArgb(160, 15, 15);
            btnOk.ForeColor = Color.White;
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(200, 25, 25);
            btnOk.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 25, 25);

            var btnCancel = CreateButton(Strings.CancelButton, DialogResult.Cancel, new Point(288, 90));

            AcceptButton = btnOk;
            CancelButton = btnCancel;
            Controls.AddRange([lblPrompt, _txtName, btnOk, btnCancel]);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _txtName.SelectAll();
            _txtName.Focus();
        }

        private static Button CreateButton(string text, DialogResult result, Point location)
        {
            var button = new Button
            {
                Text = text,
                DialogResult = result,
                Location = location,
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.FromArgb(220, 220, 220),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(75, 75, 75);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 70, 70);
            return button;
        }
    }
}
