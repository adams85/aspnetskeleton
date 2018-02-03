using Karambolo.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.DataAccess
{
    public delegate void QueryableIncludeMonitor(IQueryable queryable, string path);

    public interface IIncludeMonitorQueryProvider
    {
        void RegisterMonitor(QueryableIncludeMonitor monitor);
        void UnregisterMonitor(QueryableIncludeMonitor monitor);
        void OnInclude(IQueryable queryable, string path);
    }

    public static class QueryableExtensions
    {
        class EFQueryableAdapter : IQueryable, IOrderedQueryable, IDbAsyncEnumerable
        {
            struct AsyncEnumerator : IDbAsyncEnumerator
            {
                readonly IEnumerator _enumerator;

                public AsyncEnumerator(IEnumerator enumerator)
                {
                    _enumerator = enumerator;
                }

                public object Current => _enumerator.Current;

                public void Dispose()
                {
                    if (_enumerator is IDisposable disposable)
                        disposable.Dispose();
                }

                public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return TaskUtils.FromCancelled<bool>(cancellationToken);

                    return Task.FromResult(_enumerator.MoveNext());
                }
            }

            class AsyncQueryProvider : IDbAsyncQueryProvider
            {
                readonly IQueryProvider _provider;

                public AsyncQueryProvider(IQueryProvider provider)
                {
                    _provider = provider;
                }

                public IQueryable CreateQuery(Expression expression)
                {
                    throw new InvalidOperationException();
                }

                public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                {
                    throw new InvalidOperationException();
                }

                public object Execute(Expression expression)
                {
                    throw new InvalidOperationException();
                }

                public TResult Execute<TResult>(Expression expression)
                {
                    throw new InvalidOperationException();
                }

                public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
                {
                    return Task.Run(() => Execute(expression), cancellationToken).AsCancellable(cancellationToken);
                }

                public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
                {
                    return Task.Run(() => Execute<TResult>(expression), cancellationToken).AsCancellable(cancellationToken);
                }
            }

            class IncludeMonitorQueryProvider : IIncludeMonitorQueryProvider
            {
                public void OnInclude(IQueryable queryable, string path)
                {
                    Monitors?.Invoke(queryable, path);
                }

                public void RegisterMonitor(QueryableIncludeMonitor monitor)
                {
                    Monitors += monitor;
                }

                public void UnregisterMonitor(QueryableIncludeMonitor monitor)
                {
                    Monitors -= monitor;
                }

                event QueryableIncludeMonitor Monitors;
            }

            internal class ProviderDecorator : IQueryProvider, IDbAsyncQueryProvider, IIncludeMonitorQueryProvider
            {
                readonly IQueryProvider _provider;
                readonly IDbAsyncQueryProvider _asyncQueryProvider;
                readonly IIncludeMonitorQueryProvider _includeMonitorQueryProvider;

                public ProviderDecorator(IQueryProvider provider)
                {
                    _provider = provider;
                    _asyncQueryProvider = provider as IDbAsyncQueryProvider ?? new AsyncQueryProvider(provider);
                    _includeMonitorQueryProvider = provider as IIncludeMonitorQueryProvider ?? new IncludeMonitorQueryProvider();
                }

                public IQueryable CreateQuery(Expression expression)
                {
                    return new EFQueryableAdapter(_provider.CreateQuery(expression), this);
                }

                public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                {
                    return new EFQueryableAdapter<TElement>(_provider.CreateQuery<TElement>(expression), this);
                }

                public object Execute(Expression expression)
                {
                    return _provider.Execute(expression);
                }

                public TResult Execute<TResult>(Expression expression)
                {
                    return _provider.Execute<TResult>(expression);
                }

                public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
                {
                    return _asyncQueryProvider.ExecuteAsync(expression, cancellationToken);
                }

                public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
                {
                    return _asyncQueryProvider.ExecuteAsync<TResult>(expression, cancellationToken);
                }

                public void OnInclude(IQueryable queryable, string path)
                {
                    _includeMonitorQueryProvider.OnInclude(queryable, path);
                }

                public void RegisterMonitor(QueryableIncludeMonitor monitor)
                {
                    _includeMonitorQueryProvider.RegisterMonitor(monitor);
                }

                public void UnregisterMonitor(QueryableIncludeMonitor monitor)
                {
                    _includeMonitorQueryProvider.UnregisterMonitor(monitor);
                }
            }

            protected readonly IQueryable _source;
            protected readonly ProviderDecorator _provider;

            internal EFQueryableAdapter(IQueryable source, ProviderDecorator provider)
            {
                _source = source;
                _provider = provider;
            }

            public EFQueryableAdapter(IQueryable source)
                : this(source, new ProviderDecorator(source.Provider)) { }

            public Expression Expression => _source.Expression;

            public Type ElementType => _source.ElementType;

            public IQueryProvider Provider => _provider;

            public IQueryable Include(string path)
            {
                _provider.OnInclude(this, path);
                return this;
            }

            public IEnumerator GetEnumerator()
            {
                return _source.GetEnumerator();
            }

            public IDbAsyncEnumerator GetAsyncEnumerator()
            {
                return new AsyncEnumerator(GetEnumerator());
            }
        }

        class EFQueryableAdapter<T> : EFQueryableAdapter, IQueryable<T>, IOrderedQueryable<T>, IDbAsyncEnumerable<T>
        {
            struct AsyncEnumerator : IDbAsyncEnumerator<T>
            {
                readonly IEnumerator<T> _enumerator;

                public AsyncEnumerator(IEnumerator<T> enumerator)
                {
                    _enumerator = enumerator;
                }

                public T Current => _enumerator.Current;

                object IDbAsyncEnumerator.Current => Current;

                public void Dispose()
                {
                    _enumerator?.Dispose();
                }

                public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return TaskUtils.FromCancelled<bool>(cancellationToken);

                    return Task.FromResult(_enumerator.MoveNext());
                }
            }

            internal EFQueryableAdapter(IQueryable<T> source, ProviderDecorator provider) : base(source, provider) { }

            public EFQueryableAdapter(IQueryable<T> source) : base(source) { }

            public new IQueryable<T> Include(string path)
            {
                _provider.OnInclude(this, path);
                return this;
            }

            public new IEnumerator<T> GetEnumerator()
            {
                return ((IQueryable<T>)_source).GetEnumerator();
            }

            public new IDbAsyncEnumerator<T> GetAsyncEnumerator()
            {
                return new AsyncEnumerator(GetEnumerator());
            }
        }

        public static IQueryable EnableAsync(this IQueryable source)
        {
            return source.Provider is IDbAsyncQueryProvider ? source : new EFQueryableAdapter(source);
        }

        public static IQueryable<T> EnableAsync<T>(this IQueryable<T> source)
        {
            return source.Provider is IDbAsyncQueryProvider ? source : new EFQueryableAdapter<T>(source);
        }

        public static IQueryable RegisterIncludeMonitor(this IQueryable source, QueryableIncludeMonitor monitor)
        {
            if (!(source.Provider is IIncludeMonitorQueryProvider provider))
                provider = (IIncludeMonitorQueryProvider)((source = new EFQueryableAdapter(source)).Provider);

            provider.RegisterMonitor(monitor);

            return source;
        }

        public static IQueryable UnregisterIncludeMonitor(this IQueryable source, QueryableIncludeMonitor monitor)
        {
            if (source.Provider is IIncludeMonitorQueryProvider provider)
                provider.UnregisterMonitor(monitor);

            return source;
        }

        public static IQueryable<T> RegisterIncludeMonitor<T>(this IQueryable<T> source, QueryableIncludeMonitor monitor)
        {
            if (!(source.Provider is IIncludeMonitorQueryProvider provider))
                provider = (IIncludeMonitorQueryProvider)((source = new EFQueryableAdapter<T>(source)).Provider);

            provider.RegisterMonitor(monitor);

            return source;
        }

        public static IQueryable<T> UnregisterIncludeMonitor<T>(this IQueryable<T> source, QueryableIncludeMonitor monitor)
        {
            if (source.Provider is IIncludeMonitorQueryProvider provider)
                provider.UnregisterMonitor(monitor);

            return source;
        }
    }
}
