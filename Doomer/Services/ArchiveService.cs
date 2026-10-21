using SharpCompress.Archives;
using SharpCompress.Common;

namespace Doomer.Services
{
    public static class ArchiveService
    {
        public const string FilePatterns = "*.zip;*.rar;*.7z";

        private static readonly string[] ArchiveExtensions = [.. FilePatterns.Split(';').Select(p => p.TrimStart('*'))];

        // Whether a file is an archive this app can extract, going by its extension like the file dialog does.
        public static bool IsArchive(string path) =>
            ArchiveExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

        // Files a source port can load directly as the batch's WAD.
        private static readonly string[] WadExtensions = [".wad", ".pk3", ".pk7"];

        // What the batch loads alongside its WAD: main files first, then DeHackEd (.deh/.bex) patches.
        private static readonly string[] MainExtraExtensions = [".wad", ".pk3"];
        private static readonly string[] SecondaryExtraExtensions = [".deh", ".bex"];

        // "D.O.O.M..rar" -> "D.O.O.M": Windows drops trailing dots and spaces from folder names anyway.
        // Each word starts with a capital ("ancient aliens.zip" -> "Ancient Aliens"); the rest is kept as-is.
        public static string GetFolderName(string archivePath) =>
            CapitalizeWords(Path.GetFileNameWithoutExtension(archivePath).TrimEnd('.', ' '));

        private static string CapitalizeWords(string text)
        {
            var chars = text.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                if (i == 0 || chars[i - 1] is ' ' or '-' or '_')
                    chars[i] = char.ToUpperInvariant(chars[i]);
            }

            return new string(chars);
        }

        // Extracts every file in the archive under destination, keeping its folder structure,
        // and returns the extracted file paths. Entries that would land outside destination are rejected.
        // When the whole archive sits inside a single folder, that folder is skipped so its contents
        // land directly in destination instead of e.g. "sunlust\sunlust\sunlust.wad".
        public static List<string> Extract(string archivePath, string destination)
        {
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(destination)) + Path.DirectorySeparatorChar;
            var extracted = new List<string>();

            using var archive = ArchiveFactory.OpenArchive(archivePath, null);

            var sharedFolder = GetSharedFolder(archive.Entries.Where(e => !e.IsDirectory && !string.IsNullOrEmpty(e.Key)).Select(e => e.Key!));

            // Solid archives (and 7z) must be read front to back; the rest can open each entry on its own.
            if (archive.IsSolid || archive.Type == ArchiveType.SevenZip)
            {
                using var reader = archive.ExtractAllEntries();

                while (reader.MoveToNextEntry())
                    ExtractEntry(reader.Entry, root, sharedFolder, extracted, reader.WriteEntryTo);
            }
            else
            {
                foreach (var entry in archive.Entries)
                    ExtractEntry(entry, root, sharedFolder, extracted, output =>
                    {
                        using var input = entry.OpenEntryStream();
                        input.CopyTo(output);
                    });
            }

            return extracted;
        }

        // The top-level folder (with its trailing separator) that every file in the archive is inside,
        // or an empty string when some file sits at the top level or files are spread across folders.
        public static string GetSharedFolder(IEnumerable<string> fileKeys)
        {
            string? shared = null;

            foreach (var key in fileKeys)
            {
                var separator = key.IndexOfAny(['/', '\\']);

                if (separator <= 0)
                    return string.Empty;

                var folder = key[..(separator + 1)];

                // "." and ".." aren't real folders; leave them for the unsafe path check.
                if (folder[..^1] is "." or "..")
                    return string.Empty;

                if (shared is null)
                    shared = folder;
                else if (!string.Equals(shared[..^1], folder[..^1], StringComparison.OrdinalIgnoreCase))
                    return string.Empty;
            }

            return shared ?? string.Empty;
        }

        private static void ExtractEntry(IEntry entry, string root, string sharedFolder, List<string> extracted, Action<Stream> write)
        {
            if (entry.IsDirectory || string.IsNullOrEmpty(entry.Key))
                return;

            var target = Path.GetFullPath(Path.Combine(root, entry.Key[sharedFolder.Length..]));

            if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(string.Format(Strings.ArchiveUnsafePath, entry.Key));

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);

            using (var output = File.Create(target))
                write(output);

            extracted.Add(target);
        }

        // The other files that came with the WAD and that the batch loads too: .wad/.pk3 files first,
        // then .deh/.bex patches, each group by name. Anything else (readmes, screenshots...) is left out.
        public static List<string> FindExtraFiles(IEnumerable<string> files, string wad)
        {
            var wadPath = Path.GetFullPath(wad);

            return [.. files
                .Where(f => !string.Equals(Path.GetFullPath(f), wadPath, StringComparison.OrdinalIgnoreCase))
                .Select(f => (File: f, Group: GetExtraGroup(f)))
                .Where(f => f.Group >= 0)
                .OrderBy(f => f.Group)
                .ThenBy(f => f.File, StringComparer.OrdinalIgnoreCase)
                .Select(f => f.File)];
        }

        // 0 for main files, 1 for secondary ones, -1 for files the batch doesn't load.
        private static int GetExtraGroup(string file)
        {
            var extension = Path.GetExtension(file);

            if (MainExtraExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                return 0;

            return SecondaryExtraExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ? 1 : -1;
        }

        // The extracted files that could be the batch's WAD.
        public static List<string> FindWadCandidates(IEnumerable<string> files) =>
            files.Where(f => WadExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)).ToList();
    }
}
