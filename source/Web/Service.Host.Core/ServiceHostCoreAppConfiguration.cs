using System.IO;
using System.Linq;
using AspNetSkeleton.Base;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Core;
using AspNetSkeleton.Core.Infrastructure.Caching;
using AspNetSkeleton.DataAccess;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Host.Core.Handlers.Mails;
using AspNetSkeleton.Service.Host.Core.Infrastructure;
using AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork;
using AspNetSkeleton.Service.Host.Core.Infrastructure.Caching;
using Autofac;
using Karambolo.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;

namespace AspNetSkeleton.Service.Host.Core
{
    public abstract class ServiceHostCoreAppConfiguration : AppConfigurationBase
    {
        class DbConfigurationProviderAdapter : IDbConfigurationProvider, IAppInitializer
        {
            readonly IDbConfigurationProvider _target;

            public DbConfigurationProviderAdapter(IDbConfigurationProvider target)
            {
                _target = target;
            }

            public void Initialize()
            {
                _target.Initialize();
            }

            public string ProvideFor<TContext>() where TContext : IDbContext
            {
                return _target.ProvideFor<TContext>();
            }
        }

        public static readonly object QueryLifetimeScopeTag = typeof(IQuery);
        public static readonly object CommandLifetimeScopeTag = typeof(ICommand);

        public ServiceHostCoreAppConfiguration(IConfigurationRoot configuration) : base(configuration) { }

        public override void RegisterCommonServices(IServiceCollection services)
        {
            base.RegisterCommonServices(services);

            services.ConfigureByConvention<ServiceHostCoreSettings>(Configuration);
            services.ConfigureByConvention<DbConfiguration>(Configuration);
            services.ConfigureByConvention<MailSettings>(Configuration);
        }

        public override void RegisterAppComponents(ContainerBuilder builder)
        {
            base.RegisterAppComponents(builder);

            #region Data Access
            builder.RegisterType<DbConfigurationProvider>()
                .As<IDbConfigurationProvider>()
                .SingleInstance();

            builder.RegisterAdapter<IDbConfigurationProvider, IAppInitializer>(f => new DbConfigurationProviderAdapter(f));

            builder.RegisterType<DataContext>()
                .Keyed<IReadOnlyDbContext>(typeof(DataContext))
                .Keyed<IReadWriteDbContext>(typeof(DataContext))
                .Keyed<IDbContext>(typeof(DataContext));
            #endregion

            #region Queries
            builder.RegisterType<QueryDispatcher>()
                .As<IQueryDispatcher>()
                // crucial to register per lifetime scope to enable nested queries!
                .InstancePerLifetimeScope();

            builder.RegisterType<QueryContext>()
                .As<IQueryContext>()
                .InstancePerMatchingLifetimeScope(QueryLifetimeScopeTag);

            builder.RegisterAssemblyTypes(typeof(IQueryHandler<,>).Assembly)
                .AsClosedTypesOf(typeof(IQueryHandler<,>));
            #endregion

            #region Commands
            builder.RegisterType<CommandDispatcher>()
                .As<ICommandDispatcher>()
                .InstancePerLifetimeScope();

            builder.RegisterType<CommandContext>()
                .As<ICommandContext>()
                // crucial to register per lifetime scope to enable nested commands!
                .InstancePerMatchingLifetimeScope(CommandLifetimeScopeTag);

            builder.RegisterAssemblyTypes(typeof(ICommandHandler<>).Assembly)
                .AsClosedTypesOf(typeof(ICommandHandler<>));
            #endregion

            #region Caching
            builder.RegisterType<InProcessCache>()
                .As<ICache>()
                .SingleInstance();

            builder.ConfigureQueryCaching(CommonContext);
            #endregion

            #region Templating
            builder.Register(ctx =>
                new RazorLightEngineBuilder()
                    .UseFilesystemProject(Path.Combine(ctx.Resolve<IAppEnvironment>().AppBasePath, "Templates"))
                    .Build())
                .As<IRazorLightEngine>();
            #endregion

            #region Mail Management
            builder.RegisterType<MailSenderProcess>()
                .As<IBackgroundProcess>();

            builder.RegisterAssemblyTypes(typeof(NotificationHandler<>).Assembly)
                .As<INotificationHandler>()
                .Keyed<INotificationHandler>(t => t.GetAttributes<HandlerForAttribute>().First().Key);
            #endregion
        }
    }
}
