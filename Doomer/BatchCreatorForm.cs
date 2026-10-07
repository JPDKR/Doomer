using Doomer.Options;
using Doomer.Services;

namespace Doomer
{
    public partial class BatchCreatorForm : Form
    {
        private const string WadFileFilter = "Doom files|*.wad;*.pk3;*.pk7;*.zip;*.deh;*.bex|All files|*.*";
        private const string ImageFileFilter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*";

        private readonly GZDoomSettings _gzdoomSettings = AppConfiguration.GZDoom;
        private readonly DSDADoomSettings _dsdaDoomSettings = AppConfiguration.DSDADoom;
        private readonly string? _editingFilePath;
        private bool _hasExistingImage;

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

            // The batch name is fixed once created, and so is its image once it has one.
            txtFileName.ReadOnly = true;
            var imagePath = GetImagePath(txtFileName.Text);
            _hasExistingImage = imagePath is not null && File.Exists(imagePath);

            if (_hasExistingImage)
            {
                btnBrowseImage.Enabled = false;
                txtImage.Text = imagePath;
            }

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
            txtImage.PlaceholderText = "Optional icon for the WAD button";
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

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_gzdoomSettings.Images.Location))
            {
                MessageBox.Show("The images directory isn't configured. Set it in Settings first.",
                    "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog { Title = "Select image", Filter = ImageFileFilter };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using var _ = Image.FromFile(dlg.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("The selected file isn't a valid image.", "Batch creation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtImage.Text = dlg.FileName;
        }

        private string? GetImagePath(string name) =>
            string.IsNullOrWhiteSpace(_gzdoomSettings.Images.Location)
                ? null
                : Path.Combine(_gzdoomSettings.Images.Location, name + _gzdoomSettings.Images.Extension);

        // Moves the picked image into the images folder named after the WAD. If its format
        // differs from the configured extension, it's re-encoded so the icon loads correctly.
        private static void MoveImage(string source, string destination)
        {
            if (string.Equals(Path.GetFullPath(source), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
                return;

            var sourceExt = Path.GetExtension(source);
            var destExt = Path.GetExtension(destination);

            if (string.Equals(sourceExt, destExt, StringComparison.OrdinalIgnoreCase))
            {
                File.Move(source, destination, overwrite: true);
                return;
            }

            using (var img = Image.FromFile(source))
                img.Save(destination, GetImageFormat(destExt));

            File.Delete(source);
        }

        private static System.Drawing.Imaging.ImageFormat GetImageFormat(string extension) => extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => System.Drawing.Imaging.ImageFormat.Jpeg,
            ".bmp" => System.Drawing.Imaging.ImageFormat.Bmp,
            ".gif" => System.Drawing.Imaging.ImageFormat.Gif,
            _ => System.Drawing.Imaging.ImageFormat.Png
        };

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

            // A WAD outside the WADs directory gets moved into it, and the batch points to its new location.
            var wadSource = BatchFileService.EnsureWadExtension(wadFile);
            var wadDestination = File.Exists(wadSource)
                ? BatchFileService.GetWadDestination(wadSource, _gzdoomSettings.Wads.Location)
                : null;

            if (wadDestination is not null)
            {
                if (!Directory.Exists(_gzdoomSettings.Wads.Location))
                {
                    MessageBox.Show($"WADs directory not found:\n{_gzdoomSettings.Wads.Location}\n\nFix it in Settings first.",
                        "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (File.Exists(wadDestination))
                {
                    var replace = MessageBox.Show(
                        $"A WAD named \"{Path.GetFileName(wadDestination)}\" already exists in the WADs directory. Replace it?",
                        "Batch creation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (replace != DialogResult.Yes)
                        return;
                }

                wadFile = BatchFileService.StripWadExtension(wadDestination);
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

            var path = _editingFilePath ?? Path.Combine(_gzdoomSettings.Batchs.Location, fileName + _gzdoomSettings.Batchs.Extension);

            // An existing image is never replaced; one can only be added if the batch has none yet.
            var imageSource = _hasExistingImage ? string.Empty : txtImage.Text.Trim();
            var imageDestination = string.IsNullOrEmpty(imageSource) ? null : GetImagePath(fileName);

            if (!string.IsNullOrEmpty(imageSource))
            {
                if (imageDestination is null || !Directory.Exists(_gzdoomSettings.Images.Location))
                {
                    MessageBox.Show($"Images directory not found:\n{_gzdoomSettings.Images.Location}\n\nFix it in Settings first.",
                        "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!File.Exists(imageSource))
                {
                    MessageBox.Show($"Image file not found:\n{imageSource}", "Batch creation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (File.Exists(path) && _editingFilePath is null)
            {
                var overwrite = MessageBox.Show(
                    $"A batch file named \"{fileName}\" already exists. Overwrite it?",
                    "Batch creation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (overwrite != DialogResult.Yes)
                    return;
            }

            if (wadDestination is not null)
            {
                try
                {
                    File.Move(wadSource, wadDestination, overwrite: true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error moving the WAD to the WADs directory: " + ex.Message, "Batch creation",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                txtWad.Text = wadFile;
            }

            try
            {
                File.WriteAllText(path, command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving the batch: " + ex.Message, "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var imageMoved = true;

            try
            {
                if (imageDestination is not null)
                    MoveImage(imageSource, imageDestination);
            }
            catch (Exception ex)
            {
                imageMoved = false;
                MessageBox.Show("The batch was saved, but the image couldn't be moved: " + ex.Message,
                    "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (imageMoved)
                MessageBox.Show(_editingFilePath is null ? "Batch created successfully." : "Batch updated successfully.",
                    "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
