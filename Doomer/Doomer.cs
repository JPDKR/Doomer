using Doomer.Options;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace Doomer
{
    public partial class Doomer : Form
    {
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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadButtonsBatch();
        }

        private void LoadButtonsBatch()
        {
            flowLayoutPanel1.Controls.Clear();

            string[] files = Directory.GetFiles(_gzdoomSettings.Batchs.Location, $"*{_gzdoomSettings.Batchs.Extension}");

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string baseName = Path.GetFileNameWithoutExtension(file);
                string iconPath = Path.Combine(_gzdoomSettings.Images.Location, baseName + _gzdoomSettings.Images.Extension);

                Button boton = new()
                {
                    Width = _iconsSettings.Width+ 10,
                    Height = _iconsSettings.Height + 10,
                    Tag = file,
                    Margin = new Padding(_iconsSettings.Padding),
                };

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
                        boton.Text = fileName;
                    }
                }
                else
                {
                    boton.Text = fileName;
                }

                boton.Click += Boton_Click!;
                flowLayoutPanel1.Controls.Add(boton);
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
    }
}