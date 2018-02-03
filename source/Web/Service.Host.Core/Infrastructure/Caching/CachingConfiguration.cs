using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.Queries;
using Autofac;
using System;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.Caching
{
    public static class CachingConfiguration
    {
        public static readonly TimeSpan DefaultSlidingCacheExpiration = TimeSpan.FromMinutes(10);

        public static void ConfigureQueryCaching(this ContainerBuilder builder)
        {
            var configurer = new QueryCachingConfigurer();

            configurer.Cache<GetAccountInfoQuery>()
                .WithScope(q => q.UserName)
                .InvalidatedBy<AddUsersToRolesCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<RemoveUsersFromRolesCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<DeleteRoleCommand>()
                .WithSlidingExpiration(DefaultSlidingCacheExpiration);

            configurer.Configure(builder);
        }
    }
}
