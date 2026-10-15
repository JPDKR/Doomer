using Doomer.Options;
using Doomer.Services;

namespace Doomer
{
    public partial class BatchCreatorForm : Form
    {
        private static string WadFileFilter => $"{Strings.FileFilterDoom}|*.wad;*.pk3;*.pk7;*.zip;*.deh;*.bex|{Strings.FileFilterAll}|*.*";
        private static string ImageFileFilter => $"{Strings.FileFilterImages}|*.png;*.jpg;*.jpeg;*.bmp;*.gif|{Strings.FileFilterAll}|*.*";
        private static string ArchiveFileFilter => $"{Strings.FileFilterArchives}|{ArchiveService.FilePatterns}|{Strings.FileFilterAll}|*.*";

        private readonly GZDoomSettings _gzdoomSettings = AppConfiguration.GZDoom;
        private readonly DSDADoomSettings _dsdaDoomSettings = AppConfiguration.DSDADoom;
        private readonly string? _editingFilePath;
        private bool _hasExistingImage;

        public BatchCreatorForm() : this(null) { }

        public BatchCreatorForm(string? existingFilePath)
        {
            InitializeComponent();
            ApplyTexts();
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // TextBoxes size their height to the font after DPI scaling, so match the buttons at runtime.
            AlignToTextBox(btnBrowseIWad, txtIWad);
            AlignToTextBox(btnBrowseWad, txtWad);
            AlignToTextBox(btnBrowseImage, txtImage);
        }

        private static void AlignToTextBox(Button button, TextBox textBox)
        {
            button.Top = textBox.Top;
            button.Height = textBox.Height;
        }

        private SourcePort SelectedPort
        {
            get => (SourcePort)cmbSourcePort.SelectedIndex;
            set => cmbSourcePort.SelectedIndex = (int)value;
        }

        private void LoadExistingBatch(string path)
        {
            Text = Strings.BatchFormEditTitle;
            btnCreate.Text = Strings.SaveButton;
            txtFileName.Text = Path.GetFileNameWithoutExtension(path);

            // The batch name is fixed once created, and so is its image once it has one.
            txtFileName.ReadOnly = true;
            var imagePath = GetImagePath(path);
            _hasExistingImage = imagePath is not null && File.Exists(imagePath);

            if (_hasExistingImage)
            {
                btnBrowseImage.Enabled = false;
                txtImage.ReadOnly = true;
                txtImage.Text = imagePath;
            }

            string command;

            try
            {
                command = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.CantReadBatch, ex.Message), Strings.EditBatchTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!BatchFileService.TryParseCommand(command, _gzdoomSettings.Plugins, out var port, out var iwad, out var wad,
                    out var extraFiles, out var plugins))
            {
                MessageBox.Show(Strings.CantParseBatch,
                    Strings.EditBatchTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedPort = port;
            txtIWad.Text = iwad;
            txtWad.Text = wad;
            txtExtraFiles.Lines = [.. extraFiles];
            txtPlugins.Text = plugins;
        }

        private void ApplyTexts()
        {
            Text = Strings.BatchFormAddTitle;
            btnFromArchive.Text = Strings.FromArchiveButton;
            lblFromArchive.Text = Strings.FromArchiveHint;
            lblFileName.Text = Strings.LabelFileName;
            lblSourcePort.Text = Strings.LabelSourcePort;
            lblExtraFiles.Text = Strings.LabelExtraFiles;
            lblImage.Text = Strings.LabelImage;
            btnBrowseIWad.Text = btnBrowseWad.Text = btnBrowseImage.Text = Strings.BrowseButton;
            btnCreate.Text = Strings.CreateButton;

            txtFileName.PlaceholderText = Strings.PlaceholderFileName;
            txtIWad.PlaceholderText = Strings.PlaceholderIWad;
            txtWad.PlaceholderText = Strings.PlaceholderWad;
            txtExtraFiles.PlaceholderText = Strings.PlaceholderExtraFiles;
            txtImage.PlaceholderText = Strings.PlaceholderImage;
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

            UpdatePluginsState();
        }

        // DSDA Doom batches only take an IWAD and a WAD; plugins are a GZDoom-only feature.
        private void UpdatePluginsState()
        {
            var supportsPlugins = SelectedPort == SourcePort.GZDoom;
            txtPlugins.Enabled = supportsPlugins;
            txtPlugins.PlaceholderText = supportsPlugins ? Strings.PlaceholderPlugins : Strings.PlaceholderPluginsUnsupported;
        }

        private void CmbSourcePort_SelectedIndexChanged(object? sender, EventArgs e) => UpdatePluginsState();

        private void BtnBrowseIWad_Click(object sender, EventArgs e) => BrowseFile(txtIWad, Strings.SelectIWad);

        private void BtnBrowseWad_Click(object sender, EventArgs e) => BrowseFile(txtWad, Strings.SelectWad);

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

        // Extracts a downloaded archive into its own folder in the WADs directory and fills
        // in the WAD (and the file name, for a new batch) from what's inside.
        private async void BtnFromArchive_Click(object sender, EventArgs e)
        {
            var wadsLocation = _gzdoomSettings.Wads.Location;

            if (string.IsNullOrWhiteSpace(wadsLocation) || !Directory.Exists(wadsLocation))
            {
                MessageBox.Show(string.Format(Strings.WadsDirNotFoundFixInSettings, wadsLocation),
                    Strings.ExtractArchiveTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog { Title = Strings.SelectArchive, Filter = ArchiveFileFilter };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            var folderName = AskForWadName(ArchiveService.GetFolderName(dlg.FileName));

            if (folderName is null)
                return;

            var destination = Path.Combine(wadsLocation, folderName);

            if (Directory.Exists(destination) && Directory.EnumerateFileSystemEntries(destination).Any())
            {
                var proceed = MessageBox.Show(
                    string.Format(Strings.FolderExistsOverwrite, folderName),
                    Strings.ExtractArchiveTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (proceed != DialogResult.Yes)
                    return;
            }

            List<string> files;
            Enabled = false;
            UseWaitCursor = true;

            try
            {
                files = await Task.Run(() => ArchiveService.Extract(dlg.FileName, destination));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.ExtractError, ex.Message), Strings.ExtractArchiveTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                UseWaitCursor = false;
                Enabled = true;
            }

            var wad = PickWad(ArchiveService.FindWadCandidates(files), destination);

            if (wad is null)
                return;

            txtWad.Text = BatchFileService.StripWadExtension(wad);
            txtExtraFiles.Lines = [.. ArchiveService.FindExtraFiles(files, wad)];

            if (!txtFileName.ReadOnly)
                txtFileName.Text = folderName;
        }

        // Lets the user keep the name taken from the archive or type another one, until it's valid.
        // Returns null if they cancel.
        private string? AskForWadName(string suggestion)
        {
            var prompt = txtFileName.ReadOnly ? Strings.AskFolderName : Strings.AskFolderAndBatchName;
            var name = suggestion;

            while (true)
            {
                using var dlg = new NamePromptForm(Strings.ExtractArchiveTitle, prompt, name);

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return null;

                // Windows drops trailing dots and spaces from folder names, so do it up front.
                name = dlg.EnteredName.TrimEnd('.', ' ');

                if (BatchFileService.IsValidFileName(name, out var error))
                    return name;

                MessageBox.Show(error, Strings.ExtractArchiveTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string? PickWad(List<string> candidates, string folder)
        {
            if (candidates.Count == 1)
                return candidates[0];

            if (candidates.Count == 0)
            {
                MessageBox.Show(string.Format(Strings.NoWadInArchive, folder),
                    Strings.ExtractArchiveTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            using var dlg = new OpenFileDialog
            {
                Title = Strings.PickWadTitle,
                Filter = WadFileFilter,
                InitialDirectory = folder
            };

            return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_gzdoomSettings.Images.Location))
            {
                MessageBox.Show(Strings.ImagesDirNotConfigured,
                    Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog { Title = Strings.SelectImage, Filter = ImageFileFilter };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            if (!IsValidImage(dlg.FileName))
            {
                MessageBox.Show(Strings.InvalidImageSelected, Strings.BatchCreationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtImage.Text = dlg.FileName;
        }

        // Ctrl+V with a copied image (e.g. "Copy image" in a browser) saves it to a temp file and uses that.
        // Sites like Doom Wiki block downloads from apps, but the browser has already fetched the image.
        private void TxtImage_KeyDown(object? sender, KeyEventArgs e)
        {
            if (txtImage.ReadOnly || !e.Control || e.KeyCode != Keys.V || !Clipboard.ContainsImage())
                return;

            e.SuppressKeyPress = true;

            using var image = GetClipboardImage();

            if (image is null)
            {
                MessageBox.Show(Strings.ClipboardImageUnreadable, Strings.BatchCreationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var path = Path.Combine(Path.GetTempPath(), $"Doomer-pasted-{Guid.NewGuid():N}.png");

            try
            {
                image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.PastedImageSaveError, ex.Message), Strings.BatchCreationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtImage.Text = path;
        }

        // Browsers also put a PNG copy on the clipboard, which keeps transparency; the plain bitmap one doesn't.
        private static Image? GetClipboardImage()
        {
            if (Clipboard.TryGetData<MemoryStream>("PNG", out var png))
            {
                try
                {
                    using (png)
                    using (var image = Image.FromStream(png))
                        return new Bitmap(image);
                }
                catch (Exception)
                {
                    // Fall back to the bitmap copy below.
                }
            }

            return Clipboard.GetImage();
        }

        private static bool IsValidImage(string path)
        {
            try
            {
                using var _ = Image.FromFile(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Downloads the image at url to a temp file, or returns null (after telling the user why) if it fails.
        private async Task<string?> DownloadImageAsync(string url)
        {
            string path;
            Enabled = false;
            UseWaitCursor = true;

            try
            {
                path = await ImageDownloadService.DownloadAsync(url);
            }
            catch (Exception ex)
            {
                var message = ex is TaskCanceledException ? Strings.DownloadTimedOut : ex.Message;
                MessageBox.Show(string.Format(Strings.ImageDownloadError, message), Strings.BatchCreationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            finally
            {
                UseWaitCursor = false;
                Enabled = true;
            }

            if (IsValidImage(path))
                return path;

            File.Delete(path);
            MessageBox.Show(Strings.UrlNotAnImage,
                Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        private string? GetImagePath(string batchPath) =>
            string.IsNullOrWhiteSpace(_gzdoomSettings.Images.Location)
                ? null
                : BatchFileService.GetImagePath(batchPath, _gzdoomSettings.Images);

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
                string.Format(Strings.FileNotFoundCreateAnyway, label, path),
                Strings.BatchCreationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            return proceed == DialogResult.Yes;
        }

        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            var port = SelectedPort;
            var iwad = txtIWad.Text.Trim();
            var wadFile = txtWad.Text.Trim();
            var plugins = port == SourcePort.GZDoom ? txtPlugins.Text.Trim() : string.Empty;
            List<string> extraFiles = [.. txtExtraFiles.Lines.Select(l => l.Trim()).Where(l => l.Length > 0)];
            var fileName = txtFileName.Text.Trim();

            if (string.IsNullOrWhiteSpace(iwad) || string.IsNullOrWhiteSpace(wadFile) || string.IsNullOrWhiteSpace(fileName))
            {
                MessageBox.Show(Strings.MissingRequiredFields, Strings.BatchCreationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var executable = port == SourcePort.DSDADoom ? _dsdaDoomSettings.Location : _gzdoomSettings.Location;

            if (string.IsNullOrWhiteSpace(executable))
            {
                MessageBox.Show(string.Format(Strings.ExecutableNotConfigured, BatchFileService.GetDisplayName(port)),
                    Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!BatchFileService.IsValidFileName(fileName, out var fileNameError))
            {
                MessageBox.Show(fileNameError, Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ConfirmPathIfMissing("IWAD", BatchFileService.EnsureWadExtension(iwad)) ||
                !ConfirmPathIfMissing("WAD", BatchFileService.EnsureWadExtension(wadFile)) ||
                !extraFiles.All(f => ConfirmPathIfMissing(Strings.ExtraFileLabel, BatchFileService.EnsureWadExtension(f))))
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
                    MessageBox.Show(string.Format(Strings.WadsDirNotFoundFixInSettings, _gzdoomSettings.Wads.Location),
                        Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (File.Exists(wadDestination))
                {
                    var replace = MessageBox.Show(
                        string.Format(Strings.WadExistsReplace, Path.GetFileName(wadDestination)),
                        Strings.BatchCreationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (replace != DialogResult.Yes)
                        return;
                }

                wadFile = BatchFileService.StripWadExtension(wadDestination);
            }

            string command;

            try
            {
                command = port == SourcePort.DSDADoom
                    ? BatchFileService.BuildCommand(_dsdaDoomSettings, iwad, wadFile, extraFiles)
                    : BatchFileService.BuildCommand(_gzdoomSettings, iwad, wadFile, plugins, extraFiles);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var path = _editingFilePath ?? Path.Combine(_gzdoomSettings.Batchs.Location, fileName + _gzdoomSettings.Batchs.Extension);

            // An existing image is never replaced; one can only be added if the batch has none yet.
            var imageSource = _hasExistingImage ? string.Empty : txtImage.Text.Trim();
            // Named from the batch's own path, so the icon always matches the batch file.
            var imageDestination = string.IsNullOrEmpty(imageSource) ? null : GetImagePath(path);

            if (!string.IsNullOrEmpty(imageSource))
            {
                if (imageDestination is null || !Directory.Exists(_gzdoomSettings.Images.Location))
                {
                    MessageBox.Show(string.Format(Strings.ImagesDirNotFoundFixInSettings, _gzdoomSettings.Images.Location),
                        Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ImageDownloadService.IsUrl(imageSource) && !File.Exists(imageSource))
                {
                    MessageBox.Show(string.Format(Strings.ImageFileNotFound, imageSource), Strings.BatchCreationTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ImageDownloadService.IsUrl(imageSource) && !IsValidImage(imageSource))
                {
                    MessageBox.Show(string.Format(Strings.ImageFileInvalid, imageSource), Strings.BatchCreationTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (File.Exists(path) && _editingFilePath is null)
            {
                var overwrite = MessageBox.Show(
                    string.Format(Strings.BatchExistsOverwrite, fileName),
                    Strings.BatchCreationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (overwrite != DialogResult.Yes)
                    return;
            }

            // Downloaded last, once everything else is checked, so nothing is saved if it fails.
            if (ImageDownloadService.IsUrl(imageSource))
            {
                var downloaded = await DownloadImageAsync(imageSource);

                if (downloaded is null)
                    return;

                imageSource = downloaded;
            }

            if (wadDestination is not null)
            {
                try
                {
                    File.Move(wadSource, wadDestination, overwrite: true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Strings.MoveWadError, ex.Message), Strings.BatchCreationTitle,
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
                MessageBox.Show(string.Format(Strings.SaveBatchError, ex.Message), Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(string.Format(Strings.ImageMoveError, ex.Message),
                    Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (imageMoved)
                MessageBox.Show(_editingFilePath is null ? Strings.BatchCreated : Strings.BatchUpdated,
                    Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
