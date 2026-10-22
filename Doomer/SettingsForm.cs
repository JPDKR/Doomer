using Doomer.Options;
using Doomer.Services;

namespace Doomer
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            ApplyTexts();
            Theme.ApplyToDialog(this, btnSave);
            LoadSettings();
        }

        private void LoadSettings()
        {
            var ports = AppConfiguration.Ports;
            var folders = AppConfiguration.Folders;
            var icons = AppConfiguration.Interface.Icons;

            txtGZDoomLocation.Text = ports.GZDoom.Path;
            txtGZDoomPlugins.Text = ports.GZDoom.Plugins;
            txtDSDADoomLocation.Text = ports.DSDADoom.Path;
            txtBatchsLocation.Text = folders.Batches.Path;
            txtBatchsExtension.Text = folders.Batches.Extension;
            txtImagesLocation.Text = folders.Images.Path;
            txtImagesExtension.Text = folders.Images.Extension;
            txtWadsLocation.Text = folders.Wads.Path;

            nudWidth.Value = Math.Clamp(icons.Width, (int)nudWidth.Minimum, (int)nudWidth.Maximum);
            nudHeight.Value = Math.Clamp(icons.Height, (int)nudHeight.Minimum, (int)nudHeight.Maximum);
            nudPadding.Value = Math.Clamp(icons.Padding, (int)nudPadding.Minimum, (int)nudPadding.Maximum);

            cmbLanguage.Items.Add(Strings.LanguageSystem);
            foreach (var (_, name) in Localization.Languages)
                cmbLanguage.Items.Add(name);

            var languageIndex = Localization.Languages
                .Select(l => l.Culture)
                .ToList()
                .FindIndex(c => string.Equals(c, AppConfiguration.Interface.Language, StringComparison.OrdinalIgnoreCase));
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
                Ports = new PortsSettings
                {
                    GZDoom = new GZDoomSettings
                    {
                        Path = txtGZDoomLocation.Text.Trim(),
                        Plugins = txtGZDoomPlugins.Text.Trim()
                    },
                    DSDADoom = new DSDADoomSettings
                    {
                        Path = txtDSDADoomLocation.Text.Trim()
                    }
                },
                Folders = new FoldersSettings
                {
                    Batches = new BatchSettings
                    {
                        Path = txtBatchsLocation.Text.Trim(),
                        Extension = txtBatchsExtension.Text.Trim()
                    },
                    Images = new ImageSettings
                    {
                        Path = txtImagesLocation.Text.Trim(),
                        Extension = txtImagesExtension.Text.Trim()
                    },
                    Wads = new WadSettings
                    {
                        Path = txtWadsLocation.Text.Trim()
                    }
                },
                Interface = new InterfaceSettings
                {
                    Language = SelectedLanguage,
                    Icons = new IconsSettings
                    {
                        Width = (int)nudWidth.Value,
                        Height = (int)nudHeight.Value,
                        Padding = (int)nudPadding.Value
                    }
                }
            };

            AtomicFile.WriteAllText(AppConfiguration.FilePath, SettingsFile.Serialize(root));
            AppConfiguration.Load();
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

        // Shows the extension as it will be saved (e.g. "bat" becomes ".bat"), or says why it can't be used.
        private static bool NormalizeExtension(TextBox textBox, string group, string example)
        {
            if (SettingsFile.TryNormalizeExtension(textBox.Text, out var extension))
            {
                textBox.Text = extension;
                return true;
            }

            MessageBox.Show(string.Format(Strings.ExtensionInvalid, group, example), Strings.ValidationTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            textBox.Focus();
            return false;
        }

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

            if (!NormalizeExtension(txtBatchsExtension, Strings.GroupBatchFiles, new BatchSettings().Extension) ||
                !NormalizeExtension(txtImagesExtension, Strings.GroupImages, new ImageSettings().Extension))
                return;

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
