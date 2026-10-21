using Doomer.Options;
using Doomer.Services;
using System.Runtime.InteropServices;

namespace Doomer
{
    public partial class BatchCreatorForm : Form
    {
        private static string WadFileFilter => $"{Strings.FileFilterDoom}|*.wad;*.pk3;*.pk7;*.zip;*.deh;*.bex|{Strings.FileFilterAll}|*.*";
        private static string ArchiveFileFilter => $"{Strings.FileFilterArchives}|{ArchiveService.FilePatterns}|{Strings.FileFilterAll}|*.*";

        private readonly PortsSettings _ports = AppConfiguration.Ports;
        private readonly FoldersSettings _folders = AppConfiguration.Folders;
        private readonly string? _editingFilePath;
        private bool _hasExistingImage;

        // The extras and plugins lists grow a row per file up to this many, then scroll.
        private const int MaxVisibleListRows = 8;
        private int _baseClientHeight;
        // Where every control sits before the lists grow, so they can move back when the lists shrink.
        private Dictionary<Control, int>? _baseTops;
        private readonly Dictionary<ListBox, (int MinHeight, int BaseHeight, int BaseBottom)> _growingLists = [];

        // An archive dropped on the main window, extracted as soon as the form shows.
        private string? _archiveToImport;

        public BatchCreatorForm() : this(null) { }

        // A new batch filled in from an archive, as if it had been picked with "From archive...".
        public static BatchCreatorForm FromArchive(string archivePath) => new() { _archiveToImport = archivePath };

        public BatchCreatorForm(string? existingFilePath)
        {
            InitializeComponent();
            ApplyTexts();
            _editingFilePath = existingFilePath;

            foreach (var port in Enum.GetValues<SourcePort>())
                cmbSourcePort.Items.Add(BatchFileService.GetDisplayName(port));

            SelectedPort = string.IsNullOrWhiteSpace(_ports.GZDoom.Path) && !string.IsNullOrWhiteSpace(_ports.DSDADoom.Path)
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
            AlignListButtons(lstExtraFiles, btnAddExtraFile, btnRemoveExtraFile);
            AlignListButtons(lstPlugins, btnAddPlugin, btnRemovePlugin);

            _baseClientHeight = ClientSize.Height;
            _baseTops = Controls.Cast<Control>().ToDictionary(c => c, c => c.Top);

            UpdateListsLayout();
            _ = LoadIWadsAsync();
        }

        // Lists the IWADs the other batches use and the ones in the WADs folder. It reads every batch
        // and WAD header, so it runs in the background; the box can be typed in meanwhile.
        private async Task LoadIWadsAsync()
        {
            var batches = _folders.Batches;
            var pluginsFolder = _ports.GZDoom.Plugins;
            var wadsLocation = _folders.Wads.Path;
            List<string> iwads;

            try
            {
                iwads = await Task.Run(() => IWadService.Merge([
                    .. IWadService.FindUsedIWads(batches.Path, batches.Extension, pluginsFolder),
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

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_archiveToImport is not null)
                await ImportArchiveAsync(_archiveToImport);
        }

        // An empty list is as tall as its two buttons; everything below it moves along as it grows.
        private void AlignListButtons(ListBox list, Button add, Button remove)
        {
            AlignToTextBox(add, txtWad);
            add.Top = list.Top;
            remove.Height = add.Height;
            remove.Top = add.Bottom + 6;
            _growingLists[list] = (remove.Bottom - list.Top, list.Height, list.Bottom);
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

            if (!BatchFileService.TryParseCommand(command, _ports.GZDoom.Plugins, out var port, out var iwad, out var wad,
                    out var extraFiles, out var plugins))
            {
                MessageBox.Show(Strings.CantParseBatch,
                    Strings.EditBatchTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedPort = port;
            cmbIWad.Text = BatchFileService.EnsureWadExtension(iwad);
            txtWad.Text = wad;
            SetItems(lstExtraFiles, extraFiles);
            SetItems(lstPlugins, plugins);
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
            btnAddExtraFile.Text = btnAddPlugin.Text = Strings.AddButton;
            btnRemoveExtraFile.Text = btnRemovePlugin.Text = Strings.RemoveButton;
            btnCreate.Text = Strings.CreateButton;

            txtFileName.PlaceholderText = Strings.PlaceholderFileName;
            txtWad.PlaceholderText = Strings.PlaceholderWad;
            txtImage.PlaceholderText = Strings.PlaceholderImage;
            txtPluginsUnsupported.PlaceholderText = Strings.PlaceholderPluginsUnsupported;
            SetCueBanner(cmbIWad, Strings.PlaceholderIWad);
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

        private bool SupportsPlugins => SelectedPort == SourcePort.GZDoom;

        // DSDA Doom batches only take an IWAD and a WAD; plugins are a GZDoom-only feature.
        // A disabled list ignores the dark theme, so DSDA Doom shows a disabled text box in its place.
        private void UpdatePluginsState()
        {
            var supportsPlugins = SupportsPlugins;
            lstPlugins.Visible = btnAddPlugin.Visible = btnRemovePlugin.Visible = supportsPlugins;
            txtPluginsUnsupported.Visible = !supportsPlugins;

            if (!supportsPlugins)
                lstPlugins.Items.Clear();

            UpdateListsLayout();
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

        private List<string> ExtraFiles => Items(lstExtraFiles);

        // In the order GZDoom loads them.
        private List<string> Plugins => Items(lstPlugins);

        private static List<string> Items(ListBox list) => [.. list.Items.Cast<string>()];

        private void SetItems(ListBox list, IEnumerable<string> items)
        {
            list.Items.Clear();
            AddItems(list, items);
        }

        private void AddItems(ListBox list, IEnumerable<string> items)
        {
            foreach (var item in items.Select(i => i.Trim()).Where(i => i.Length > 0))
            {
                if (!Items(list).Contains(item, StringComparer.OrdinalIgnoreCase))
                    list.Items.Add(item);
            }

            UpdateListsLayout();
        }

        private void RemoveSelectedItems(ListBox list)
        {
            var index = list.SelectedIndices.Count > 0 ? list.SelectedIndices[0] : -1;

            foreach (var i in list.SelectedIndices.Cast<int>().OrderDescending().ToList())
                list.Items.RemoveAt(i);

            // Keep a selection where the removed items were, so Delete can be pressed repeatedly.
            if (index >= 0 && list.Items.Count > 0)
                list.SelectedIndex = Math.Min(index, list.Items.Count - 1);

            UpdateListsLayout();
        }

        // Sizes each list to its items (between its buttons' height and MaxVisibleListRows rows) and moves
        // every control below a list, and the form's bottom edge, by how much the lists above it grew.
        private void UpdateListsLayout()
        {
            btnRemoveExtraFile.Enabled = lstExtraFiles.SelectedIndices.Count > 0;
            btnRemovePlugin.Enabled = lstPlugins.SelectedIndices.Count > 0;

            if (_baseTops is null)
                return;

            var deltas = _growingLists.ToDictionary(l => l.Key, l => GetListHeight(l.Key, l.Value.MinHeight) - l.Value.BaseHeight);

            SuspendLayout();
            foreach (var (list, delta) in deltas)
                list.Height = _growingLists[list].BaseHeight + delta;
            foreach (var (control, top) in _baseTops)
                control.Top = top + _growingLists.Where(l => l.Value.BaseBottom <= top).Sum(l => deltas[l.Key]);
            ClientSize = new Size(ClientSize.Width, _baseClientHeight + deltas.Values.Sum());
            ResumeLayout();
        }

        private int GetListHeight(ListBox list, int minHeight)
        {
            // Without plugins, only the one-line "not used" box takes the list's place.
            if (list == lstPlugins && !SupportsPlugins)
                return txtPluginsUnsupported.Height;

            var border = list.Height - list.ClientSize.Height;
            var rows = Math.Min(list.Items.Count, MaxVisibleListRows);
            var height = rows * list.ItemHeight + border;

            // An item wider than the list brings up a horizontal scrollbar, which would hide the last row.
            var widest = Items(list).Select(i => TextRenderer.MeasureText(i, list.Font).Width).DefaultIfEmpty(0).Max();
            if (widest > list.Width - border)
                height += SystemInformation.HorizontalScrollBarHeight;

            return Math.Max(minHeight, height);
        }

        // Offers the plugins folder's files that aren't in the batch yet.
        private void BtnAddPlugin_Click(object sender, EventArgs e)
        {
            var directory = BatchFileService.GetPluginsDirectory(_ports.GZDoom.Plugins, _ports.GZDoom.Path);
            var available = BatchFileService.FindPlugins(directory)
                .Where(p => !Plugins.Contains(p, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var menu = new ContextMenuStrip { Renderer = new DarkMenuRenderer() };
            menu.Closed += (s, args) => BeginInvoke(menu.Dispose);

            if (available.Count == 0)
            {
                var message = Directory.Exists(directory)
                    ? string.Format(Strings.NoMorePlugins, directory)
                    : string.Format(Strings.PluginsFolderNotFound, directory ?? _ports.GZDoom.Plugins);
                menu.Items.Add(new ToolStripMenuItem(message) { Enabled = false });
            }

            foreach (var plugin in available)
                menu.Items.Add(plugin, null, (s, args) => AddItems(lstPlugins, [plugin]));

            menu.Show(btnAddPlugin, new Point(0, btnAddPlugin.Height));
        }

        private void BtnRemovePlugin_Click(object sender, EventArgs e) => RemoveSelectedItems(lstPlugins);

        private void LstPlugins_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            e.Handled = true;
            RemoveSelectedItems(lstPlugins);
        }

        private void LstPlugins_SelectedIndexChanged(object? sender, EventArgs e) =>
            btnRemovePlugin.Enabled = lstPlugins.SelectedIndices.Count > 0;

        private void BtnAddExtraFile_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Title = Strings.SelectExtraFiles, Filter = WadFileFilter, Multiselect = true };

            var wad = BatchFileService.EnsureWadExtension(txtWad.Text.Trim());
            var dir = Path.IsPathRooted(wad) ? Path.GetDirectoryName(wad) : null;
            if (Directory.Exists(dir))
                dlg.InitialDirectory = dir;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            AddItems(lstExtraFiles, dlg.FileNames);
            SwitchToGZDoomIfNeeded();
        }

        // Files dragged from Explorer onto the list are added like ones picked with Add...; folders aren't.
        private static List<string> DroppedFiles(DragEventArgs e) =>
            e.Data?.GetData(DataFormats.FileDrop) is string[] paths ? [.. paths.Where(File.Exists)] : [];

        private void LstExtraFiles_DragEnter(object? sender, DragEventArgs e) =>
            e.Effect = DroppedFiles(e).Count > 0 ? DragDropEffects.Copy : DragDropEffects.None;

        private void LstExtraFiles_DragDrop(object? sender, DragEventArgs e)
        {
            AddItems(lstExtraFiles, DroppedFiles(e));
            SwitchToGZDoomIfNeeded();
        }

        private void BtnRemoveExtraFile_Click(object sender, EventArgs e) => RemoveSelectedItems(lstExtraFiles);

        private void LstExtraFiles_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            e.Handled = true;
            RemoveSelectedItems(lstExtraFiles);
        }

        private void LstExtraFiles_SelectedIndexChanged(object? sender, EventArgs e) =>
            btnRemoveExtraFile.Enabled = lstExtraFiles.SelectedIndices.Count > 0;

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

        private async void BtnFromArchive_Click(object sender, EventArgs e)
        {
            if (!CheckWadsLocation())
                return;

            using var dlg = new OpenFileDialog { Title = Strings.SelectArchive, Filter = ArchiveFileFilter };

            if (dlg.ShowDialog() == DialogResult.OK)
                await ImportArchiveAsync(dlg.FileName);
        }

        private bool CheckWadsLocation()
        {
            var wadsLocation = _folders.Wads.Path;

            if (!string.IsNullOrWhiteSpace(wadsLocation) && Directory.Exists(wadsLocation))
                return true;

            MessageBox.Show(string.Format(Strings.WadsDirNotFoundFixInSettings, wadsLocation),
                Strings.ExtractArchiveTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // Extracts a downloaded archive into its own folder in the WADs directory and fills
        // in the WAD (and the file name, for a new batch) from what's inside.
        private async Task ImportArchiveAsync(string archivePath)
        {
            if (!CheckWadsLocation())
                return;

            var wadsLocation = _folders.Wads.Path;
            var folderName = AskForWadName(ArchiveService.GetFolderName(archivePath));

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
                files = await Task.Run(() => ArchiveService.Extract(archivePath, destination));
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
            SetItems(lstExtraFiles, ArchiveService.FindExtraFiles(files, wad));
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
            if (string.IsNullOrWhiteSpace(_folders.Images.Path))
            {
                MessageBox.Show(Strings.ImagesDirNotConfigured,
                    Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog { Title = Strings.SelectImage, Filter = ImageFileService.FileFilter };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            if (!ImageFileService.IsValidImage(dlg.FileName))
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

            if (ImageFileService.IsValidImage(path))
                return path;

            File.Delete(path);
            MessageBox.Show(Strings.UrlNotAnImage,
                Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        private string? GetImagePath(string batchPath) =>
            string.IsNullOrWhiteSpace(_folders.Images.Path)
                ? null
                : BatchFileService.GetImagePath(batchPath, _folders.Images);

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
            var directory = BatchFileService.GetPluginsDirectory(_ports.GZDoom.Plugins, _ports.GZDoom.Path);

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
            List<string> plugins = port == SourcePort.GZDoom ? Plugins : [];
            var extraFiles = ExtraFiles;
            var fileName = txtFileName.Text.Trim();

            if (string.IsNullOrWhiteSpace(iwad) || string.IsNullOrWhiteSpace(wadFile) || string.IsNullOrWhiteSpace(fileName))
            {
                MessageBox.Show(Strings.MissingRequiredFields, Strings.BatchCreationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var executable = port == SourcePort.DSDADoom ? _ports.DSDADoom.Path : _ports.GZDoom.Path;

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
                !plugins.All(ConfirmPluginIfMissing))
            {
                return;
            }

            // A WAD outside the WADs directory gets moved into it, and the batch points to its new location.
            var wadSource = BatchFileService.EnsureWadExtension(wadFile);
            var wadDestination = File.Exists(wadSource)
                ? BatchFileService.GetWadDestination(wadSource, _folders.Wads.Path)
                : null;

            if (wadDestination is not null)
            {
                if (!Directory.Exists(_folders.Wads.Path))
                {
                    MessageBox.Show(string.Format(Strings.WadsDirNotFoundFixInSettings, _folders.Wads.Path),
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
                    ? BatchFileService.BuildDSDADoomCommand(_ports.DSDADoom.Path, iwad, wadFile, extraFiles)
                    : BatchFileService.BuildGZDoomCommand(_ports.GZDoom.Path, _ports.GZDoom.Plugins, iwad, wadFile, plugins, extraFiles);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var path = _editingFilePath ?? Path.Combine(_folders.Batches.Path, fileName + _folders.Batches.Extension);

            // An existing image is never replaced; one can only be added if the batch has none yet.
            var imageSource = _hasExistingImage ? string.Empty : txtImage.Text.Trim();
            // Named from the batch's own path, so the icon always matches the batch file.
            var imageDestination = string.IsNullOrEmpty(imageSource) ? null : GetImagePath(path);

            if (!string.IsNullOrEmpty(imageSource))
            {
                if (imageDestination is null || !Directory.Exists(_folders.Images.Path))
                {
                    MessageBox.Show(string.Format(Strings.ImagesDirNotFoundFixInSettings, _folders.Images.Path),
                        Strings.BatchCreationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ImageDownloadService.IsUrl(imageSource) && !File.Exists(imageSource))
                {
                    MessageBox.Show(string.Format(Strings.ImageFileNotFound, imageSource), Strings.BatchCreationTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ImageDownloadService.IsUrl(imageSource) && !ImageFileService.IsValidImage(imageSource))
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
                    ImageFileService.Save(imageSource, imageDestination, move: true);
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
