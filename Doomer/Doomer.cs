using Doomer.Options;
using Doomer.Services;
using System.Diagnostics;

namespace Doomer
{
    public partial class Doomer : Form
    {
        private PortsSettings _ports = default!;
        private FoldersSettings _folders = default!;
        private IconsSettings _iconsSettings = default!;
        private readonly ToolTip _wadTooltip = new();
        private readonly Font _wadButtonFont = new("Segoe UI", 8.5f);
        private CancellationTokenSource? _loadCts;
        private readonly LibraryService _library = new(AppConfiguration.LibraryFilePath);

        // The batches whose game is still open, so a double click or a second click doesn't launch it again.
        // Kept by path, so it survives the buttons being rebuilt.
        private readonly HashSet<string> _runningBatches = new(StringComparer.OrdinalIgnoreCase);

        public Doomer()
        {
            InitializeComponent();
            components?.Add(_wadTooltip);
            ApplyTexts();
            lblVersion.Text = $"v{UpdateService.CurrentVersion}";
            LoadConfiguration();
            ApplyDarkTheme();
            EnableArchiveDrop(this);
            EnableArchiveDrop(flowLayoutPanel1);
        }

        private void LoadConfiguration()
        {
            _ports = AppConfiguration.Ports;
            _folders = AppConfiguration.Folders;
            _iconsSettings = AppConfiguration.Interface.Icons;
        }

        private void ApplyDarkTheme()
        {
            BackColor = Theme.Background;
            ForeColor = Theme.Text;

            menuStrip1.BackColor = Theme.Bar;
            menuStrip1.ForeColor = Theme.Text;
            menuStrip1.Renderer = new DarkMenuRenderer();

            sortToolStripMenuItem.DropDown.Renderer = menuStrip1.Renderer;
            UpdateSortMenu();

            foreach (ToolStripItem item in menuStrip1.Items)
                item.ForeColor = Theme.Text;

            txtSearch.BackColor = Theme.SearchBox;
            txtSearch.ForeColor = Theme.Text;

            flowLayoutPanel1.BackColor = Theme.Background;

            statusStrip1.BackColor = Theme.Bar;
            statusStrip1.Renderer = new DarkMenuRenderer();
            lblStatus.ForeColor = Theme.MutedText;

            loadingPanel.BackColor = Theme.Background;
            lblLoading.ForeColor = Theme.MutedText;
            lblVersion.ForeColor = Theme.MutedText;
        }

        private void ApplyTexts()
        {
            addBatchFileToolStripMenuItem.Text = Strings.MenuAddBatch;
            refreshListToolStripMenuItem.Text = Strings.MenuRefresh;
            settingsToolStripMenuItem.ToolTipText = Strings.MenuSettings;
            settingsToolStripMenuItem.AccessibleName = Strings.MenuSettings;
            txtSearch.TextBox.PlaceholderText = Strings.SearchPlaceholder;
            lblLoading.Text = Strings.LoadingWads;
            sortToolStripMenuItem.Text = Strings.MenuSort;
            sortByNameToolStripMenuItem.Text = Strings.MenuSortByName;
            sortByLastPlayedToolStripMenuItem.Text = Strings.MenuSortByLastPlayed;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // Images a crash left in the temp folder. It never fails, so it runs on its own in the background.
            _ = Task.Run(() => TempFileService.DeleteLeftovers(Path.GetTempPath(), TimeSpan.FromDays(1), DateTime.UtcNow));

            await LoadButtonsBatchAsync();
            await CheckForUpdateAsync();
        }

        private async Task CheckForUpdateAsync()
        {
            var update = await UpdateService.CheckForUpdateAsync();
            if (update is null || UpdateService.IsSkipped(update, UpdateService.GetSkippedVersion(AppConfiguration.SkippedUpdateFilePath)))
                return;

            // MessageBox can't name its buttons, so this uses Windows' task dialog for the third choice.
            var updateButton = new TaskDialogButton(Strings.UpdateNowButton);
            var skipButton = new TaskDialogButton(Strings.UpdateSkipButton);
            var page = new TaskDialogPage
            {
                Caption = Strings.UpdateAvailableTitle,
                Text = string.Format(Strings.UpdateAvailableMessage, update.Version, UpdateService.CurrentVersion),
                Icon = TaskDialogIcon.Information,
                Buttons = { updateButton, new TaskDialogButton(Strings.UpdateLaterButton), skipButton },
                DefaultButton = updateButton
            };

            var answer = TaskDialog.ShowDialog(this, page);

            if (answer == skipButton)
            {
                // If it can't be saved, the worst case is being asked again on the next start.
                try
                {
                    UpdateService.SkipVersion(AppConfiguration.SkippedUpdateFilePath, update.Version);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                }
            }

            if (answer != updateButton)
                return;

            var previousStatus = lblStatus.Text;
            try
            {
                lblStatus.Text = "  " + string.Format(Strings.UpdateDownloading, update.Version);
                UseWaitCursor = true;

                var installerPath = await UpdateService.DownloadInstallerAsync(update);
                UpdateService.RunInstaller(installerPath);
                Close();
            }
            catch (Exception ex)
            {
                UseWaitCursor = false;
                lblStatus.Text = previousStatus;
                MessageBox.Show(string.Format(Strings.UpdateFailedMessage, ex.Message), Strings.UpdateFailedTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private record WadEntry(string File, string Name, Bitmap? Icon);

        // Reads the batch files and decodes their icons off the UI thread, then builds the buttons.
        // A newer call cancels an older one still running, so only the latest result is shown.
        private async Task LoadButtonsBatchAsync()
        {
            _loadCts?.Cancel();
            var cts = new CancellationTokenSource();
            _loadCts = cts;

            loadingPanel.Visible = true;
            loadingPanel.BringToFront();

            var batches = _folders.Batches;
            var images = _folders.Images;
            var iconSize = new Size(_iconsSettings.Width, _iconsSettings.Height);

            List<WadEntry> entries;
            try
            {
                entries = await Task.Run(() => ReadWads(batches, images, iconSize, cts.Token), cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException or ArgumentException)
            {
                if (_loadCts != cts || IsDisposed)
                    return;

                ShowButtons([]);
                MessageBox.Show(string.Format(Strings.BatchsDirNotFoundOpenSettings, ex.Message),
                    Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_loadCts != cts || IsDisposed)
            {
                foreach (var entry in entries)
                    entry.Icon?.Dispose();
                return;
            }

            ShowButtons(entries);
        }

        private static List<WadEntry> ReadWads(BatchSettings batches, ImageSettings images, Size iconSize, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(batches.Path))
                return [];

            var entries = new List<WadEntry>();
            try
            {
                foreach (var file in Directory.GetFiles(batches.Path, $"*{batches.Extension}"))
                {
                    token.ThrowIfCancellationRequested();
                    entries.Add(new WadEntry(file, Path.GetFileNameWithoutExtension(file),
                        LoadIcon(BatchFileService.GetImagePath(file, images), iconSize)));
                }
            }
            catch
            {
                foreach (var entry in entries)
                    entry.Icon?.Dispose();
                throw;
            }

            return entries;
        }

        private static Bitmap? LoadIcon(string iconPath, Size size)
        {
            if (!File.Exists(iconPath))
                return null;

            try
            {
                using var img = Image.FromFile(iconPath);
                return new Bitmap(img, size);
            }
            catch
            {
                return null;
            }
        }

        private void ShowButtons(List<WadEntry> entries)
        {
            flowLayoutPanel1.SuspendLayout();

            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is Button boton)
                {
                    boton.Image?.Dispose();
                    boton.ContextMenuStrip?.Dispose();
                }

                control.Dispose();
            }

            flowLayoutPanel1.Controls.Clear();
            var order = _library.Sort(entries.Select(e => e.Name));
            flowLayoutPanel1.Controls.AddRange([.. entries.OrderBy(e => order.IndexOf(e.Name)).Select(CreateWadButton)]);
            ApplyFilter();

            flowLayoutPanel1.ResumeLayout();
            loadingPanel.Visible = false;
        }

        private Button CreateWadButton(WadEntry entry)
        {
            Button boton = new()
            {
                Width = _iconsSettings.Width + 10,
                Height = _iconsSettings.Height + 10,
                Tag = entry.File,
                Name = entry.Name,
                Margin = new Padding(_iconsSettings.Padding),
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                Cursor = Cursors.Hand,
            };

            boton.FlatAppearance.MouseOverBackColor = Theme.WadButtonHover;
            ApplyFavoriteStyle(boton);
            boton.ContextMenuStrip = BuildWadContextMenu(boton);

            if (entry.Icon is not null)
            {
                boton.Image = entry.Icon;
                boton.ImageAlign = ContentAlignment.MiddleCenter;
            }
            else
            {
                boton.Text = entry.Name;
                boton.Font = _wadButtonFont;
            }

            boton.Click += Boton_Click!;
            EnableArchiveDrop(boton);
            return boton;
        }

        // Filters the already loaded buttons in place, so searching never touches the disk.
        private void ApplyFilter()
        {
            var filter = txtSearch.Text;
            int count = 0;

            flowLayoutPanel1.SuspendLayout();
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                control.Visible = control.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
                if (control.Visible)
                    count++;
            }
            flowLayoutPanel1.ResumeLayout();

            var loaded = string.Format(count == 1 ? Strings.StatusWadCountOne : Strings.StatusWadCountMany, count);
            lblStatus.Text = $"  {loaded}   |   {_folders.Batches.Path}";
        }

        private void ApplyFavoriteStyle(Button boton)
        {
            var favorite = _library.IsFavorite(boton.Name);
            boton.FlatAppearance.BorderColor = favorite ? Theme.Favorite : Theme.Accent;
            boton.FlatAppearance.BorderSize = favorite ? 2 : 1;
            _wadTooltip.SetToolTip(boton, favorite ? $"★ {boton.Name}" : boton.Name);
        }

        // Reorders the buttons already shown (after a favorite changes, a WAD is played or the sort
        // changes), so their icons don't have to be loaded again.
        private void ApplyOrder()
        {
            var buttons = flowLayoutPanel1.Controls.Cast<Control>().ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var order = _library.Sort(buttons.Keys);

            flowLayoutPanel1.SuspendLayout();
            for (var i = 0; i < order.Count; i++)
                flowLayoutPanel1.Controls.SetChildIndex(buttons[order[i]], i);
            flowLayoutPanel1.ResumeLayout();
        }

        private void UpdateSortMenu()
        {
            sortByNameToolStripMenuItem.Checked = _library.SortOrder == WadSortOrder.Name;
            sortByLastPlayedToolStripMenuItem.Checked = _library.SortOrder == WadSortOrder.LastPlayed;
        }

        private void SortByNameToolStripMenuItem_Click(object sender, EventArgs e) => SetSortOrder(WadSortOrder.Name);

        private void SortByLastPlayedToolStripMenuItem_Click(object sender, EventArgs e) => SetSortOrder(WadSortOrder.LastPlayed);

        private void SetSortOrder(WadSortOrder order)
        {
            // Reordered even if saving fails, so the choice still applies until the app closes.
            TrySaveLibrary(() => _library.SetSortOrder(order));
            UpdateSortMenu();
            ApplyOrder();
        }

        private void ToggleFavorite(Button boton)
        {
            TrySaveLibrary(() => _library.SetFavorite(boton.Name, !_library.IsFavorite(boton.Name)));
            ApplyFavoriteStyle(boton);
            ApplyOrder();
        }

        // The library changes in memory before it's written, so a failed save only loses it on restart.
        private static void TrySaveLibrary(Action change)
        {
            try
            {
                change();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(string.Format(Strings.LibrarySaveError, ex.Message), Strings.ErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private ContextMenuStrip BuildWadContextMenu(Button boton)
        {
            var filePath = (string)boton.Tag!;
            var menu = new ContextMenuStrip { Renderer = new DarkMenuRenderer() };

            var favoriteItem = new ToolStripMenuItem(Strings.MenuFavorite);
            favoriteItem.Click += (s, e) => ToggleFavorite(boton);
            // Checked when it opens, since the same menu stays with the button while favorites change.
            menu.Opening += (s, e) => favoriteItem.Checked = _library.IsFavorite(boton.Name);

            var editItem = new ToolStripMenuItem(Strings.MenuEdit);
            editItem.Click += (s, e) => EditBatch(filePath);

            var changeImageItem = new ToolStripMenuItem(Strings.MenuChangeImage);
            changeImageItem.Click += (s, e) => ChangeImage(filePath);

            var openFolderItem = new ToolStripMenuItem(Strings.MenuOpenWadFolder);
            openFolderItem.Click += (s, e) => OpenWadFolder(filePath);

            var duplicateItem = new ToolStripMenuItem(Strings.MenuDuplicate);
            duplicateItem.Click += (s, e) => DuplicateBatch(filePath);

            var deleteItem = new ToolStripMenuItem(Strings.MenuDelete);
            deleteItem.Click += (s, e) => DeleteBatch(filePath);

            menu.Items.Add(favoriteItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(editItem);
            menu.Items.Add(changeImageItem);
            menu.Items.Add(openFolderItem);
            menu.Items.Add(duplicateItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(deleteItem);

            return menu;
        }

        private async void EditBatch(string filePath)
        {
            using var form = new BatchCreatorForm(filePath);

            if (form.ShowDialog() != DialogResult.OK)
                return;

            // The library knows WADs by name, so a renamed one keeps its favorite and history.
            if (form.Renamed is var (oldName, newName))
                TrySaveLibrary(() => _library.Rename(oldName, newName));

            await LoadButtonsBatchAsync();
        }

        // Like the batch form does with a picked image, this copies it, so replacing an icon never
        // takes a file away from where the user keeps it.
        private async void ChangeImage(string filePath)
        {
            var images = _folders.Images;

            if (string.IsNullOrWhiteSpace(images.Path))
            {
                MessageBox.Show(Strings.ImagesDirNotConfigured, Strings.ChangeImageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(images.Path))
            {
                MessageBox.Show(string.Format(Strings.ImagesDirNotFoundFixInSettings, images.Path), Strings.ChangeImageTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog { Title = Strings.SelectImage, Filter = ImageFileService.FileFilter };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            if (!ImageFileService.IsValidImage(dlg.FileName))
            {
                MessageBox.Show(Strings.InvalidImageSelected, Strings.ChangeImageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ImageFileService.Save(dlg.FileName, BatchFileService.GetImagePath(filePath, images), move: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.ChangeImageError, ex.Message), Strings.ChangeImageTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await LoadButtonsBatchAsync();
        }

        // Opens Explorer with the batch's WAD selected.
        private void OpenWadFolder(string filePath)
        {
            string command;

            try
            {
                command = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.CantReadBatch, ex.Message), Strings.OpenWadFolderTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!BatchFileService.TryParseCommand(command, _ports.GZDoom.Plugins, out var port, out _, out var wad, out _, out _, out _))
            {
                MessageBox.Show(Strings.WadNotInBatch, Strings.OpenWadFolderTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var executable = port == SourcePort.DSDADoom ? _ports.DSDADoom.Path : _ports.GZDoom.Path;
            var path = BatchFileService.ResolveWadPath(wad, executable);

            if (path is null || !File.Exists(path))
            {
                MessageBox.Show(string.Format(Strings.WadFileNotFound, path ?? BatchFileService.EnsureWadExtension(wad)),
                    Strings.OpenWadFolderTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Batch paths can't contain quotes, so quoting it is enough.
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.CantExecuteFile, ex.Message), Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Copies the batch, and its icon if it has one, under a new name, e.g. to try the same WAD
        // with another plugin or source port.
        private async void DuplicateBatch(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath)!;
            var extension = Path.GetExtension(filePath);
            var name = BatchFileService.GetDuplicateName(Path.GetFileNameWithoutExtension(filePath),
                n => File.Exists(Path.Combine(directory, n + extension)));
            string newPath;

            while (true)
            {
                using var dlg = new NamePromptForm(Strings.DuplicateBatchTitle, Strings.DuplicateBatchPrompt, name);

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                // Windows drops trailing dots and spaces from file names, so do it up front.
                name = dlg.EnteredName.TrimEnd('.', ' ');

                if (!BatchFileService.IsValidFileName(name, out var error))
                {
                    MessageBox.Show(error, Strings.DuplicateBatchTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                newPath = Path.Combine(directory, name + extension);

                if (!File.Exists(newPath))
                    break;

                MessageBox.Show(string.Format(Strings.BatchNameTaken, name), Strings.DuplicateBatchTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            try
            {
                File.Copy(filePath, newPath);

                // An image left over with the new name would become its icon anyway, so the copy replaces it.
                var imagePath = BatchFileService.GetImagePath(filePath, _folders.Images);
                if (File.Exists(imagePath))
                    File.Copy(imagePath, BatchFileService.GetImagePath(newPath, _folders.Images), overwrite: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.DuplicateBatchError, ex.Message), Strings.DuplicateBatchTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            await LoadButtonsBatchAsync();
        }

        private async void DeleteBatch(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            var imagePath = BatchFileService.GetImagePath(filePath, _folders.Images);

            var confirm = MessageBox.Show(
                string.Format(Strings.DeleteBatchConfirm, name),
                Strings.DeleteBatchTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                File.Delete(filePath);

                if (File.Exists(imagePath))
                    File.Delete(imagePath);

                // A batch made later with the same name starts without this one's history.
                try
                {
                    _library.Forget(name);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                }

                await LoadButtonsBatchAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.CantDeleteFile, ex.Message), Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Boton_Click(object sender, EventArgs e)
        {
            if (sender is not Button boton || boton.Tag is not string path)
                return;

            var alreadyRunning = "  " + string.Format(Strings.StatusAlreadyRunning, boton.Name);

            if (!_runningBatches.Add(path))
            {
                lblStatus.Text = alreadyRunning;
                return;
            }

            try
            {
                await LaunchAsync(boton.Name, path);
            }
            finally
            {
                _runningBatches.Remove(path);

                // The notice was about this game, which isn't running anymore.
                if (!IsDisposed && lblStatus.Text == alreadyRunning)
                    ApplyFilter();
            }
        }

        private async Task LaunchAsync(string name, string path)
        {
            // Recorded before launching, since RunAsync only returns once the game is closed.
            // Not being able to save it is no reason to keep the user from playing.
            try
            {
                _library.MarkPlayed(name, DateTime.UtcNow);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }

            if (_library.SortOrder == WadSortOrder.LastPlayed)
                ApplyOrder();

            string? errors;

            try
            {
                errors = await BatchLaunchService.RunAsync(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.CantExecuteFile, ex.Message), Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // The console is hidden, so whatever went wrong would otherwise go unseen.
            if (errors is not null && !IsDisposed)
                MessageBox.Show(string.Format(Strings.BatchFailed, Path.GetFileNameWithoutExtension(path), errors),
                    Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private async void AddBatchFileToolStripMenuItem_Click(object sender, EventArgs e) =>
            await AddBatchAsync(new BatchCreatorForm());

        private async Task AddBatchAsync(BatchCreatorForm form)
        {
            using (form)
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return;
            }

            await LoadButtonsBatchAsync();
        }

        // Dropping a downloaded .zip/.rar/.7z anywhere on the window opens Add Batch File
        // with it already extracted, like "From archive...". Drag events only reach the control
        // under the cursor, so the WAD buttons take drops too.
        private void EnableArchiveDrop(Control control)
        {
            control.AllowDrop = true;
            control.DragEnter += Archive_DragEnter;
            control.DragDrop += Archive_DragDrop;
        }

        // Only a single archive is taken; anything else isn't something to make one batch from.
        private static string? DroppedArchive(DragEventArgs e) =>
            e.Data?.GetData(DataFormats.FileDrop) is string[] { Length: 1 } paths &&
            ArchiveService.IsArchive(paths[0]) && File.Exists(paths[0])
                ? paths[0]
                : null;

        private void Archive_DragEnter(object? sender, DragEventArgs e) =>
            e.Effect = DroppedArchive(e) is not null ? DragDropEffects.Copy : DragDropEffects.None;

        private void Archive_DragDrop(object? sender, DragEventArgs e)
        {
            if (DroppedArchive(e) is not { } archive)
                return;

            // Explorer waits for the drop to return, so the form opens after it instead of during it.
            BeginInvoke(async () => await AddBatchAsync(BatchCreatorForm.FromArchive(archive)));
        }

        private async void RefreshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await LoadButtonsBatchAsync();
        }

        private async void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new SettingsForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                // The language may have changed; the buttons' menus pick it up when they're rebuilt.
                Localization.Apply(AppConfiguration.Interface.Language);
                ApplyTexts();
                LoadConfiguration();
                await LoadButtonsBatchAsync();
            }
        }

        private void SearchWad(object sender, EventArgs e)
        {
            ApplyFilter();
        }
    }
}
