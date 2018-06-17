using System;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetSkeleton.Core.Infrastructure
{
    public class NestedServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        readonly ILifetimeScopeFactory _lifetimeScopeFactory;

        public NestedServiceProviderFactory(ILifetimeScopeFactory lifetimeScopeFactory)
        {
            _lifetimeScopeFactory = lifetimeScopeFactory;
        }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            var scope = _lifetimeScopeFactory.CreateChildScope(new object(),
                (cb, ctx) => cb.Populate(containerBuilder),
                enablePropertyInjection: false);

            return new AutofacServiceProvider(scope);
        }
    }
}
