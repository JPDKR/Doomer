using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Windows.Forms;

namespace Doomer
{
    public partial class Doomer : Form
    {
        private readonly string batchsLocation;
        private readonly string imagesLocation;
        private readonly string batchsExtension;
        private readonly string imagesExtension;
        private readonly int iconWidth;
        private readonly int iconHeight;
        private readonly int iconPadding;

        public Doomer()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            batchsLocation = config["GZDoom:Batchs:Location"]!;
            batchsExtension = config["GZDoom:Batchs:Extension"]!;
            imagesLocation = config["GZDoom:Images:Location"]!;
            imagesExtension = config["GZDoom:Images:Extension"]!;
            iconWidth = int.Parse(config["Icons:Width"]!);
            iconHeight = int.Parse(config["Icons:Height"]!);
            iconPadding = int.Parse(config["Icons:Padding"]!);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadButtonsBatch();
        }

        private void LoadButtonsBatch()
        {
            flowLayoutPanel1.Controls.Clear();

            string[] files = Directory.GetFiles(batchsLocation, $"*{batchsExtension}");

            int index = 0;

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string baseName = Path.GetFileNameWithoutExtension(file);
                string iconPath = Path.Combine(imagesLocation, baseName + imagesExtension);

                Button boton = new()
                {
                    Width = iconWidth + 10,
                    Height = iconHeight + 10,
                    Tag = file,
                    Margin = new Padding(iconPadding),
                };

                ToolTip tooltip = new();
                tooltip.SetToolTip(boton, baseName);

                if (File.Exists(iconPath))
                {
                    try
                    {
                        Image img = Image.FromFile(iconPath);
                        boton.Image = new Bitmap(img, new Size(iconWidth, iconHeight));
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
                index++;
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

        private void addBatchFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new BatchCreatorForm();

            if (form.ShowDialog() == DialogResult.OK)
            {                
                LoadButtonsBatch();
            }
        }

        private void refreshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadButtonsBatch();
        }
    }
}