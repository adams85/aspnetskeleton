using System;

namespace AspNetSkeleton.UI
{
    [Flags]
    public enum ResponseKind
    {
        None = 0,
        StaticFiles = 1,
        Bundles = 2,
        Views = 4,
        All = StaticFiles | Bundles | Views
    }

    public class UISettings
    {
        public string ListenUrl { get; set; }

        public string[] ReverseProxies { get; set; }

        public ResponseKind EnableResponseMinification { get; set; }
        public bool EnableResponseCompression { get; set; }
        public ResponseKind EnableResponseCaching { get; set; }
        public TimeSpan CacheHeaderMaxAge { get; set; } = TimeSpan.FromDays(7);
        public ResponseKind UsePersistentCache { get; set; }

        public bool EnableLocalization { get; set; }
        public string DefaultCulture { get; set; } = "en-US";

        public bool EnableTheming { get; set; }
        public string DefaultTheme { get; set; } = "Dark";

        public bool EnableRegistration { get; set; }

        public TimeSpan PasswordTokenExpiration { get; set; } = TimeSpan.FromDays(1);
        public int DefaultDeviceLimit { get; set; }
    }
}
