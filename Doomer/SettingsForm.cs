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
            ApplyTexts();
            ApplyDarkTheme();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var gzdoom = AppConfiguration.GZDoom;
            var icons = AppConfiguration.Icons;

            txtGZDoomLocation.Text = gzdoom.Location;
            txtGZDoomPlugins.Text = gzdoom.Plugins;
            txtDSDADoomLocation.Text = AppConfiguration.DSDADoom.Location;
            txtBatchsLocation.Text = gzdoom.Batchs.Location;
            txtBatchsExtension.Text = gzdoom.Batchs.Extension;
            txtImagesLocation.Text = gzdoom.Images.Location;
            txtImagesExtension.Text = gzdoom.Images.Extension;
            txtWadsLocation.Text = gzdoom.Wads.Location;

            nudWidth.Value = Math.Clamp(icons.Width, (int)nudWidth.Minimum, (int)nudWidth.Maximum);
            nudHeight.Value = Math.Clamp(icons.Height, (int)nudHeight.Minimum, (int)nudHeight.Maximum);
            nudPadding.Value = Math.Clamp(icons.Padding, (int)nudPadding.Minimum, (int)nudPadding.Maximum);

            cmbLanguage.Items.Add(Strings.LanguageSystem);
            foreach (var (_, name) in Localization.Languages)
                cmbLanguage.Items.Add(name);

            var languageIndex = Localization.Languages
                .Select(l => l.Culture)
                .ToList()
                .FindIndex(c => string.Equals(c, AppConfiguration.General.Language, StringComparison.OrdinalIgnoreCase));
            cmbLanguage.SelectedIndex = languageIndex + 1;
        }

        // Index 0 is "system default", saved as an empty language.
        private string SelectedLanguage =>
            cmbLanguage.SelectedIndex > 0 ? Localization.Languages[cmbLanguage.SelectedIndex - 1].Culture : string.Empty;

        private void ApplyTexts()
        {
            Text = Strings.SettingsTitle;
            lblGZDoomExe.Text = lblDSDADoomExe.Text = Strings.LabelExecutable;
            lblGZDoomPlugins.Text = Strings.LabelPluginsFolder;
            grpBatchs.Text = Strings.GroupBatchFiles;
            grpImages.Text = Strings.GroupImages;
            grpWads.Text = Strings.GroupWads;
            lblBatchsLocation.Text = lblImagesLocation.Text = lblWadsLocation.Text = Strings.LabelDirectory;
            lblBatchsExtension.Text = lblImagesExtension.Text = Strings.LabelExtension;
            grpIcons.Text = Strings.GroupIcons;
            lblIconsWidth.Text = Strings.LabelWidth;
            lblIconsHeight.Text = Strings.LabelHeight;
            lblIconsPadding.Text = Strings.LabelPadding;
            grpInterface.Text = Strings.GroupInterface;
            lblLanguage.Text = Strings.LabelLanguage;
            btnBrowseExe.Text = btnBrowseDSDAExe.Text = btnBrowseBatchs.Text =
                btnBrowseImages.Text = btnBrowseWads.Text = Strings.BrowseButton;
            btnCancel.Text = Strings.CancelButton;
            btnSave.Text = Strings.SaveButton;
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
                    },
                    Wads = new WadSettings
                    {
                        Location = txtWadsLocation.Text.Trim()
                    }
                },
                DSDADoom = new DSDADoomSettings
                {
                    Location = txtDSDADoomLocation.Text.Trim()
                },
                Icons = new IconsSettings
                {
                    Width = (int)nudWidth.Value,
                    Height = (int)nudHeight.Value,
                    Padding = (int)nudPadding.Value
                },
                General = new GeneralSettings
                {
                    Language = SelectedLanguage
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
                    case ComboBox cb:
                        cb.FlatStyle = FlatStyle.Flat;
                        cb.BackColor = SurfaceColor;
                        cb.ForeColor = TextColor;
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
            BrowseExecutable(txtGZDoomLocation, string.Format(Strings.SelectExecutable, "GZDoom"), "GZDoom|gzdoom.exe");
        }

        private void BtnBrowseDSDAExe_Click(object sender, EventArgs e)
        {
            BrowseExecutable(txtDSDADoomLocation, string.Format(Strings.SelectExecutable, "DSDA Doom"), "DSDA Doom|dsda-doom.exe");
        }

        private static void BrowseExecutable(TextBox target, string title, string filter)
        {
            using var dlg = new OpenFileDialog
            {
                Title = title,
                Filter = $"{filter}|{Strings.FileFilterExecutables}|*.exe|{Strings.FileFilterAll}|*.*"
            };

            var dir = Path.GetDirectoryName(target.Text);
            if (Directory.Exists(dir))
                dlg.InitialDirectory = dir;

            if (dlg.ShowDialog() == DialogResult.OK)
                target.Text = dlg.FileName;
        }

        private void BtnBrowseBatchs_Click(object sender, EventArgs e)
        {
            BrowseFolder(txtBatchsLocation, Strings.SelectBatchsDir);
        }

        private void BtnBrowseImages_Click(object sender, EventArgs e)
        {
            BrowseFolder(txtImagesLocation, Strings.SelectImagesDir);
        }

        private void BtnBrowseWads_Click(object sender, EventArgs e)
        {
            BrowseFolder(txtWadsLocation, Strings.SelectWadsDir);
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
            var dsdaDoomLocation = txtDSDADoomLocation.Text.Trim();
            var batchsLocation = txtBatchsLocation.Text.Trim();
            var imagesLocation = txtImagesLocation.Text.Trim();
            var wadsLocation = txtWadsLocation.Text.Trim();

            if ((string.IsNullOrWhiteSpace(gzdoomLocation) && string.IsNullOrWhiteSpace(dsdaDoomLocation)) ||
                string.IsNullOrWhiteSpace(batchsLocation))
            {
                MessageBox.Show(Strings.SettingsRequired, Strings.ValidationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(dsdaDoomLocation) && !ExecutableExists(dsdaDoomLocation))
            {
                MessageBox.Show(string.Format(Strings.ExecutableNotFound, "DSDA Doom", dsdaDoomLocation), Strings.ValidationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(gzdoomLocation) && !ExecutableExists(gzdoomLocation))
            {
                MessageBox.Show(string.Format(Strings.ExecutableNotFound, "GZDoom", gzdoomLocation), Strings.ValidationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(batchsLocation))
            {
                MessageBox.Show(string.Format(Strings.BatchsDirNotFound, batchsLocation), Strings.ValidationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(wadsLocation) && !Directory.Exists(wadsLocation))
            {
                MessageBox.Show(string.Format(Strings.WadsDirNotFound, wadsLocation), Strings.ValidationTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(imagesLocation) && !Directory.Exists(imagesLocation))
            {
                var proceed = MessageBox.Show(
                    string.Format(Strings.ImagesDirNotFoundSaveAnyway, imagesLocation),
                    Strings.ValidationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

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
                MessageBox.Show(string.Format(Strings.SettingsSaveError, ex.Message), Strings.ErrorTitle,
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
