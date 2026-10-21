using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Doomer.Services
{
    public static class BatchLaunchService
    {
        static BatchLaunchService() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Runs the batch through cmd with its window hidden, so no console stays open while playing
        // (cmd waits for the game to exit). The working directory is inherited, like launching it
        // from the shell did, so relative paths in the batch resolve the same way.
        public static ProcessStartInfo CreateStartInfo(string batchPath) => new()
        {
            FileName = "cmd.exe",
            // The outer quotes are stripped by cmd /c, which keeps the path's own quotes intact.
            Arguments = $"/d /c \"\"{batchPath}\"\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            // cmd writes its errors in the console's OEM code page.
            StandardErrorEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage)
        };

        // Waits for the batch (and so the game) to finish. Returns cmd's error output when it failed,
        // e.g. because the executable isn't there, or null when it ran fine.
        public static async Task<string?> RunAsync(string batchPath)
        {
            using var process = Process.Start(CreateStartInfo(batchPath))
                ?? throw new InvalidOperationException(batchPath);

            var errors = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode != 0 && !string.IsNullOrWhiteSpace(errors) ? errors.Trim() : null;
        }
    }
}
