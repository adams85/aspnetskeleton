using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.Base;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Core.Hosting;
using AspNetSkeleton.Core.Hosting.Operations;
using AspNetSkeleton.Core.Infrastructure;
using Autofac;
using Karambolo.Extensions.Logging.File;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetSkeleton.Core.Hosting
{
    public interface IHostConfiguration : IContainerConfiguration
    {
        void OnConfigureHost(IComponentContext context);
        void RegisterHostComponents(ContainerBuilder builder);
    }

    public abstract class HostConfigurationBase : IHostConfiguration
    {
        protected HostConfigurationBase(IConfigurationRoot configuration)
        {
            Configuration = configuration;
        }

        public IConfigurationRoot Configuration { get; }

        protected IComponentContext CommonContext { get; private set; }

        public virtual void RegisterCommonServices(IServiceCollection services) { }

        public virtual void RegisterCommonComponents(ContainerBuilder builder) { }

        public void OnConfigureHost(IComponentContext context)
        {
            CommonContext = context;
        }

        public virtual void RegisterHostComponents(ContainerBuilder builder)
        {
            foreach (var operationDescriptor in OperationDescriptor.Scan(typeof(ConsoleOperation).Assembly.GetTypes()))
            {
                builder.RegisterType(operationDescriptor.Type)
                    .Keyed<Operation>(operationDescriptor.Name);

                builder.Register(ctx =>
                {
                    operationDescriptor.Factory = ctx.ResolveKeyed<OperationFactory>(operationDescriptor.Name);
                    return operationDescriptor;
                })
                    .As<OperationDescriptor>()
                    .SingleInstance();
            }

            builder.RegisterType<WindowsServiceManager>()
                .As<IWindowsServiceManager>();
        }
    }
}
