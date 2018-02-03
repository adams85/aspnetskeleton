using System;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure
{
    public class QueryInterceptorContext
    {
        public IQuery Query { get; set; }
        public Type QueryType { get; set; }
        public Type ResultType { get; set; }

        public bool TryGet<TQuery>(out TQuery command)
            where TQuery : IQuery
        {
            if (QueryType == typeof(TQuery))
            {
                command = (TQuery)Query;
                return true;
            }
            else
            {
                command = default(TQuery);
                return false;
            }
        }
    }

    public interface IQueryInterceptor
    {
        Task<object> ExecuteAsync(QueryInterceptorContext context, CancellationToken cancellationToken);
    }

    public delegate IQueryInterceptor QueryInterceptorFactory(IQueryInterceptor target);

    public abstract class QueryInterceptor : IQueryInterceptor
    {
        protected QueryInterceptor(IQueryInterceptor target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Target = target;
        }

        protected IQueryInterceptor Target { get; }

        public abstract Task<object> ExecuteAsync(QueryInterceptorContext context, CancellationToken cancellationToken);
    }
}
