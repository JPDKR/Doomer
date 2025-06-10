using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace Doomer
{
    public partial class Doomer : Form
    {
        private readonly string folderBatchs;
        private readonly string folderImages;
        private readonly string extension;
        private readonly string imagesExtension;

        public Doomer()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            folderBatchs = config["Folders:Batchs"]!;
            extension = config["Folders:BatchsExtension"]!;
            folderImages = config["Folders:Images"]!;
            imagesExtension = config["Folders:ImagesExtension"]!;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadButtonsBatch();
        }

        private void LoadButtonsBatch()
        {
            string[] files = Directory.GetFiles(folderBatchs, extension);
            int iconWidth = 120;
            int iconHeight = 100;
            int iconPadding = 5;

            int index = 0;

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string baseName = Path.GetFileNameWithoutExtension(file);
                string iconPath = Path.Combine(folderImages, baseName + imagesExtension);

                Button boton = new()
                {
                    Width = iconWidth + 10,
                    Height = iconHeight + 10,
                    Tag = file,
                    Margin = new Padding(iconPadding)
                };

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
    }
}