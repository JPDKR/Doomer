using System.Text.Json;
using System.Text.Json.Serialization;

namespace Doomer.Services
{
    public enum WadSortOrder
    {
        Name,
        LastPlayed
    }

    // What the user does with their WADs (favorites, which ones they finished, when each was last played,
    // how the list is sorted),
    // kept apart from the settings in its own file. WADs are known by their batch file's name.
    public class LibraryService
    {
        private readonly string _filePath;
        private readonly HashSet<string> _favorites = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _completed = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> _lastPlayed = new(StringComparer.OrdinalIgnoreCase);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public WadSortOrder SortOrder { get; private set; }

        // Whether the main window lists only the favorites.
        public bool FavoritesOnly { get; private set; }

        // A missing or unreadable file just starts an empty library, so it never stops the app from opening.
        public LibraryService(string filePath)
        {
            _filePath = filePath;

            LibraryData? data = null;

            try
            {
                if (File.Exists(filePath))
                    data = JsonSerializer.Deserialize<LibraryData>(File.ReadAllText(filePath), JsonOptions);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
            }

            if (data is null)
                return;

            SortOrder = Enum.IsDefined(data.SortOrder) ? data.SortOrder : WadSortOrder.Name;
            FavoritesOnly = data.FavoritesOnly;
            _favorites.UnionWith(data.Favorites ?? []);
            _completed.UnionWith(data.Completed ?? []);

            foreach (var (name, when) in data.LastPlayed ?? [])
                _lastPlayed[name] = when;
        }

        public bool IsFavorite(string name) => _favorites.Contains(name);

        public bool IsCompleted(string name) => _completed.Contains(name);

        public DateTime? GetLastPlayed(string name) => _lastPlayed.TryGetValue(name, out var when) ? when : null;

        // The setters save right away and throw if the file can't be written.
        public void SetSortOrder(WadSortOrder order)
        {
            SortOrder = order;
            Save();
        }

        public void SetFavoritesOnly(bool favoritesOnly)
        {
            FavoritesOnly = favoritesOnly;
            Save();
        }

        public void SetFavorite(string name, bool favorite)
        {
            if (favorite)
                _favorites.Add(name);
            else
                _favorites.Remove(name);

            Save();
        }

        public void SetCompleted(string name, bool completed)
        {
            if (completed)
                _completed.Add(name);
            else
                _completed.Remove(name);

            Save();
        }

        public void MarkPlayed(string name, DateTime when)
        {
            _lastPlayed[name] = when;
            Save();
        }

        // For a renamed batch, so it's still a favorite (or completed) and keeps its history under the new name.
        public void Rename(string oldName, string newName)
        {
            var favorite = _favorites.Remove(oldName);
            var completed = _completed.Remove(oldName);
            var played = _lastPlayed.Remove(oldName, out var when);

            if (!favorite && !completed && !played)
                return;

            if (favorite)
                _favorites.Add(newName);

            if (completed)
                _completed.Add(newName);

            if (played)
                _lastPlayed[newName] = when;

            Save();
        }

        // For a deleted batch, so a new one with the same name doesn't inherit its history.
        public void Forget(string name)
        {
            if (_favorites.Remove(name) | _completed.Remove(name) | _lastPlayed.Remove(name))
                Save();
        }

        // In the chosen order, favorites included. WADs never played come last when sorting by last played,
        // and ties fall back to the name.
        public List<string> Sort(IEnumerable<string> names)
        {
            var ordered = SortOrder == WadSortOrder.LastPlayed
                ? names.OrderByDescending(n => GetLastPlayed(n) ?? DateTime.MinValue)
                    .ThenBy(n => n, StringComparer.OrdinalIgnoreCase)
                : names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase);

            return [.. ordered];
        }

        private void Save()
        {
            var data = new LibraryData
            {
                SortOrder = SortOrder,
                FavoritesOnly = FavoritesOnly,
                Favorites = [.. _favorites.Order(StringComparer.OrdinalIgnoreCase)],
                Completed = [.. _completed.Order(StringComparer.OrdinalIgnoreCase)],
                LastPlayed = new(_lastPlayed, StringComparer.OrdinalIgnoreCase)
            };

            AtomicFile.WriteAllText(_filePath, JsonSerializer.Serialize(data, JsonOptions));
        }

        private class LibraryData
        {
            public WadSortOrder SortOrder { get; set; }
            public bool FavoritesOnly { get; set; }
            public List<string>? Favorites { get; set; }
            public List<string>? Completed { get; set; }
            public Dictionary<string, DateTime>? LastPlayed { get; set; }
        }
    }
}
