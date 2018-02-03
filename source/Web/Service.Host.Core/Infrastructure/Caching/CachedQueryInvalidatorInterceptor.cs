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
            var tasks = Array.ConvertAll(_queryTypes, qt => InvalidateQueryCacheAsync(context, qt, cancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);

            await Target.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    public class GetAccountInfoQueryInvalidatorInterceptor : CachedQueryInvalidatorInterceptor
    {
        public GetAccountInfoQueryInvalidatorInterceptor(ICache cache, ICommandInterceptor target, Type[] queryTypes)
            : base(cache, target, queryTypes) { }

        protected string[] GetAffectedUserNames(CommandInterceptorContext context)
        {
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
