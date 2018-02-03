using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspNetSkeleton.Core.Infrastructure;
using Autofac;

namespace AspNetSkeleton.Core
{
    public interface IAppScope : IDisposable
    {
        ILifetimeScope LifetimeScope { get; }
        IApp App { get; }
    }

    public class AppScope : IAppScope
    {
        public static IAppScope Current { get; private set; }

        public AppScope(IEnumerable<IAppConfiguration> configurations, ILifetimeScopeFactory lifetimeScopeFactory)
        {
            if (Current != null)
                throw new NotSupportedException("Concurrently hosting multiple applications is not supported.");

            LifetimeScope = lifetimeScopeFactory.CreateChildScope("app", (cb, ctx) =>
            {
                foreach (var configuration in configurations)
                {
                    configuration.OnConfigureApp(ctx);
                    configuration.RegisterAppComponents(cb);
                }
            });

            _app = new Lazy<IApp>(() => LifetimeScope.Resolve<IApp>(), isThreadSafe: false);

            Current = this;
        }

        public void Dispose()
        {
            Current = null;

            LifetimeScope.Dispose();
        }

        public ILifetimeScope LifetimeScope { get; }

        readonly Lazy<IApp> _app;
        public IApp App => _app.Value;
    }
}
