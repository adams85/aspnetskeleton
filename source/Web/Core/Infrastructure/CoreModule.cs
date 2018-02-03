using AspNetSkeleton.Common;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract;
using Autofac;
using Karambolo.Common.Logging;
using System;
using System.Threading;

namespace AspNetSkeleton.Core.Infrastructure
{
    public abstract class CoreModule : Module
    {
        class Mutex : IStartable, IDisposable
        {
            readonly ILifetimeScope _lifetimeScope;

            public Mutex(ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope;
            }

            public void Start()
            {
                if (Interlocked.CompareExchange(ref rootLifetimeScope, _lifetimeScope, null) != null)
                    throw new InvalidOperationException();
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref rootLifetimeScope, null);
            }
        }

        static ILifetimeScope rootLifetimeScope;
        public static ILifetimeScope RootLifetimeScope => rootLifetimeScope;

        protected virtual bool IsServiceHost => false;

        protected virtual PropertyInjectorModule CreatePropertyInjectorModule()
        {
            return new PropertyInjectorModule();
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(CreatePropertyInjectorModule());

            builder.RegisterType<Mutex>()
                .As<IStartable>()
                .SingleInstance();

            #region Core Infrastructure
            builder
                .RegisterGeneric(typeof(AutofacKeyedProvider<>))
                .As(typeof(IKeyedProvider<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<Clock>()
                .As<IClock>()
                .SingleInstance();

            builder.RegisterType<TraceSourceLogger>()
                .As<ILogger>();
            #endregion

            if (!IsServiceHost)
            {
                builder.RegisterType<ServiceProxyQueryDispatcher>()
                    .As<IQueryDispatcher>()
                    .SingleInstance();

                builder.RegisterType<ServiceProxyCommandDispatcher>()
                    .As<ICommandDispatcher>()
                    .SingleInstance();
            }
        }
    }
}
