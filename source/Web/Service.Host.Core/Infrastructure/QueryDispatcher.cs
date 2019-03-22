using System;
using System.Reflection;
using Autofac;
using System.Linq;
using System.Collections.Generic;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Common.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using Karambolo.Common;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure
{
    public class QueryDispatcher : IQueryDispatcher, IQueryInterceptor
    {
        readonly ILifetimeScope _lifetimeScope;
        readonly IKeyedProvider<IEnumerable<QueryInterceptorFactory>> _interceptorFactoriesProvider;

        public QueryDispatcher(ILifetimeScope lifetimeScope, IKeyedProvider<IEnumerable<QueryInterceptorFactory>> interceptorFactoriesProvider)
        {
            _lifetimeScope = lifetimeScope;
            _interceptorFactoriesProvider = interceptorFactoriesProvider;
        }

        static readonly MethodInfo invokeHandlerMethodDefinition = Lambda.Method(() => InvokeHandlerAsync<IQuery<object>, object>(null, null, default(CancellationToken))).GetGenericMethodDefinition();
        static readonly MethodInfo getTaskResultMethodDefinition = Lambda.Method(() => GetTaskResult<object>(null)).GetGenericMethodDefinition();

        static Task<TResult> InvokeHandlerAsync<TQuery, TResult>(ILifetimeScope lifetimeScope, TQuery query, CancellationToken cancellationToken)
            where TQuery : IQuery<TResult>
        {
            var handler = lifetimeScope.Resolve<IQueryHandler<TQuery, TResult>>();
            return handler.HandleAsync(query, cancellationToken);
        }

        static TResult GetTaskResult<TResult>(Task<TResult> task)
        {
            return task.Result;
        }

        public Task<object> DispatchAsync(IQuery query, CancellationToken cancellationToken)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var actualQueryType = Query.GetActualTypeFor(query.GetType());
            var interfaceType = Query.GetInterfaceTypeFor(actualQueryType);
            if (interfaceType == null)
                throw new ArgumentException(null, nameof(query));

            var interceptorFactories = _interceptorFactoriesProvider.ProvideFor(actualQueryType)
                .Concat(_interceptorFactoriesProvider.ProvideFor(KeyedProvider.Default));

            IQueryInterceptor interceptor = this;
            interceptor = interceptorFactories.Aggregate(interceptor, (ic, fac) => fac(ic));

            var context = new QueryInterceptorContext
            {
                Query = query,
                QueryType = actualQueryType,
                ResultType = interfaceType.GetGenericArguments()[0],
            };

            return interceptor.ExecuteAsync(context, cancellationToken);
        }

        public async Task<object> ExecuteAsync(QueryInterceptorContext context, CancellationToken cancellationToken)
        {
            var invokeHandlerMethod = invokeHandlerMethodDefinition.MakeGenericMethod(context.QueryType, context.ResultType);
            var getTaskResultMethod = getTaskResultMethodDefinition.MakeGenericMethod(context.ResultType);

            var isNestedQuery = _lifetimeScope.Tag == ServiceHostCoreAppConfiguration.QueryLifetimeScopeTag;
            var queryLifetimeScope =
                !isNestedQuery ?
                _lifetimeScope.BeginLifetimeScope(ServiceHostCoreAppConfiguration.QueryLifetimeScopeTag) :
                _lifetimeScope;

            try
            {
                var task = (Task)invokeHandlerMethod.Invoke(null, new object[] { queryLifetimeScope, context.Query, cancellationToken });
                await task.ConfigureAwait(false);
                return getTaskResultMethod.Invoke(null, new[] { task });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
            finally
            {
                if (!isNestedQuery)
                    queryLifetimeScope.Dispose();
            }
        }

        public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            return (TResult)await DispatchAsync((IQuery)query, cancellationToken).ConfigureAwait(false);
        }
    }
}