using System;
using AspNetSkeleton.Core.Middlewares;

namespace AspNetSkeleton.Core
{     
    public class CoreSettings
    {
        public bool EnableApplicationInsights { get; set; }
        public TimeSpan ShutDownTimeOut { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan ServiceTimeOut { get; set; } = TimeSpan.FromMinutes(1);

        public PathAdjustment[] PathAdjustments { get; set; }
        public string[] ReverseProxies { get; set; }

        public string ServiceBaseUrl { get; set; }
        public string UIBaseUrl { get; set; }
        public string[] AdminMailTo { get; set; }
    }
}
