using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.Core.Infrastructure;
using Autofac;

namespace AspNetSkeleton.Core.Hosting
{
    public interface IHostScope : IDisposable
    {
        ILifetimeScope LifetimeScope { get; }
        IHost Host { get; }
    }

    public class HostScope : IHostScope
    {
        public HostScope(IHostConfiguration configuration, ILifetimeScopeFactory lifetimeScopeFactory)
        {
            LifetimeScope = lifetimeScopeFactory.CreateChildScope("host", (cb, ctx) =>
            {
                configuration.OnConfigureHost(ctx);
                configuration.RegisterHostComponents(cb);
            });

            _host = new Lazy<IHost>(() => LifetimeScope.Resolve<IHost>(), isThreadSafe: false);
        }

        public void Dispose()
        {
            LifetimeScope.Dispose();
        }

        public ILifetimeScope LifetimeScope { get; }

        readonly Lazy<IHost> _host;
        public IHost Host => _host.Value;
    }
}
