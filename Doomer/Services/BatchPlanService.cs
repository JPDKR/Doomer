using Doomer.Options;
using System.Diagnostics.CodeAnalysis;

namespace Doomer.Services
{
    // What the batch form has filled in, as typed. EditingPath and ExistingImagePath are set when editing a batch.
    // WikiUrl is the WAD's Doom Wiki page, if any.
    public record BatchDraft(SourcePort Port, string FileName, string IWad, string Wad, IReadOnlyList<string> ExtraFiles,
        IReadOnlyList<string> Plugins, string Arguments, string Image, string? EditingPath = null, string? ExistingImagePath = null,
        string WikiUrl = "");

    // Everything saving a batch will do, worked out before anything is touched. Confirmations are the yes/no
    // questions to ask first. ImageSource is the new icon (a file or a URL), or empty to keep the current one,
    // and ImageDestination is where it goes.
    public record BatchPlan(string Path, bool Renaming, string Command, List<FileMove> Moves, string ImageSource,
        string? ImageDestination, List<string> Confirmations);

    public static class BatchPlanService
    {
        // Returns false with the reason when the batch can't be saved as it is. Every such check comes before
        // the confirmations, so the user is never asked something only to be told it can't be saved anyway.
        public static bool TryPlan(BatchDraft draft, PortsSettings ports, FoldersSettings folders,
            [NotNullWhen(true)] out BatchPlan? plan, out string error)
        {
            plan = null;

            var port = draft.Port;
            var fileName = draft.FileName.Trim();
            var iwad = draft.IWad.Trim();
            var wad = draft.Wad.Trim();
            var extraFiles = draft.ExtraFiles;
            List<string> plugins = port == SourcePort.GZDoom ? [.. draft.Plugins] : [];

            if (string.IsNullOrWhiteSpace(iwad) || string.IsNullOrWhiteSpace(wad) || string.IsNullOrWhiteSpace(fileName))
                return Fail(Strings.MissingRequiredFields, out error);

            var executable = port == SourcePort.DSDADoom ? ports.DSDADoom.Path : ports.GZDoom.Path;

            if (string.IsNullOrWhiteSpace(executable))
                return Fail(string.Format(Strings.ExecutableNotConfigured, BatchFileService.GetDisplayName(port)), out error);

            if (!BatchFileService.IsValidFileName(fileName, out error))
                return false;

            // An edited batch stays in its folder with its extension, so renaming only changes its name.
            var path = draft.EditingPath is null
                ? Path.Combine(folders.Batches.Path, fileName + folders.Batches.Extension)
                : Path.Combine(Path.GetDirectoryName(draft.EditingPath)!, fileName + Path.GetExtension(draft.EditingPath));
            // Compared exactly, so changing only the case ("sigil" -> "Sigil") renames it too.
            var renaming = draft.EditingPath is not null && !string.Equals(path, draft.EditingPath, StringComparison.Ordinal);

            // Renaming never replaces another batch; only a new one can, after asking.
            if (renaming && File.Exists(path) && !string.Equals(path, draft.EditingPath, StringComparison.OrdinalIgnoreCase))
                return Fail(string.Format(Strings.BatchNameTaken, fileName), out error);

            if (port == SourcePort.DSDADoom && BatchFileService.RequiresGZDoom(wad, extraFiles))
                return Fail(Strings.DSDADoomNoPackages, out error);

            if (!BatchFileService.TryNormalizeWikiUrl(draft.WikiUrl, out var wikiUrl))
                return Fail(Strings.WikiUrlInvalid, out error);

            // A WAD outside the WADs directory gets moved into it, along with the extra files next to it,
            // and the batch points to their new location.
            var moves = WadMoveService.Plan(wad, extraFiles, folders.Wads.Path);

            if (moves.Count > 0 && !Directory.Exists(folders.Wads.Path))
                return Fail(string.Format(Strings.WadsDirNotFoundFixInSettings, folders.Wads.Path), out error);

            var movedWad = moves.Count > 0 ? BatchFileService.StripWadExtension(moves[0].Destination) : wad;
            List<string> movedExtraFiles = [.. extraFiles.Select(f => WadMoveService.GetDestination(f, moves) ?? f)];
            string command;

            try
            {
                command = port == SourcePort.DSDADoom
                    ? BatchFileService.BuildDSDADoomCommand(ports.DSDADoom.Path, iwad, movedWad, movedExtraFiles, draft.Arguments)
                    : BatchFileService.BuildGZDoomCommand(ports.GZDoom.Path, ports.GZDoom.Plugins, iwad, movedWad, plugins,
                        movedExtraFiles, draft.Arguments);
                command = BatchFileService.SetWikiUrl(command, wikiUrl);
            }
            catch (ArgumentException ex)
            {
                return Fail(ex.Message, out error);
            }

            // The current icon left in the Image box is kept (and follows a rename); anything else replaces it.
            var imageSource = draft.Image.Trim();
            if (string.Equals(imageSource, draft.ExistingImagePath, StringComparison.OrdinalIgnoreCase))
                imageSource = string.Empty;

            // Named from the batch's own path, so the icon always matches the batch file.
            string? imageDestination = null;

            if (imageSource.Length > 0)
            {
                if (string.IsNullOrWhiteSpace(folders.Images.Path) || !Directory.Exists(folders.Images.Path))
                    return Fail(string.Format(Strings.ImagesDirNotFoundFixInSettings, folders.Images.Path), out error);

                // A URL is downloaded (and checked) only once the user has confirmed everything else.
                if (!ImageDownloadService.IsUrl(imageSource))
                {
                    if (!File.Exists(imageSource))
                        return Fail(string.Format(Strings.ImageFileNotFound, imageSource), out error);

                    if (!ImageFileService.IsValidImage(imageSource))
                        return Fail(string.Format(Strings.ImageFileInvalid, imageSource), out error);
                }

                imageDestination = BatchFileService.GetImagePath(path, folders.Images);
            }

            List<string> confirmations =
            [
                .. MissingFile("IWAD", iwad),
                .. MissingFile("WAD", wad),
                .. extraFiles.SelectMany(f => MissingFile(Strings.ExtraFileLabel, f)),
                .. plugins.SelectMany(p => MissingPlugin(p, ports.GZDoom)),
                .. moves.Where(m => File.Exists(m.Destination))
                    .Select(m => string.Format(Strings.WadExistsReplace, Path.GetRelativePath(folders.Wads.Path, m.Destination))),
            ];

            if (draft.EditingPath is null && File.Exists(path))
                confirmations.Add(string.Format(Strings.BatchExistsOverwrite, fileName));

            plan = new BatchPlan(path, renaming, command, moves, imageSource, imageDestination, confirmations);
            return true;
        }

        // Only full paths are checked: relative ones are resolved by the source port when it starts, not by the app.
        private static IEnumerable<string> MissingFile(string label, string file)
        {
            var path = BatchFileService.EnsureWadExtension(file.Trim());

            if (Path.IsPathRooted(path) && !File.Exists(path))
                yield return string.Format(Strings.FileNotFoundCreateAnyway, label, path);
        }

        // Plugins are looked up in the plugins folder; like GZDoom, one without an extension as a .wad.
        private static IEnumerable<string> MissingPlugin(string plugin, GZDoomSettings gzdoom)
        {
            var directory = BatchFileService.GetPluginsDirectory(gzdoom.Plugins, gzdoom.Path);

            if (string.IsNullOrEmpty(plugin) || directory is null || File.Exists(Path.Combine(directory, plugin)))
                return [];

            return MissingFile(Strings.PluginFileLabel, Path.Combine(directory, plugin));
        }

        private static bool Fail(string message, out string error)
        {
            error = message;
            return false;
        }
    }
}
