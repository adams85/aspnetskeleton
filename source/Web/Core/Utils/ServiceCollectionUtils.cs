using System;
using System.Collections.Generic;
using System.Text;
using Karambolo.Common;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetSkeleton.Core.Utils
{
    public static class ServiceCollectionUtils
    {
        public static readonly IEqualityComparer<ServiceDescriptor> DescriptorEqualityComparer = new DelegatedEqualityComparer<ServiceDescriptor>(
            (x, y) =>
                x.Lifetime == y.Lifetime &&
                x.ServiceType == y.ServiceType &&
                x.ImplementationType == y.ImplementationType &&
                x.ImplementationInstance == y.ImplementationInstance &&
                x.ImplementationFactory == y.ImplementationFactory,
            x => 
                x.Lifetime.GetHashCode() ^ 
                x.ServiceType.GetHashCode());

        public static IServiceCollection Clone(this IServiceCollection services)
        {
            IServiceCollection result = new ServiceCollection();

            var n = services.Count;
            for (var i = 0; i < n; i++)
                result.Add(services[i]);

            return result;
        }

        public static void Remove(this IServiceCollection services, Func<ServiceDescriptor, bool> match)
        {
            var n = services.Count;
            for (var i = n - 1; i >= 0; i--)
                if (match(services[i]))
                    services.RemoveAt(i);
        }
    }
}
