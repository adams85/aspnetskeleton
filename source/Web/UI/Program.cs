using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.UI.Hosting;
using AspNetSkeleton.Base;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Core;
using AspNetSkeleton.Core.Hosting;
using AspNetSkeleton.Core.Infrastructure;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AspNetSkeleton.UI.Infrastructure;
using AspNetSkeleton.Core.Utils;

namespace AspNetSkeleton.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = ConfigurationHelper.CreateDefaultBuilder()
                .AddJsonConfigFile("appsettings.json")
#if DISTRIBUTED
                .AddJsonConfigFile("appsettings.Distributed.json")
#else
                .AddJsonConfigFile("appsettings.Monolithic.json")
#endif
                .Build();

            var hostConfiguration = new HostConfiguration(configuration);
            var appConfigurations = new IAppConfiguration[] 
            {
                new AppConfiguration(configuration),
                #if !DISTRIBUTED
                new Api.AppConfiguration(configuration),
                #endif
            };

            var module = new CoreModule(hostConfiguration, appConfigurations);
            var builder = new ContainerBuilder();
            builder.RegisterModule(module);

            using (var container = builder.Build())
            using (var hostScope = container.Resolve<IHostScope>())
                hostScope.Host.Execute(args);
        }
    }
}
