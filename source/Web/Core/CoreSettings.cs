using System;

namespace AspNetSkeleton.Core
{     
    public interface ICoreSettings
    {
        TimeSpan ServiceTimeOut { get; }
        string ServiceBaseUrl { get; }
        string UIBaseUrl { get; }
        string[] AdminMailTo { get; }
    }
}
