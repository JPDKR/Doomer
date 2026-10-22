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

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(12, 12),
                Size = new Size(376, 24)
            };

            _txtName.Text = suggestion;
            _txtName.Location = new Point(12, 40);
            _txtName.Size = new Size(376, 27);

            var btnOk = CreateButton(Strings.OkButton, DialogResult.OK, new Point(182, 90));

            var btnCancel = CreateButton(Strings.CancelButton, DialogResult.Cancel, new Point(288, 90));

            AcceptButton = btnOk;
            CancelButton = btnCancel;
            Controls.AddRange([lblPrompt, _txtName, btnOk, btnCancel]);
            Theme.ApplyToDialog(this, btnOk);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _txtName.SelectAll();
            _txtName.Focus();
        }

        private static Button CreateButton(string text, DialogResult result, Point location) => new()
        {
            Text = text,
            DialogResult = result,
            Location = location,
            Size = new Size(100, 30)
        };
    }
}
