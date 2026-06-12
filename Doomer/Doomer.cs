using Doomer.Options;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;

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

        private readonly GZDoomSettings _gzdoomSettings;
        private readonly IconsSettings _iconsSettings;

        public Doomer()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _gzdoomSettings = config.GetSection("GZDoom").Get<GZDoomSettings>()!;
            _iconsSettings = config.GetSection("Icons").Get<IconsSettings>()!;

            ApplyDarkTheme();
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
            txtSearch.TextBox.PlaceholderText = "Buscar WAD...";

            flowLayoutPanel1.BackColor = BgColor;

            statusStrip1.BackColor = SurfaceColor;
            statusStrip1.Renderer = new DarkMenuRenderer();
            lblStatus.ForeColor = MutedColor;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadButtonsBatch();
        }

        private void LoadButtonsBatch(string filter = "")
        {
            flowLayoutPanel1.Controls.Clear();

            string[] files;

            try
            {
                files = [.. Directory
                .GetFiles(_gzdoomSettings.Batchs.Location, $"*{_gzdoomSettings.Batchs.Extension}")
                .Where(f =>
                    Path.GetFileNameWithoutExtension(f)
                        .Contains(filter, StringComparison.OrdinalIgnoreCase))];
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show($"Batchs directory not found:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            foreach (var file in files)
            {
                string baseName = Path.GetFileNameWithoutExtension(file);
                string iconPath = Path.Combine(_gzdoomSettings.Images.Location, baseName + _gzdoomSettings.Images.Extension);

                Button boton = new()
                {
                    Width = _iconsSettings.Width + 10,
                    Height = _iconsSettings.Height + 10,
                    Tag = file,
                    Margin = new Padding(_iconsSettings.Padding),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ButtonBgColor,
                    ForeColor = TextColor,
                    Cursor = Cursors.Hand,
                };

                boton.FlatAppearance.BorderColor = AccentColor;
                boton.FlatAppearance.BorderSize = 1;
                boton.FlatAppearance.MouseOverBackColor = ButtonHoverColor;

                ToolTip tooltip = new();
                tooltip.SetToolTip(boton, baseName);

                if (File.Exists(iconPath))
                {
                    try
                    {
                        Image img = Image.FromFile(iconPath);
                        boton.Image = new Bitmap(img, new Size(_iconsSettings.Width, _iconsSettings.Height));
                        boton.ImageAlign = ContentAlignment.MiddleCenter;
                    }
                    catch
                    {
                        boton.Text = baseName;
                        boton.Font = new Font("Segoe UI", 8.5f);
                    }
                }
                else
                {
                    boton.Text = baseName;
                    boton.Font = new Font("Segoe UI", 8.5f);
                }

                boton.Click += Boton_Click!;
                flowLayoutPanel1.Controls.Add(boton);
            }

            int count = flowLayoutPanel1.Controls.Count;
            string plural = count != 1 ? "s" : "";
            lblStatus.Text = $"  {count} WAD{plural} cargado{plural}   |   {_gzdoomSettings.Batchs.Location}";
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
                    MessageBox.Show($"Can't execute file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddBatchFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new BatchCreatorForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadButtonsBatch();
            }
        }

        private void RefreshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadButtonsBatch();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            try
            {
                Process.Start(new ProcessStartInfo("notepad.exe", settingsPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir settings:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchWad(object sender, EventArgs e)
        {
            LoadButtonsBatch(txtSearch.Text);
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
