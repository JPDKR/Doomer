namespace Doomer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Localization.Apply(Options.AppConfiguration.Interface.Language);

            if (Options.AppConfiguration.LoadError is { } error)
                MessageBox.Show(string.Format(Strings.SettingsLoadError, Options.AppConfiguration.FilePath, error),
                    Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

            Application.Run(new Doomer());
        }
    }
}