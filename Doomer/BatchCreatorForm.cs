using Doomer.Options;
using Doomer.Services;
using System.Runtime.InteropServices;

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

        // The extra files list grows a row per file up to this many, then scrolls.
        private const int MaxVisibleExtraFiles = 8;
        private int _extraFilesMinHeight;
        private int _extraFilesBaseHeight;
        private int _baseClientHeight;
        private Dictionary<Control, int>? _belowExtraFiles;

        public BatchCreatorForm() : this(null) { }

        public BatchCreatorForm(string? existingFilePath)
        {
            InitializeComponent();
            ApplyTexts();
            _editingFilePath = existingFilePath;

            foreach (var port in Enum.GetValues<SourcePort>())
                cmbSourcePort.Items.Add(BatchFileService.GetDisplayName(port));

            // Still editable, so a plugin that isn't in the folder (or an older batch's) can be typed in.
            cmbPlugins.Items.AddRange([.. BatchFileService.FindPlugins(BatchFileService.GetPluginsDirectory(_gzdoomSettings))]);

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
            AlignToTextBox(btnBrowseIWad, cmbIWad);
            AlignToTextBox(btnBrowseWad, txtWad);
            AlignToTextBox(btnBrowseImage, txtImage);
            AlignToTextBox(btnAddExtraFile, txtWad);
            btnAddExtraFile.Top = lstExtraFiles.Top;
            btnRemoveExtraFile.Height = btnAddExtraFile.Height;
            btnRemoveExtraFile.Top = btnAddExtraFile.Bottom + 6;

            // The empty list is as tall as its two buttons; everything below it moves along as it grows.
            _extraFilesMinHeight = btnRemoveExtraFile.Bottom - lstExtraFiles.Top;
            _extraFilesBaseHeight = lstExtraFiles.Height;
            _baseClientHeight = ClientSize.Height;
            _belowExtraFiles = Controls.Cast<Control>()
                .Where(c => c.Top >= lstExtraFiles.Bottom)
                .ToDictionary(c => c, c => c.Top);

            UpdateExtraFilesLayout();
            _ = LoadIWadsAsync();
        }

        // Lists the IWADs the other batches use and the ones in the WADs folder. It reads every batch
        // and WAD header, so it runs in the background; the box can be typed in meanwhile.
        private async Task LoadIWadsAsync()
        {
            var batchs = _gzdoomSettings.Batchs;
            var pluginsFolder = _gzdoomSettings.Plugins;
            var wadsLocation = _gzdoomSettings.Wads.Location;
            List<string> iwads;

            try
            {
                iwads = await Task.Run(() => IWadService.Merge([
                    .. IWadService.FindUsedIWads(batchs.Location, batchs.Extension, pluginsFolder),
                    .. IWadService.FindIWadFiles(wadsLocation)]));
            }
            catch (Exception)
            {
                // They're only suggestions; a folder that can't be read just leaves the list empty.
                return;
            }

            if (IsDisposed)
                return;

            cmbIWad.Items.AddRange([.. iwads]);

            // Full paths are long, so the list widens to show them whole.
            var widest = iwads.Select(i => TextRenderer.MeasureText(i, cmbIWad.Font).Width).DefaultIfEmpty(0).Max();
            cmbIWad.DropDownWidth = Math.Max(cmbIWad.Width, widest + SystemInformation.VerticalScrollBarWidth);
        }

        private static void AlignToTextBox(Button button, Control textBox)
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
            cmbIWad.Text = BatchFileService.EnsureWadExtension(iwad);
            txtWad.Text = wad;
            SetExtraFiles(extraFiles);
            cmbPlugins.Text = plugins;
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
            btnAddExtraFile.Text = Strings.AddButton;
            btnRemoveExtraFile.Text = Strings.RemoveButton;
            btnCreate.Text = Strings.CreateButton;

            txtFileName.PlaceholderText = Strings.PlaceholderFileName;
            txtWad.PlaceholderText = Strings.PlaceholderWad;
            txtImage.PlaceholderText = Strings.PlaceholderImage;
            txtPluginsUnsupported.PlaceholderText = Strings.PlaceholderPluginsUnsupported;
            SetCueBanner(cmbIWad, Strings.PlaceholderIWad);
            SetCueBanner(cmbPlugins, Strings.PlaceholderPlugins);
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
                    case ListBox lb:
                        lb.BackColor = Color.FromArgb(38, 38, 38);
                        lb.ForeColor = Color.FromArgb(220, 220, 220);
                        lb.BorderStyle = BorderStyle.FixedSingle;
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
        // A disabled ComboBox ignores the dark theme, so DSDA Doom shows a disabled text box in its place.
        private void UpdatePluginsState()
        {
            var supportsPlugins = SelectedPort == SourcePort.GZDoom;
            cmbPlugins.Visible = supportsPlugins;
            txtPluginsUnsupported.Visible = !supportsPlugins;

            if (!supportsPlugins)
            {
                cmbPlugins.SelectedIndex = -1;
                cmbPlugins.Text = string.Empty;
            }
        }

        private const int CB_SETCUEBANNER = 0x1703;

        [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        private static extern nint SendMessage(nint hWnd, int msg, nint wParam, string lParam);

        // ComboBox has no PlaceholderText, but Windows draws one for its text box when asked.
        private static void SetCueBanner(ComboBox comboBox, string text) =>
            SendMessage(comboBox.Handle, CB_SETCUEBANNER, 0, text);

        private void CmbSourcePort_SelectedIndexChanged(object? sender, EventArgs e) => UpdatePluginsState();

        // IWADs keep their .wad, like the ones in the list.
        private void BtnBrowseIWad_Click(object sender, EventArgs e) => BrowseFile(cmbIWad, Strings.SelectIWad, stripWadExtension: false);

        private void BtnBrowseWad_Click(object sender, EventArgs e)
        {
            BrowseFile(txtWad, Strings.SelectWad);
            SwitchToGZDoomIfNeeded();
        }

        private List<string> ExtraFiles => [.. lstExtraFiles.Items.Cast<string>()];

        private void SetExtraFiles(IEnumerable<string> files)
        {
            lstExtraFiles.Items.Clear();
            AddExtraFiles(files);
        }

        private void AddExtraFiles(IEnumerable<string> files)
        {
            foreach (var file in files.Select(f => f.Trim()).Where(f => f.Length > 0))
            {
                if (!ExtraFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
                    lstExtraFiles.Items.Add(file);
            }

            UpdateExtraFilesLayout();
        }

        // Sizes the list to its files (between its buttons' height and MaxVisibleExtraFiles rows)
        // and moves the controls below it, and the form's bottom edge, by the same amount.
        private void UpdateExtraFilesLayout()
        {
            btnRemoveExtraFile.Enabled = lstExtraFiles.SelectedIndices.Count > 0;

            if (_belowExtraFiles is null)
                return;

            var border = lstExtraFiles.Height - lstExtraFiles.ClientSize.Height;
            var rows = Math.Min(lstExtraFiles.Items.Count, MaxVisibleExtraFiles);
            var height = rows * lstExtraFiles.ItemHeight + border;

            // A file wider than the list brings up a horizontal scrollbar, which would hide the last row.
            var widest = ExtraFiles.Select(f => TextRenderer.MeasureText(f, lstExtraFiles.Font).Width).DefaultIfEmpty(0).Max();
            if (widest > lstExtraFiles.Width - border)
                height += SystemInformation.HorizontalScrollBarHeight;

            height = Math.Max(_extraFilesMinHeight, height);
            var delta = height - _extraFilesBaseHeight;

            SuspendLayout();
            lstExtraFiles.Height = height;
            foreach (var (control, top) in _belowExtraFiles)
                control.Top = top + delta;
            ClientSize = new Size(ClientSize.Width, _baseClientHeight + delta);
            ResumeLayout();
        }

        private void BtnAddExtraFile_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Title = Strings.SelectExtraFiles, Filter = WadFileFilter, Multiselect = true };

            var wad = BatchFileService.EnsureWadExtension(txtWad.Text.Trim());
            var dir = Path.IsPathRooted(wad) ? Path.GetDirectoryName(wad) : null;
            if (Directory.Exists(dir))
                dlg.InitialDirectory = dir;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            AddExtraFiles(dlg.FileNames);
            SwitchToGZDoomIfNeeded();
        }

        private void BtnRemoveExtraFile_Click(object sender, EventArgs e) => RemoveSelectedExtraFiles();

        private void LstExtraFiles_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            e.Handled = true;
            RemoveSelectedExtraFiles();
        }

        private void LstExtraFiles_SelectedIndexChanged(object? sender, EventArgs e) =>
            btnRemoveExtraFile.Enabled = lstExtraFiles.SelectedIndices.Count > 0;

        private void RemoveSelectedExtraFiles()
        {
            var index = lstExtraFiles.SelectedIndices.Count > 0 ? lstExtraFiles.SelectedIndices[0] : -1;

            foreach (var i in lstExtraFiles.SelectedIndices.Cast<int>().OrderDescending().ToList())
                lstExtraFiles.Items.RemoveAt(i);

            // Keep a selection where the removed files were, so Delete can be pressed repeatedly.
            if (index >= 0 && lstExtraFiles.Items.Count > 0)
                lstExtraFiles.SelectedIndex = Math.Min(index, lstExtraFiles.Items.Count - 1);

            UpdateExtraFilesLayout();
        }

        // DSDA Doom can't run .pk3/.pk7 files, so picking one switches the batch to GZDoom.
        private void SwitchToGZDoomIfNeeded()
        {
            if (BatchFileService.RequiresGZDoom(txtWad.Text.Trim(), ExtraFiles))
                SelectedPort = SourcePort.GZDoom;
        }

        private static void BrowseFile(Control target, string title, bool stripWadExtension = true)
        {
            using var dlg = new OpenFileDialog { Title = title, Filter = WadFileFilter };

            var current = BatchFileService.EnsureWadExtension(target.Text.Trim());
            var dir = Path.IsPathRooted(current) ? Path.GetDirectoryName(current) : null;
            if (Directory.Exists(dir))
                dlg.InitialDirectory = dir;

            if (dlg.ShowDialog() == DialogResult.OK)
                target.Text = stripWadExtension ? BatchFileService.StripWadExtension(dlg.FileName) : dlg.FileName;
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
            SetExtraFiles(ArchiveService.FindExtraFiles(files, wad));
            SwitchToGZDoomIfNeeded();

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

        // Like GZDoom, a plugin without an extension is looked up as a .wad.
        private bool ConfirmPluginIfMissing(string plugin)
        {
            var directory = BatchFileService.GetPluginsDirectory(_gzdoomSettings);

            if (string.IsNullOrEmpty(plugin) || directory is null)
                return true;

            var path = Path.Combine(directory, plugin);
            return File.Exists(path) || ConfirmPathIfMissing(Strings.PluginFileLabel, BatchFileService.EnsureWadExtension(path));
        }

        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            var port = SelectedPort;
            var iwad = cmbIWad.Text.Trim();
            var wadFile = txtWad.Text.Trim();
            var plugins = port == SourcePort.GZDoom ? cmbPlugins.Text.Trim() : string.Empty;
            var extraFiles = ExtraFiles;
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

            if (port == SourcePort.DSDADoom && BatchFileService.RequiresGZDoom(wadFile, extraFiles))
            {
                MessageBox.Show(Strings.DSDADoomNoPackages, Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ConfirmPathIfMissing("IWAD", BatchFileService.EnsureWadExtension(iwad)) ||
                !ConfirmPathIfMissing("WAD", BatchFileService.EnsureWadExtension(wadFile)) ||
                !extraFiles.All(f => ConfirmPathIfMissing(Strings.ExtraFileLabel, BatchFileService.EnsureWadExtension(f))) ||
                !ConfirmPluginIfMissing(plugins))
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
