using System.Globalization;

namespace AspNetSkeleton.UI
{
    public static class UIConstants
    {
        public static readonly CultureInfo DefaultCulture = new CultureInfo("en-US");
        public const string DefaultTheme = "Dark";

        public const int DefaultOutputCacheDuration = 60 * 60 * 24;
    }
}