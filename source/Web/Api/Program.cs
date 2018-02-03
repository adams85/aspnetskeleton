using AspNetSkeleton.Api.Hosting;
using AspNetSkeleton.Core.Hosting;
using AspNetSkeleton.Core.Infrastructure;
using AspNetSkeleton.Core.Utils;
using Autofac;

namespace AspNetSkeleton.Api
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = ConfigurationHelper.CreateDefaultBuilder()
                .AddJsonConfigFile("appsettings.json")
                .Build();

            var hostConfiguration = new HostConfiguration(configuration);
            var appConfiguration = new AppConfiguration(configuration);

            var module = new CoreModule(hostConfiguration, appConfiguration);
            var builder = new ContainerBuilder();
            builder.RegisterModule(module);

            using (var container = builder.Build())
            using (var hostScope = container.Resolve<IHostScope>())
                hostScope.Host.Execute(args);
        }
    }
}
