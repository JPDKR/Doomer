using Doomer.Services;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;

namespace Doomer.Tests
{
    public class LocalizationTests
    {
        private static readonly ResourceManager Resources = new("Doomer.Strings", typeof(BatchFileService).Assembly);

        public static TheoryData<string> Translations()
        {
            var data = new TheoryData<string>();

            foreach (var (culture, _) in Localization.Languages)
            {
                if (culture != "en")
                    data.Add(culture);
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(Translations))]
        public void Translation_HasEveryStringWithTheSamePlaceholders(string culture)
        {
            var english = ReadStrings(CultureInfo.InvariantCulture);
            var translated = ReadStrings(CultureInfo.GetCultureInfo(culture));

            Assert.NotEmpty(english);

            foreach (var (key, text) in english)
            {
                Assert.True(translated.TryGetValue(key, out var translation), $"{culture} is missing \"{key}\".");
                Assert.False(string.IsNullOrWhiteSpace(translation), $"{culture} has an empty \"{key}\".");
                Assert.Equal(Placeholders(text), Placeholders(translation!));
            }
        }

        // Only the strings in that exact culture's file, without falling back to English.
        private static Dictionary<string, string> ReadStrings(CultureInfo culture)
        {
            var set = Resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
            Assert.NotNull(set);

            return set.Cast<DictionaryEntry>().ToDictionary(e => (string)e.Key, e => (string)e.Value!);
        }

        private static string[] Placeholders(string text) =>
            [.. Regex.Matches(text, @"\{\d+\}").Select(m => m.Value).Order()];
    }
}
