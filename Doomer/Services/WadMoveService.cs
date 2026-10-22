using Doomer.Options;
using System.Text.RegularExpressions;

namespace Doomer.Services
{
    public record FileMove(string Source, string Destination);

    // The batches that load a moved file and now point to its new place, and the ones that couldn't be updated.
    public record BatchesUpdate(List<string> Updated, List<string> Failed);

    // A WAD picked from outside the WADs directory is moved into it. The extra files next to it go along,
    // and every batch that loads any of them is pointed to their new place, so none is left behind.
    public static class WadMoveService
    {
        private static readonly Regex QuotedPathPattern = new("\"(?<path>[^\"\\r\\n]*)\"", RegexOptions.Compiled);

        // The WAD goes straight into the WADs directory, and the extra files in its folder (subfolders
        // included) keep their place relative to it. Extras elsewhere, e.g. a music WAD several batches
        // share, stay where they are. Nothing moves when the WAD is already in the WADs directory or
        // isn't a file that can be found.
        public static List<FileMove> Plan(string wad, IEnumerable<string> extraFiles, string wadsDirectory)
        {
            var source = BatchFileService.EnsureWadExtension(wad);

            if (!File.Exists(source) || BatchFileService.GetWadDestination(source, wadsDirectory) is not { } destination)
                return [];

            source = Path.GetFullPath(source);
            var folder = WithTrailingSeparator(Path.GetDirectoryName(source)!);
            var moves = new List<FileMove> { new(source, destination) };

            foreach (var extra in extraFiles.Select(BatchFileService.EnsureWadExtension))
            {
                if (!Path.IsPathRooted(extra) || !File.Exists(extra))
                    continue;

                var extraSource = Path.GetFullPath(extra);

                if (!extraSource.StartsWith(folder, StringComparison.OrdinalIgnoreCase) ||
                    BatchFileService.GetWadDestination(extraSource, wadsDirectory) is null ||
                    FindMove(extraSource, moves) is not null)
                    continue;

                moves.Add(new(extraSource, Path.Combine(Path.GetFullPath(wadsDirectory), Path.GetRelativePath(folder, extraSource))));
            }

            return moves;
        }

        // Where a batch's path (with or without .wad) ends up after the moves, or null when it doesn't move.
        public static string? GetDestination(string path, IReadOnlyList<FileMove> moves)
        {
            var file = BatchFileService.EnsureWadExtension(path);
            return Path.IsPathRooted(file) ? FindMove(Path.GetFullPath(file), moves)?.Destination : null;
        }

        // Moves the files in order. If one fails, the ones already moved go back before the error is rethrown.
        public static void Apply(IReadOnlyList<FileMove> moves)
        {
            var done = new List<FileMove>();

            try
            {
                foreach (var move in moves)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(move.Destination)!);
                    File.Move(move.Source, move.Destination, overwrite: true);
                    done.Add(move);
                }
            }
            catch
            {
                Undo(done);
                throw;
            }
        }

        // Puts moved files back where they were, as far as it can: it's only called when something already failed.
        public static void Undo(IEnumerable<FileMove> moves)
        {
            foreach (var move in moves.Reverse())
            {
                try
                {
                    File.Move(move.Destination, move.Source);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                }
            }
        }

        // Points the quoted paths of moved files in a batch command to their new place. Relative paths are
        // resolved from the executable's folder, like the source port does. Null when nothing in it moved.
        public static string? UpdateCommand(string command, string? executable, IReadOnlyList<FileMove> moves)
        {
            var changed = false;

            var updated = QuotedPathPattern.Replace(command, match =>
            {
                var path = BatchFileService.ResolveWadPath(match.Groups["path"].Value, executable);

                if (path is null || FindMove(path, moves) is not { } move)
                    return match.Value;

                changed = true;
                return $"\"{move.Destination}\"";
            });

            return changed ? updated : null;
        }

        // Updates every batch in the batches folder but the one being saved (savedBatchPath).
        public static BatchesUpdate UpdateBatches(BatchSettings batches, PortsSettings ports, string savedBatchPath,
            IReadOnlyList<FileMove> moves)
        {
            var result = new BatchesUpdate([], []);

            if (moves.Count == 0 || !Directory.Exists(batches.Path))
                return result;

            var saved = Path.GetFullPath(savedBatchPath);

            foreach (var file in Directory.EnumerateFiles(batches.Path, $"*{batches.Extension}"))
            {
                if (string.Equals(Path.GetFullPath(file), saved, StringComparison.OrdinalIgnoreCase))
                    continue;

                var name = Path.GetFileNameWithoutExtension(file);

                try
                {
                    var command = File.ReadAllText(file);

                    // A batch that can't be parsed still gets its full paths updated.
                    string? executable = null;
                    if (BatchFileService.TryParseCommand(command, ports.GZDoom.Plugins, out var port, out _, out _, out _, out _, out _))
                        executable = port == SourcePort.DSDADoom ? ports.DSDADoom.Path : ports.GZDoom.Path;

                    if (UpdateCommand(command, executable, moves) is not { } updated)
                        continue;

                    File.WriteAllText(file, updated);
                    result.Updated.Add(name);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    result.Failed.Add($"{name}: {ex.Message}");
                }
            }

            return result;
        }

        private static FileMove? FindMove(string fullPath, IReadOnlyList<FileMove> moves) =>
            moves.FirstOrDefault(m => string.Equals(m.Source, fullPath, StringComparison.OrdinalIgnoreCase));

        // "C:\" already ends in a separator, so it isn't added twice.
        private static string WithTrailingSeparator(string folder) =>
            Path.EndsInDirectorySeparator(folder) ? folder : folder + Path.DirectorySeparatorChar;
    }
}
