using System;
using System.Collections.Generic;
using System.Linq;
using AspNetSkeleton.Common.Utils;
using System.Threading.Tasks;
using System.Threading;
using Karambolo.Common;
using AspNetSkeleton.Core.Infrastructure;
using AspNetSkeleton.Core.Infrastructure.Caching;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.Caching
{
    public abstract class QueryCachingOptions : CacheOptions
    {
        public abstract bool IsCached(QueryInterceptorContext context);
        public abstract IEnumerable<string> GetScopes(QueryInterceptorContext context);
    }

    public class QueryCacherInterceptor : QueryInterceptor
    {
        readonly QueryCachingOptions _options;

        public static string GetCacheScope(Type queryType, params string[] subScopes)
        {
            return string.Join("|", subScopes.WithHead(queryType.FullName));
        }

        public QueryCacherInterceptor(ICache cache, IQueryInterceptor target, QueryCachingOptions options)
            : base(target)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Cache = cache;
            _options = options;
        }

        protected ICache Cache { get; }       

        protected virtual string GetCacheKey(QueryInterceptorContext context)
        {
            // TODO: use hash if short keys are supported only by the cache impl. or a large amount of objects needs to be cached
            return string.Concat(context.QueryType.FullName, "|", SerializationUtils.SerializeObject(context.Query));
        }

        protected virtual bool IsCached(QueryInterceptorContext context)
        {
            return _options.IsCached(context);
        }

        protected virtual IEnumerable<string> GetScopes(QueryInterceptorContext context)
        {
            return _options.GetScopes(context);
        }

        public override Task<object> ExecuteAsync(QueryInterceptorContext context, CancellationToken cancellationToken)
        {
            return
                IsCached(context) ?
                Cache.GetOrAddAsync(GetCacheKey(context), (k, ct) => Target.ExecuteAsync(context, ct), _options, cancellationToken, GetScopes(context).ToArray()) :
                Target.ExecuteAsync(context, cancellationToken);
        }
    }
}
