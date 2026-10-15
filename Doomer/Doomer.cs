using Doomer.Options;
using Doomer.Services;
using System.Diagnostics;

namespace Doomer
{
    public partial class Doomer : Form
    {
        private static readonly Color BgColor = Color.FromArgb(18, 18, 18);
        private static readonly Color SurfaceColor = Color.FromArgb(30, 30, 30);
        private static readonly Color AccentColor = Color.FromArgb(160, 15, 15);
        private static readonly Color ButtonBgColor = Color.FromArgb(38, 38, 38);
        private static readonly Color ButtonHoverColor = Color.FromArgb(65, 10, 10);
        private static readonly Color TextColor = Color.FromArgb(220, 220, 220);
        private static readonly Color MutedColor = Color.FromArgb(140, 140, 140);

        private GZDoomSettings _gzdoomSettings = default!;
        private IconsSettings _iconsSettings = default!;
        private readonly ToolTip _wadTooltip = new();
        private readonly Font _wadButtonFont = new("Segoe UI", 8.5f);
        private CancellationTokenSource? _loadCts;

        public Doomer()
        {
            InitializeComponent();
            components?.Add(_wadTooltip);
            ApplyTexts();
            lblVersion.Text = $"v{UpdateService.CurrentVersion}";
            LoadConfiguration();
            ApplyDarkTheme();
        }

        private void LoadConfiguration()
        {
            _gzdoomSettings = AppConfiguration.GZDoom;
            _iconsSettings = AppConfiguration.Icons;
        }

        private void ApplyDarkTheme()
        {
            BackColor = BgColor;
            ForeColor = TextColor;

            menuStrip1.BackColor = SurfaceColor;
            menuStrip1.ForeColor = TextColor;
            menuStrip1.Renderer = new DarkMenuRenderer();

            foreach (ToolStripItem item in menuStrip1.Items)
                item.ForeColor = TextColor;

            txtSearch.BackColor = Color.FromArgb(48, 48, 48);
            txtSearch.ForeColor = TextColor;

            flowLayoutPanel1.BackColor = BgColor;

            statusStrip1.BackColor = SurfaceColor;
            statusStrip1.Renderer = new DarkMenuRenderer();
            lblStatus.ForeColor = MutedColor;

            loadingPanel.BackColor = BgColor;
            lblLoading.ForeColor = MutedColor;
            lblVersion.ForeColor = MutedColor;
        }

        private void ApplyTexts()
        {
            addBatchFileToolStripMenuItem.Text = Strings.MenuAddBatch;
            refreshListToolStripMenuItem.Text = Strings.MenuRefresh;
            settingsToolStripMenuItem.ToolTipText = Strings.MenuSettings;
            settingsToolStripMenuItem.AccessibleName = Strings.MenuSettings;
            txtSearch.TextBox.PlaceholderText = Strings.SearchPlaceholder;
            lblLoading.Text = Strings.LoadingWads;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await LoadButtonsBatchAsync();
            await CheckForUpdateAsync();
        }

        private async Task CheckForUpdateAsync()
        {
            var update = await UpdateService.CheckForUpdateAsync();
            if (update is null)
                return;

            var answer = MessageBox.Show(
                string.Format(Strings.UpdateAvailableMessage, update.Version, UpdateService.CurrentVersion),
                Strings.UpdateAvailableTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (answer != DialogResult.Yes)
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

            var batchs = _gzdoomSettings.Batchs;
            var images = _gzdoomSettings.Images;
            var iconSize = new Size(_iconsSettings.Width, _iconsSettings.Height);

            List<WadEntry> entries;
            try
            {
                entries = await Task.Run(() => ReadWads(batchs, images, iconSize, cts.Token), cts.Token);
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

        private static List<WadEntry> ReadWads(BatchSettings batchs, ImageSettings images, Size iconSize, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(batchs.Location))
                return [];

            var entries = new List<WadEntry>();
            try
            {
                foreach (var file in Directory.GetFiles(batchs.Location, $"*{batchs.Extension}"))
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
            flowLayoutPanel1.Controls.AddRange([.. entries.Select(CreateWadButton)]);
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
                BackColor = ButtonBgColor,
                ForeColor = TextColor,
                Cursor = Cursors.Hand,
            };

            boton.FlatAppearance.BorderColor = AccentColor;
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.MouseOverBackColor = ButtonHoverColor;

            _wadTooltip.SetToolTip(boton, entry.Name);
            boton.ContextMenuStrip = BuildWadContextMenu(entry.File);

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
            lblStatus.Text = $"  {loaded}   |   {_gzdoomSettings.Batchs.Location}";
        }

        private ContextMenuStrip BuildWadContextMenu(string filePath)
        {
            var menu = new ContextMenuStrip();

            var editItem = new ToolStripMenuItem(Strings.MenuEdit);
            editItem.Click += (s, e) => EditBatch(filePath);

            var deleteItem = new ToolStripMenuItem(Strings.MenuDelete);
            deleteItem.Click += (s, e) => DeleteBatch(filePath);

            menu.Items.Add(editItem);
            menu.Items.Add(deleteItem);

            return menu;
        }

        private async void EditBatch(string filePath)
        {
            using var form = new BatchCreatorForm(filePath);

            if (form.ShowDialog() == DialogResult.OK)
            {
                await LoadButtonsBatchAsync();
            }
        }

        private async void DeleteBatch(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            var imagePath = BatchFileService.GetImagePath(filePath, _gzdoomSettings.Images);

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

                await LoadButtonsBatchAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.CantDeleteFile, ex.Message), Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Boton_Click(object sender, EventArgs e)
        {
            if (sender is Button boton && boton.Tag is string path)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Strings.CantExecuteFile, ex.Message), Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void AddBatchFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new BatchCreatorForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                await LoadButtonsBatchAsync();
            }
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
                Localization.Apply(AppConfiguration.General.Language);
                ApplyTexts();
                LoadConfiguration();
                await LoadButtonsBatchAsync();
            }
        }

        private void SearchWad(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            private static readonly Color Bg = Color.FromArgb(30, 30, 30);
            private static readonly Color Accent = Color.FromArgb(140, 15, 15);
            private static readonly Color Border = Color.FromArgb(55, 55, 55);

            public override Color MenuItemSelected => Accent;
            public override Color MenuItemBorder => Accent;
            public override Color MenuBorder => Border;
            public override Color MenuItemSelectedGradientBegin => Accent;
            public override Color MenuItemSelectedGradientEnd => Accent;
            public override Color MenuItemPressedGradientBegin => Accent;
            public override Color MenuItemPressedGradientEnd => Accent;
            public override Color MenuItemPressedGradientMiddle => Accent;
            public override Color ToolStripDropDownBackground => Color.FromArgb(28, 28, 28);
            public override Color ImageMarginGradientBegin => Color.FromArgb(28, 28, 28);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(28, 28, 28);
            public override Color ImageMarginGradientEnd => Color.FromArgb(28, 28, 28);
            public override Color MenuStripGradientBegin => Bg;
            public override Color MenuStripGradientEnd => Bg;
            public override Color ToolStripGradientBegin => Bg;
            public override Color ToolStripGradientMiddle => Bg;
            public override Color ToolStripGradientEnd => Bg;
            public override Color SeparatorLight => Border;
            public override Color SeparatorDark => Border;
            public override Color CheckBackground => Accent;
            public override Color CheckSelectedBackground => Accent;
            public override Color CheckPressedBackground => Accent;
            public override Color ButtonSelectedHighlight => Accent;
            public override Color ButtonPressedHighlight => Accent;
            public override Color ButtonCheckedHighlight => Accent;
            public override Color StatusStripGradientBegin => Bg;
            public override Color StatusStripGradientEnd => Bg;
        }

        private class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColorTable())
            {
                RoundedEdges = false;
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = Color.FromArgb(220, 220, 220);
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
        }
    }
}
