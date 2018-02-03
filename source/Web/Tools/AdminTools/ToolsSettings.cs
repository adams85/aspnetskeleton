using AspNetSkeleton.AdminTools.Properties;
using System;

namespace AspNetSkeleton.AdminTools
{
    public interface IToolsSettings
    {
        string ApiUrl { get; }
        TimeSpan ApiTimeout { get; }
    }

    public class ToolsSettings : IToolsSettings
    {
        public string ApiUrl { get; } = Settings.Default.ApiUrl;

        public TimeSpan ApiTimeout { get; } = Settings.Default.ApiTimeout;
    }
}
