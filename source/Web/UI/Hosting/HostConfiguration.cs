using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.Base;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Core;
using AspNetSkeleton.Core.Hosting;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AspNetSkeleton.UI.Hosting
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
