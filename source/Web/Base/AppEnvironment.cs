using Microsoft.Extensions.PlatformAbstractions;

namespace AspNetSkeleton.Base
{
    public interface IAppEnvironment
    {
        string AppBasePath { get; }
        string AppName { get; }
        string AppVersion { get; }
    }

    public class AppEnvironment : IAppEnvironment
    {
        static readonly ApplicationEnvironment environment = PlatformServices.Default.Application;

        public static readonly IAppEnvironment Instance = new AppEnvironment();

        protected AppEnvironment() { }

        public string AppBasePath => environment.ApplicationBasePath;
        public string AppName => environment.ApplicationName;
        public string AppVersion => environment.ApplicationVersion;
    }
}
