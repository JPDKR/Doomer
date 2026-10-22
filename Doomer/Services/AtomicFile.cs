namespace Doomer.Services
{
    public static class AtomicFile
    {
        // Writes to a temp file next to it first and then swaps it in, so a crash or a full disk mid-save
        // leaves the previous file whole instead of half-written. Creates the folder if it's missing.
        public static void WriteAllText(string path, string contents)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            var temp = path + ".tmp";

            try
            {
                File.WriteAllText(temp, contents);
                File.Move(temp, path, overwrite: true);
            }
            catch
            {
                TempFileService.TryDelete(temp);
                throw;
            }
        }
    }
}
