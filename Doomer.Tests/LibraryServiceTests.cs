using Doomer.Services;

namespace Doomer.Tests
{
    public class LibraryServiceTests : IDisposable
    {
        private readonly string _folder = Directory.CreateTempSubdirectory("Doomer library ").FullName;

        private string FilePath => Path.Combine(_folder, "library.json");

        public void Dispose() => Directory.Delete(_folder, recursive: true);

        private static readonly string[] Wads = ["Sunlust", "ancient aliens", "Eviternity", "Sigil"];

        [Fact]
        public void Sort_ByName_LeavesFavoritesInPlace()
        {
            var library = new LibraryService(FilePath);
            library.SetFavorite("Sigil", true);
            library.SetFavorite("eviternity", true);

            Assert.Equal(["ancient aliens", "Eviternity", "Sigil", "Sunlust"], library.Sort(Wads));
        }

        [Fact]
        public void Sort_ByLastPlayed_PutsTheMostRecentFirstAndNeverPlayedLast()
        {
            var library = new LibraryService(FilePath);
            library.SetSortOrder(WadSortOrder.LastPlayed);
            library.MarkPlayed("Sunlust", new DateTime(2026, 10, 1));
            library.MarkPlayed("Sigil", new DateTime(2026, 10, 20));

            Assert.Equal(["Sigil", "Sunlust", "ancient aliens", "Eviternity"], library.Sort(Wads));

            library.SetFavorite("Eviternity", true);

            Assert.Equal(["Sigil", "Sunlust", "ancient aliens", "Eviternity"], library.Sort(Wads));
        }

        [Fact]
        public void Changes_AreKeptAcrossRestarts()
        {
            var library = new LibraryService(FilePath);
            library.SetSortOrder(WadSortOrder.LastPlayed);
            library.SetFavoritesOnly(true);
            library.SetFavorite("Sigil", true);
            library.MarkPlayed("Sunlust", new DateTime(2026, 10, 21, 18, 30, 0, DateTimeKind.Utc));

            var reloaded = new LibraryService(FilePath);

            Assert.Equal(WadSortOrder.LastPlayed, reloaded.SortOrder);
            Assert.True(reloaded.FavoritesOnly);
            Assert.True(reloaded.IsFavorite("SIGIL"));
            Assert.Equal(new DateTime(2026, 10, 21, 18, 30, 0, DateTimeKind.Utc), reloaded.GetLastPlayed("sunlust"));
            Assert.False(File.Exists(FilePath + ".tmp"));
        }

        [Fact]
        public void Rename_KeepsTheFavoriteAndHistoryUnderTheNewName()
        {
            var library = new LibraryService(FilePath);
            library.SetFavorite("Sigil", true);
            library.MarkPlayed("Sigil", new DateTime(2026, 10, 20));

            library.Rename("sigil", "Sigil II");

            var reloaded = new LibraryService(FilePath);
            Assert.True(reloaded.IsFavorite("Sigil II"));
            Assert.Equal(new DateTime(2026, 10, 20), reloaded.GetLastPlayed("Sigil II"));
            Assert.False(reloaded.IsFavorite("Sigil"));
            Assert.Null(reloaded.GetLastPlayed("Sigil"));
        }

        [Fact]
        public void Rename_ChangingOnlyTheCaseKeepsTheWad()
        {
            var library = new LibraryService(FilePath);
            library.SetFavorite("sigil", true);

            library.Rename("sigil", "Sigil");

            Assert.True(new LibraryService(FilePath).IsFavorite("Sigil"));
            Assert.Contains("\"Sigil\"", File.ReadAllText(FilePath));
        }

        [Fact]
        public void Rename_WithoutHistoryWritesNothing()
        {
            new LibraryService(FilePath).Rename("Sigil", "Sigil II");

            Assert.False(File.Exists(FilePath));
        }

        [Fact]
        public void Completed_IsKeptAcrossRestartsAndRenames()
        {
            var library = new LibraryService(FilePath);
            library.SetCompleted("Sigil", true);
            library.SetCompleted("Sunlust", true);
            library.SetCompleted("sunlust", false);

            library.Rename("Sigil", "Sigil II");
            var reloaded = new LibraryService(FilePath);

            Assert.True(reloaded.IsCompleted("SIGIL II"));
            Assert.False(reloaded.IsCompleted("Sigil"));
            Assert.False(reloaded.IsCompleted("Sunlust"));

            reloaded.Forget("Sigil II");

            Assert.False(new LibraryService(FilePath).IsCompleted("Sigil II"));
        }

        [Fact]
        public void Forget_DropsTheWadsHistory()
        {
            var library = new LibraryService(FilePath);
            library.SetFavorite("Sigil", true);
            library.MarkPlayed("Sigil", DateTime.UtcNow);

            library.Forget("sigil");
            var reloaded = new LibraryService(FilePath);

            Assert.False(reloaded.IsFavorite("Sigil"));
            Assert.Null(reloaded.GetLastPlayed("Sigil"));
        }

        [Theory]
        [InlineData("not json at all")]
        [InlineData("{\"SortOrder\": \"Sideways\"}")]
        [InlineData("")]
        public void ADamagedFile_StartsAnEmptyLibrary(string content)
        {
            File.WriteAllText(FilePath, content);

            var library = new LibraryService(FilePath);

            Assert.Equal(WadSortOrder.Name, library.SortOrder);
            Assert.False(library.FavoritesOnly);
            Assert.False(library.IsFavorite("Sigil"));
        }

        [Fact]
        public void AFileFromBeforeTheFavoritesFilter_ListsEveryWad()
        {
            File.WriteAllText(FilePath, "{\"SortOrder\": \"LastPlayed\", \"Favorites\": [\"Sigil\"]}");

            var library = new LibraryService(FilePath);

            Assert.False(library.FavoritesOnly);
            Assert.True(library.IsFavorite("Sigil"));
        }

        [Fact]
        public void AMissingFile_StartsAnEmptyLibraryWithoutCreatingIt()
        {
            var library = new LibraryService(FilePath);

            Assert.Equal(WadSortOrder.Name, library.SortOrder);
            Assert.False(File.Exists(FilePath));
        }
    }
}
