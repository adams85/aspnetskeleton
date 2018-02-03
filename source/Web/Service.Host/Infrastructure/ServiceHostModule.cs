using System.Linq;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Service.Host.Core.Infrastructure;
using AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork;
using Autofac;
using Autofac.Integration.WebApi;
using Karambolo.Common;
using System.ServiceProcess;
using System.ComponentModel;
using AspNetSkeleton.Service.Host.Operations;

namespace AspNetSkeleton.Service.Host.Infrastructure
{
    public class ServiceHostModule : ServiceHostCoreModule
    {
        public static void Configure(ContainerBuilder builder)
        {
            // Register our dependencies
            builder.RegisterModule(new ServiceHostModule());

            builder.RegisterApiControllers(typeof(Program).Assembly);

            // useless because of filter caching
            // http://stackoverflow.com/questions/23659108/webapi-autofac-system-web-http-filters-actionfilterattribute-instance-per-requ
            // builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<ServiceHostSettings>()
                .AsImplementedInterfaces()
                .SingleInstance();

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

            builder.RegisterInstance(ConsoleHostIO.Instance)
                .As<IOperationHostIO>();

            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Keyed<Operation>(t => t.GetAttributes<HandlerForAttribute>().First().Key)
                .WithMetadataFrom<DisplayNameAttribute>();


            builder.RegisterType<Program>()
                .As<IOperationHost>()
                .As<IOperationContext>()
                .As<IServiceHost>()
                .As<IShutDownTokenAccessor>()
                .SingleInstance();

            builder.RegisterType<WindowsServiceManager>()
                .As<IWindowsServiceManager>()
                .SingleInstance();

            builder.RegisterType<WindowsService>()
                .As<ServiceBase>()
                .SingleInstance();

            builder.RegisterType<ConsoleServiceHostEnvironment>()
                .As<IServiceHostEnvironment>()
                .SingleInstance();
        }
    }
}