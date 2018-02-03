using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.Queries;
using Autofac;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.Caching
{
    public static class CachingConfiguration
    {
        public static void ConfigureQueryCaching(this ContainerBuilder builder, IComponentContext context)
        {
            var options = context.Resolve<IOptions<ServiceHostCoreSettings>>().Value;

            var configurer = new QueryCachingConfigurer();

            configurer.Cache<GetAccountInfoQuery>()
                .WithScope(q => q.UserName)
                .InvalidatedBy<ApproveUserCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<LockUserCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<UnlockUserCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<ChangePasswordCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<RegisterUserActivityCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<DeleteUserCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<AddUsersToRolesCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<RemoveUsersFromRolesCommand, GetAccountInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<DeleteRoleCommand>()
                .WithSlidingExpiration(options.DefaultCacheSlidingExpiration);

            configurer.Configure(builder);
        }
    }
}
