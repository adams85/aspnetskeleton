using System;
using Autofac;

namespace AspNetSkeleton.Core.Infrastructure
{
    public interface ILifetimeScopeFactory
    {
        ILifetimeScope CreateChildScope(object tag, Action<ContainerBuilder, IComponentContext> configuration, bool enablePropertyInjection = true);
    }

    public class LifetimeScopeFactory : ILifetimeScopeFactory
    {
        readonly ILifetimeScope _lifetimeScope;
        readonly PropertyInjectorModule _propertyInjectorModule;

        public LifetimeScopeFactory(ILifetimeScope lifetimeScope, PropertyInjectorModule propertyInjectorModule)
        {
            _lifetimeScope = lifetimeScope;
            _propertyInjectorModule = propertyInjectorModule;
        }

        public ILifetimeScope CreateChildScope(object tag, Action<ContainerBuilder, IComponentContext> configuration, bool enablePropertyInjection = true)
        {
            return _lifetimeScope.BeginLifetimeScope(tag, cb =>
            {
                // WORKAROUND: https://github.com/autofac/Autofac/issues/218
                if (enablePropertyInjection)
                    cb.RegisterModule(_propertyInjectorModule);

                configuration?.Invoke(cb, _lifetimeScope);
            });
        }
    }
}
