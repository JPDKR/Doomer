using Doomer.Options;
using Doomer.Services;

namespace Doomer
{
    public partial class BatchCreatorForm : Form
    {
        private readonly GZDoomSettings _gzdoomSettings = AppConfiguration.GZDoom;
        private readonly string? _editingFilePath;

        public BatchCreatorForm() : this(null) { }

        public BatchCreatorForm(string? existingFilePath)
        {
            InitializeComponent();
            _editingFilePath = existingFilePath;
            ApplyDarkTheme();

            if (_editingFilePath is not null)
                LoadExistingBatch(_editingFilePath);
        }

        private void LoadExistingBatch(string path)
        {
            Text = "Edit Batch File";
            btnCreate.Text = "Save";
            txtFileName.Text = Path.GetFileNameWithoutExtension(path);

            string command;

            try
            {
                command = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can't read the batch file:\n{ex.Message}", "Edit batch",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!BatchFileService.TryParseCommand(command, out var iwad, out var wad, out var plugins))
            {
                MessageBox.Show("Couldn't parse this batch file's command. Please review the fields below.",
                    "Edit batch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtIWad.Text = iwad;
            txtWad.Text = wad;
            txtPlugins.Text = plugins;
        }

        private void ApplyDarkTheme()
        {
            BackColor = Color.FromArgb(18, 18, 18);
            ForeColor = Color.FromArgb(220, 220, 220);

            foreach (Control ctrl in Controls)
            {
                switch (ctrl)
                {
                    case TextBox tb:
                        tb.BackColor = Color.FromArgb(38, 38, 38);
                        tb.ForeColor = Color.FromArgb(220, 220, 220);
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case Label lbl:
                        lbl.ForeColor = Color.FromArgb(170, 170, 170);
                        break;
                    case Button btn:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = Color.FromArgb(160, 15, 15);
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = Color.FromArgb(200, 25, 25);
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 25, 25);
                        btn.Cursor = Cursors.Hand;
                        break;
                }
            }

            txtFileName.PlaceholderText = "e.g. my-doom-mod";
            txtIWad.PlaceholderText = "e.g. doom2.wad";
            txtWad.PlaceholderText = "e.g. brutal-doom";
            txtPlugins.PlaceholderText = "e.g. smoothed (optional)";
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            var iwad = txtIWad.Text.Trim();
            var wadFile = txtWad.Text.Trim();
            var plugins = txtPlugins.Text.Trim();
            var fileName = txtFileName.Text.Trim();

            if (string.IsNullOrWhiteSpace(iwad) || string.IsNullOrWhiteSpace(wadFile) || string.IsNullOrWhiteSpace(fileName))
            {
                MessageBox.Show("Please enter the IWAD, WAD and batch file name.", "Batch creation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!BatchFileService.IsValidFileName(fileName, out var fileNameError))
            {
                MessageBox.Show(fileNameError, "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string command;

            try
            {
                command = BatchFileService.BuildCommand(_gzdoomSettings, iwad, wadFile, plugins);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var path = Path.Combine(_gzdoomSettings.Batchs.Location, fileName + _gzdoomSettings.Batchs.Extension);
            var isRename = _editingFilePath is not null &&
                !string.Equals(Path.GetFullPath(_editingFilePath), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase);

            if (File.Exists(path) && (_editingFilePath is null || isRename))
            {
                var overwrite = MessageBox.Show(
                    $"A batch file named \"{fileName}\" already exists. Overwrite it?",
                    "Batch creation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (overwrite != DialogResult.Yes)
                    return;
            }

            try
            {
                File.WriteAllText(path, command);

                if (isRename)
                    File.Delete(_editingFilePath!);

                MessageBox.Show(_editingFilePath is null ? "Batch created successfully." : "Batch updated successfully.",
                    "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving the batch: " + ex.Message, "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
