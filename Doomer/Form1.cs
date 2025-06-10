using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace Doomer
{
    public partial class Form1 : Form
    {
        private readonly string carpetaBatchs;
        private readonly string carpetaIconos;
        private readonly string extension;
        private readonly string imagenesExtension;

        public Form1()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            carpetaBatchs = config["Carpetas:Batchs"]!;
            extension = config["Carpetas:BatchsExtension"]!;
            carpetaIconos = config["Carpetas:Imagenes"]!;
            imagenesExtension = config["Carpetas:ImagenesExtension"]!;
            carpetaIconos = Path.Combine(carpetaBatchs, "Pics");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CargarBotonesBatch();
        }

        private void CargarBotonesBatch()
        {
            string[] archivos = Directory.GetFiles(carpetaBatchs, extension);
            int iconWidth = 120;
            int iconHeight = 100;
            //int iconosPorFila = 7;

            int index = 0;

            foreach (var archivo in archivos)
            {
                string nombreArchivo = Path.GetFileName(archivo);
                string nombreBase = Path.GetFileNameWithoutExtension(archivo);
                string rutaIcono = Path.Combine(carpetaIconos, nombreBase + imagenesExtension);

                Button boton = new()
                {
                    Width = iconWidth + 10,
                    Height = iconHeight + 10,
                    Tag = archivo,
                    Margin = new Padding(5)
                };

                if (File.Exists(rutaIcono))
                {
                    try
                    {
                        Image img = Image.FromFile(rutaIcono);
                        boton.Image = new Bitmap(img, new Size(iconWidth, iconHeight));
                        boton.ImageAlign = ContentAlignment.MiddleCenter;
                    }
                    catch
                    {
                        boton.Text = nombreArchivo;
                    }
                }
                else
                {
                    boton.Text = nombreArchivo;
                }

                boton.Click += Boton_Click!;
                flowLayoutPanel1.Controls.Add(boton);
                index++;
            }
        }

        private void Boton_Click(object sender, EventArgs e)
        {
            if (sender is Button boton && boton.Tag is string ruta)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo ejecutar el archivo:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}