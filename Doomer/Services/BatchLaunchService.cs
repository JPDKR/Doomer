using System.Diagnostics;

namespace Doomer.Services
{
    public static class BatchLaunchService
    {
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
            RedirectStandardError = true
        };

        // Waits for the batch (and so the game) to finish. Returns cmd's error output when it failed,
        // e.g. because the executable isn't there, or null when it ran fine.
        public static async Task<string?> RunAsync(string batchPath)
        {
            using var process = Process.Start(CreateStartInfo(batchPath))
                ?? throw new InvalidOperationException(batchPath);

            // Read as bytes, since cmd writes its errors in UTF-8 after a batch switched to it and in the OEM
            // code page otherwise.
            using var stderr = new MemoryStream();
            await process.StandardError.BaseStream.CopyToAsync(stderr);
            await process.WaitForExitAsync();

            var errors = ConsoleText.Decode(stderr.ToArray());
            return process.ExitCode != 0 && !string.IsNullOrWhiteSpace(errors) ? errors.Trim() : null;
        }
    }
}
