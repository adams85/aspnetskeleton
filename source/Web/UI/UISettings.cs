using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetSkeleton.UI
{
    public class UISettings
    {
        public string ListenUrl { get; set; }

        public string[] ReverseProxies { get; set; }
        public bool EnableResponseMinification { get; set; }
        public bool EnableResponseCompression { get; set; }
        public bool EnableResponseCaching { get; set; }
        public TimeSpan CacheHeaderMaxAge { get; set; } = TimeSpan.FromDays(7);

        public bool EnableLocalization { get; set; }
        public string DefaultCulture { get; set; } = "en-US";

        public bool EnableTheming { get; set; }
        public string DefaultTheme { get; set; } = "Dark";

        public bool EnableRegistration { get; set; }

        public TimeSpan PasswordTokenExpiration { get; set; } = TimeSpan.FromDays(1);
        public int DefaultDeviceLimit { get; set; }
    }
}
