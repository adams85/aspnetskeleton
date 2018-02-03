using System.Data.Entity;
using System.Linq;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Core.Infrastructure;
using AspNetSkeleton.Core.Infrastructure.Caching;
using AspNetSkeleton.DataAccess;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Host.Core.Handlers.Mails;
using AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork;
using AspNetSkeleton.Service.Host.Core.Infrastructure.Caching;
using Autofac;
using Karambolo.Common;
using RazorEngine;
using RazorEngine.Templating;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure
{
    public abstract class ServiceHostCoreModule : CoreModule
    {
        class DbConfigurer : IStartable
        {
            public void Start()
            {
                Database.SetInitializer(new CheckIfDatabaseCompatible<DataContext>());
            }
        }

        public static readonly object QueryLifetimeScopeTag = typeof(IQuery);
        public static readonly object CommandLifetimeScopeTag = typeof(ICommand);

        protected sealed override bool IsServiceHost => true;

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            #region Data Access
            builder.RegisterType<DbConfigurer>()
                .As<IStartable>()
                .SingleInstance();

            builder.Register(ctx =>
            {
                var service = new DataContext();
                service.Configuration.AutoDetectChangesEnabled = true;
                service.Configuration.LazyLoadingEnabled = false;
                service.Configuration.ProxyCreationEnabled = false;
                service.Configuration.ValidateOnSaveEnabled = false;

#if DEBUG && LOG_DB
                service.Database.Log = t => System.Diagnostics.Debug.WriteLine(t);
#endif

                return service;

            })
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
                // crucial to register per lifetime scope to enable nested commands!
                .InstancePerLifetimeScope();

            builder.RegisterType<CommandContext>()
                .As<ICommandContext>()
                .InstancePerMatchingLifetimeScope(CommandLifetimeScopeTag);

            builder.RegisterAssemblyTypes(typeof(ICommandHandler<>).Assembly)
                .AsClosedTypesOf(typeof(ICommandHandler<>));
            #endregion

            #region Caching
            builder.RegisterType<InProcessCache>()
                .As<ICache>()
                .SingleInstance();


            builder.ConfigureQueryCaching();
            #endregion

            builder.RegisterInstance(Engine.Razor)
                .As<IRazorEngineService>();

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
