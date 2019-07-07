using System;
using AspNetSkeleton.Service.Contract.Commands;
using System.Threading.Tasks;
using System.Threading;
using Karambolo.Common;
using AspNetSkeleton.Core.Infrastructure.Caching;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.Caching
{
    public class CachedQueryInvalidatorInterceptor : CommandInterceptor
    {
        readonly Type[] _queryTypes;

        public CachedQueryInvalidatorInterceptor(ICache cache, ICommandInterceptor target, Type[] queryTypes)
            : base(target)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (queryTypes == null)
                throw new ArgumentNullException(nameof(queryTypes));

            Cache = cache;
            _queryTypes = queryTypes ?? ArrayUtils.Empty<Type>();
        }

        protected ICache Cache { get; }

        protected virtual Task InvalidateQueryCacheAsync(CommandInterceptorContext context, Type queryType, CancellationToken cancellationToken)
        {
            return Cache.RemoveScopeAsync(QueryCacherInterceptor.GetCacheScope(queryType), cancellationToken);
        }

        public override async Task ExecuteAsync(CommandInterceptorContext context, CancellationToken cancellationToken)
        {
            await Target.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

            var tasks = Array.ConvertAll(_queryTypes, qt => InvalidateQueryCacheAsync(context, qt, cancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    public class GetAccountInfoQueryInvalidatorInterceptor : CachedQueryInvalidatorInterceptor
    {
        public GetAccountInfoQueryInvalidatorInterceptor(ICache cache, ICommandInterceptor target, Type[] queryTypes)
            : base(cache, target, queryTypes) { }

        protected string[] GetAffectedUserNames(CommandInterceptorContext context)
        {
            if (context.TryGet(out ApproveUserCommand approveUserCommand))
                return new[] { approveUserCommand.UserName };

            if (context.TryGet(out LockUserCommand lockUserCommand))
                return new[] { lockUserCommand.UserName };

            if (context.TryGet(out UnlockUserCommand unlockUserCommand))
                return new[] { unlockUserCommand.UserName };

            if (context.TryGet(out ChangePasswordCommand changePasswordCommand))
                return changePasswordCommand.Verify ? new[] { changePasswordCommand.UserName } : null;

            if (context.TryGet(out RegisterUserActivityCommand registerUserActivityCommand))
                return registerUserActivityCommand.SuccessfulLogin == false ? new[] { registerUserActivityCommand.UserName } : null;

            if (context.TryGet(out DeleteUserCommand deleteUserCommand))
                return new[] { deleteUserCommand.UserName };

            if (context.TryGet(out AddUsersToRolesCommand addUsersToRolesCommand))
                return addUsersToRolesCommand.UserNames;

            if (context.TryGet(out RemoveUsersFromRolesCommand removeUsersFromRolesCommand))
                return removeUsersFromRolesCommand.UserNames;

            throw new NotImplementedException();
        }

        protected override Task InvalidateQueryCacheAsync(CommandInterceptorContext context, Type queryType, CancellationToken cancellationToken)
        {
            var userNames = GetAffectedUserNames(context);

            if (!ArrayUtils.IsNullOrEmpty(userNames))
            {
                var tasks = Array.ConvertAll(userNames, un => Cache.RemoveScopeAsync(QueryCacherInterceptor.GetCacheScope(queryType, un), cancellationToken));
                return Task.WhenAll(tasks);
            }
            else
                return Task.FromResult<object>(null);
        }
    }
}
