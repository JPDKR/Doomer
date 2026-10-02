using Doomer.Options;
using Doomer.Services;

namespace Doomer
{
    public partial class BatchCreatorForm : Form
    {
        private const string WadFileFilter = "Doom files|*.wad;*.pk3;*.pk7;*.zip;*.deh;*.bex|All files|*.*";

        private readonly GZDoomSettings _gzdoomSettings = AppConfiguration.GZDoom;
        private readonly DSDADoomSettings _dsdaDoomSettings = AppConfiguration.DSDADoom;
        private readonly string? _editingFilePath;

        public BatchCreatorForm() : this(null) { }

        public BatchCreatorForm(string? existingFilePath)
        {
            InitializeComponent();
            _editingFilePath = existingFilePath;

            foreach (var port in Enum.GetValues<SourcePort>())
                cmbSourcePort.Items.Add(BatchFileService.GetDisplayName(port));

            SelectedPort = string.IsNullOrWhiteSpace(_gzdoomSettings.Location) && !string.IsNullOrWhiteSpace(_dsdaDoomSettings.Location)
                ? SourcePort.DSDADoom
                : SourcePort.GZDoom;

            ApplyDarkTheme();

            if (_editingFilePath is not null)
                LoadExistingBatch(_editingFilePath);
        }

        private SourcePort SelectedPort
        {
            get => (SourcePort)cmbSourcePort.SelectedIndex;
            set => cmbSourcePort.SelectedIndex = (int)value;
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

            if (!BatchFileService.TryParseCommand(command, out var port, out var iwad, out var wad, out var plugins))
            {
                MessageBox.Show("Couldn't parse this batch file's command. Please review the fields below.",
                    "Edit batch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedPort = port;
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
                    case ComboBox cb:
                        cb.FlatStyle = FlatStyle.Flat;
                        cb.BackColor = Color.FromArgb(38, 38, 38);
                        cb.ForeColor = Color.FromArgb(220, 220, 220);
                        break;
                    case Label lbl:
                        lbl.ForeColor = Color.FromArgb(170, 170, 170);
                        break;
                    case Button btn when btn == btnCreate:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = Color.FromArgb(160, 15, 15);
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = Color.FromArgb(200, 25, 25);
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 25, 25);
                        btn.Cursor = Cursors.Hand;
                        break;
                    case Button btn:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = Color.FromArgb(50, 50, 50);
                        btn.ForeColor = Color.FromArgb(220, 220, 220);
                        btn.FlatAppearance.BorderColor = Color.FromArgb(75, 75, 75);
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 70, 70);
                        btn.Cursor = Cursors.Hand;
                        break;
                }
            }

            txtFileName.PlaceholderText = "e.g. my-doom-mod";
            txtIWad.PlaceholderText = "e.g. wads/doom2";
            txtWad.PlaceholderText = "e.g. brutal-doom or mods/myhouse.pk3";
            UpdatePluginsState();
        }

        // DSDA Doom batches only take an IWAD and a WAD; plugins are a GZDoom-only feature.
        private void UpdatePluginsState()
        {
            var supportsPlugins = SelectedPort == SourcePort.GZDoom;
            txtPlugins.Enabled = supportsPlugins;
            txtPlugins.PlaceholderText = supportsPlugins ? "e.g. smoothed (optional)" : "Not used by DSDA Doom";
        }

        private void CmbSourcePort_SelectedIndexChanged(object? sender, EventArgs e) => UpdatePluginsState();

        private void BtnBrowseIWad_Click(object sender, EventArgs e) => BrowseFile(txtIWad, "Select IWAD");

        private void BtnBrowseWad_Click(object sender, EventArgs e) => BrowseFile(txtWad, "Select WAD");

        private static void BrowseFile(TextBox target, string title)
        {
            using var dlg = new OpenFileDialog { Title = title, Filter = WadFileFilter };

            var current = BatchFileService.EnsureWadExtension(target.Text.Trim());
            var dir = Path.IsPathRooted(current) ? Path.GetDirectoryName(current) : null;
            if (Directory.Exists(dir))
                dlg.InitialDirectory = dir;

            if (dlg.ShowDialog() == DialogResult.OK)
                target.Text = BatchFileService.StripWadExtension(dlg.FileName);
        }

        private static bool ConfirmPathIfMissing(string label, string path)
        {
            if (!Path.IsPathRooted(path) || File.Exists(path))
                return true;

            var proceed = MessageBox.Show(
                $"{label} file not found:\n{path}\n\nCreate the batch anyway?",
                "Batch creation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            return proceed == DialogResult.Yes;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            var port = SelectedPort;
            var iwad = txtIWad.Text.Trim();
            var wadFile = txtWad.Text.Trim();
            var plugins = port == SourcePort.GZDoom ? txtPlugins.Text.Trim() : string.Empty;
            var fileName = txtFileName.Text.Trim();

            if (string.IsNullOrWhiteSpace(iwad) || string.IsNullOrWhiteSpace(wadFile) || string.IsNullOrWhiteSpace(fileName))
            {
                MessageBox.Show("Please enter the IWAD, WAD and batch file name.", "Batch creation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var executable = port == SourcePort.DSDADoom ? _dsdaDoomSettings.Location : _gzdoomSettings.Location;

            if (string.IsNullOrWhiteSpace(executable))
            {
                MessageBox.Show($"The {BatchFileService.GetDisplayName(port)} executable isn't configured. Set it in Settings first.",
                    "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!BatchFileService.IsValidFileName(fileName, out var fileNameError))
            {
                MessageBox.Show(fileNameError, "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ConfirmPathIfMissing("IWAD", BatchFileService.EnsureWadExtension(iwad)) ||
                !ConfirmPathIfMissing("WAD", BatchFileService.EnsureWadExtension(wadFile)))
            {
                return;
            }

            string command;

            try
            {
                command = port == SourcePort.DSDADoom
                    ? BatchFileService.BuildCommand(_dsdaDoomSettings, iwad, wadFile)
                    : BatchFileService.BuildCommand(_gzdoomSettings, iwad, wadFile, plugins);
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
