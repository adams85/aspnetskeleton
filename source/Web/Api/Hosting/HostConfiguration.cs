using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Core.Hosting;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace AspNetSkeleton.Api.Hosting
{
    public class HostConfiguration : HostConfigurationBase
    {
        public HostConfiguration(IConfigurationRoot configuration) : base(configuration) { }

        public override void RegisterHostComponents(ContainerBuilder builder)
        {
            base.RegisterHostComponents(builder);

            builder.RegisterInstance(ConsoleHostIO.Instance)
                .As<IOperationHostIO>();

            builder.RegisterType<Host>()
                .As<IHost>()
                .As<IOperationHost>()
                .As<IOperationContext>()
                .SingleInstance();

            builder.RegisterType<HostWindowsService>()
                .As<IHostWindowsService>()
                .SingleInstance();
        }
    }
}
