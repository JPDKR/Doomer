using Doomer.Options;
using System.Text.Json;

namespace Doomer
{
    public partial class SettingsForm : Form
    {
        private static readonly Color BgColor = Color.FromArgb(18, 18, 18);
        private static readonly Color SurfaceColor = Color.FromArgb(38, 38, 38);
        private static readonly Color TextColor = Color.FromArgb(220, 220, 220);
        private static readonly Color MutedColor = Color.FromArgb(170, 170, 170);
        private static readonly Color AccentColor = Color.FromArgb(160, 15, 15);
        private static readonly Color AccentHoverColor = Color.FromArgb(200, 25, 25);
        private static readonly Color NeutralBtnColor = Color.FromArgb(50, 50, 50);
        private static readonly Color NeutralBtnHoverColor = Color.FromArgb(70, 70, 70);
        private static readonly Color NeutralBorderColor = Color.FromArgb(75, 75, 75);

        public SettingsForm()
        {
            InitializeComponent();
            ApplyDarkTheme();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var gzdoom = AppConfiguration.GZDoom;
            var icons = AppConfiguration.Icons;

            txtGZDoomLocation.Text = gzdoom.Location;
            txtGZDoomPlugins.Text = gzdoom.Plugins;
            txtBatchsLocation.Text = gzdoom.Batchs.Location;
            txtBatchsExtension.Text = gzdoom.Batchs.Extension;
            txtImagesLocation.Text = gzdoom.Images.Location;
            txtImagesExtension.Text = gzdoom.Images.Extension;

            nudWidth.Value = Math.Clamp(icons.Width, (int)nudWidth.Minimum, (int)nudWidth.Maximum);
            nudHeight.Value = Math.Clamp(icons.Height, (int)nudHeight.Minimum, (int)nudHeight.Maximum);
            nudPadding.Value = Math.Clamp(icons.Padding, (int)nudPadding.Minimum, (int)nudPadding.Maximum);
        }

        private void SaveSettings()
        {
            var root = new AppSettingsRoot
            {
                GZDoom = new GZDoomSettings
                {
                    Location = txtGZDoomLocation.Text.Trim(),
                    Plugins = txtGZDoomPlugins.Text.Trim(),
                    Batchs = new BatchSettings
                    {
                        Location = txtBatchsLocation.Text.Trim(),
                        Extension = txtBatchsExtension.Text.Trim()
                    },
                    Images = new ImageSettings
                    {
                        Location = txtImagesLocation.Text.Trim(),
                        Extension = txtImagesExtension.Text.Trim()
                    }
                },
                Icons = new IconsSettings
                {
                    Width = (int)nudWidth.Value,
                    Height = (int)nudHeight.Value,
                    Padding = (int)nudPadding.Value
                }
            };

            var json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(AppConfiguration.FilePath, json);
            AppConfiguration.Load();
        }

        private void ApplyDarkTheme()
        {
            BackColor = BgColor;
            ForeColor = TextColor;
            StyleControls(Controls);
        }

        private void StyleControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                switch (ctrl)
                {
                    case GroupBox gb:
                        gb.BackColor = BgColor;
                        gb.ForeColor = MutedColor;
                        StyleControls(gb.Controls);
                        break;
                    case TextBox tb:
                        tb.BackColor = SurfaceColor;
                        tb.ForeColor = TextColor;
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case NumericUpDown nud:
                        nud.BackColor = SurfaceColor;
                        nud.ForeColor = TextColor;
                        break;
                    case Label lbl:
                        lbl.ForeColor = MutedColor;
                        break;
                    case Button btn when btn.Name == "btnSave":
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = AccentColor;
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = AccentHoverColor;
                        btn.FlatAppearance.MouseOverBackColor = AccentHoverColor;
                        btn.Cursor = Cursors.Hand;
                        break;
                    case Button btn:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = NeutralBtnColor;
                        btn.ForeColor = TextColor;
                        btn.FlatAppearance.BorderColor = NeutralBorderColor;
                        btn.FlatAppearance.MouseOverBackColor = NeutralBtnHoverColor;
                        btn.Cursor = Cursors.Hand;
                        break;
                }
            }
        }

        private void BtnBrowseExe_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select GZDoom executable",
                Filter = "GZDoom|gzdoom.exe|Executables|*.exe|All files|*.*"
            };

            var dir = Path.GetDirectoryName(txtGZDoomLocation.Text);
            if (Directory.Exists(dir))
                dlg.InitialDirectory = dir;

            if (dlg.ShowDialog() == DialogResult.OK)
                txtGZDoomLocation.Text = dlg.FileName;
        }

        private void BtnBrowseBatchs_Click(object sender, EventArgs e)
        {
            BrowseFolder(txtBatchsLocation, "Select batch files directory");
        }

        private void BtnBrowseImages_Click(object sender, EventArgs e)
        {
            BrowseFolder(txtImagesLocation, "Select images directory");
        }

        private void BrowseFolder(TextBox target, string description)
        {
            using var dlg = new FolderBrowserDialog { Description = description, UseDescriptionForTitle = true };
            if (Directory.Exists(target.Text))
                dlg.InitialDirectory = target.Text;
            if (dlg.ShowDialog() == DialogResult.OK)
                target.Text = dlg.SelectedPath;
        }

        private static bool ExecutableExists(string path) =>
            File.Exists(path) || File.Exists(path + ".exe");

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var gzdoomLocation = txtGZDoomLocation.Text.Trim();
            var batchsLocation = txtBatchsLocation.Text.Trim();
            var imagesLocation = txtImagesLocation.Text.Trim();

            if (string.IsNullOrWhiteSpace(gzdoomLocation) || string.IsNullOrWhiteSpace(batchsLocation))
            {
                MessageBox.Show("GZDoom executable and batch directory are required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ExecutableExists(gzdoomLocation))
            {
                MessageBox.Show($"GZDoom executable not found:\n{gzdoomLocation}", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(batchsLocation))
            {
                MessageBox.Show($"Batch files directory not found:\n{batchsLocation}", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(imagesLocation) && !Directory.Exists(imagesLocation))
            {
                var proceed = MessageBox.Show(
                    $"Images directory not found:\n{imagesLocation}\n\nWAD buttons will show text instead of icons. Save anyway?",
                    "Validation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (proceed != DialogResult.Yes)
                    return;
            }

            try
            {
                SaveSettings();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
