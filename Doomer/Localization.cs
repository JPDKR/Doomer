using System.Globalization;

namespace Doomer
{
    // Picks the UI language. Texts live in Strings.resx (English) and Strings.<culture>.resx.
    public static class Localization
    {
        // The languages the UI is translated to. An empty setting follows Windows' language,
        // falling back to English when Windows uses one that isn't here.
        public static readonly IReadOnlyList<(string Culture, string Name)> Languages =
        [
            ("en", "English"),
            ("es", "Español"),
        ];

        // Captured before Apply ever changes it, so "system default" can be restored later.
        private static readonly CultureInfo SystemCulture = CultureInfo.CurrentUICulture;

        public static void Apply(string? language)
        {
            var culture = SystemCulture;

            if (!string.IsNullOrWhiteSpace(language))
            {
                try
                {
                    culture = CultureInfo.GetCultureInfo(language);
                }
                catch (CultureNotFoundException)
                {
                    // Keep the system language for a setting that isn't a real culture.
                }
            }

            // Both, so texts built on background threads (e.g. in Task.Run) use it too.
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
