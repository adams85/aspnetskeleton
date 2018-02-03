using AspNetSkeleton.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AspNetSkeleton.Core.Utils
{
    public static class ConfigurationHelper
    {
        static readonly IConfiguration environmentVariables = new ConfigurationBuilder().AddEnvironmentVariables("ASPNETCORE_").Build();

        public static string EnvironmentName { get; } = environmentVariables?[WebHostDefaults.EnvironmentKey] ?? Microsoft.AspNetCore.Hosting.EnvironmentName.Production;

        public static IConfigurationBuilder CreateDefaultBuilder()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(environmentVariables.AsEnumerable())
                .SetBasePath(AppEnvironment.Instance.AppBasePath);
        }

        public static IConfigurationBuilder AddJsonConfigFile(this IConfigurationBuilder @this, string path, bool optional = false, bool reloadOnChange = false)
        {
            var dirPath = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);

            return @this
                .AddJsonFile(path, optional, reloadOnChange)
                .AddJsonFile(Path.Combine(dirPath, string.Concat(fileName, ".", EnvironmentName, extension)), optional: true, reloadOnChange: reloadOnChange);
        }
    }
}
