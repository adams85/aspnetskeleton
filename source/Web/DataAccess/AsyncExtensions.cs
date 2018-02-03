using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.DataAccess
{
    public interface IQueryableDecorator : IQueryable, IOrderedQueryable
    {
        IQueryable Target { get; }
    }

    public interface IQueryableDecorator<T> : IQueryableDecorator, IQueryable<T>, IOrderedQueryable<T>
    {
        new IQueryable<T> Target { get; }
    }

    public static partial class AsyncExtensions
    {
        static IQueryable<T> Unwrap<T>(IQueryable<T> source)
        {
            IQueryable<T> target;
            while (source is IQueryableDecorator<T> decorator && (target = decorator.Target) != source)
                source = target;

            return source;
        }

        #region ForEachAsync
        public static Task ForEachAsync<TSource>(
            this IQueryable<TSource> source, Action<TSource> action,
            CancellationToken token = default(CancellationToken))
        {
            return LinqToDB.AsyncExtensions.ForEachAsync(Unwrap(source), action, token);
        }
        
        public static Task ForEachUntilAsync<TSource>(
            this IQueryable<TSource> source, Func<TSource, bool> func,
            CancellationToken token = default(CancellationToken))
        {
            return LinqToDB.AsyncExtensions.ForEachUntilAsync(Unwrap(source), func, token);
        }
        #endregion

        #region ToListAsync
        public static Task<List<TSource>> ToListAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
        {
            return LinqToDB.AsyncExtensions.ToListAsync(Unwrap(source), token);
        }
        #endregion

        #region ToArrayAsync
        public static Task<TSource[]> ToArrayAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
        {
            return LinqToDB.AsyncExtensions.ToArrayAsync(Unwrap(source), token);
        }
        #endregion

        #region ToDictionaryAsync
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken token = default(CancellationToken))
        {
            return LinqToDB.AsyncExtensions.ToDictionaryAsync(Unwrap(source), keySelector, token);
        }

        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken token = default(CancellationToken))
        {
            return LinqToDB.AsyncExtensions.ToDictionaryAsync(Unwrap(source), keySelector, comparer, token);
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken token = default(CancellationToken))
        {
            return LinqToDB.AsyncExtensions.ToDictionaryAsync(Unwrap(source), keySelector, elementSelector, token);
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken token = default(CancellationToken))
        {
            return LinqToDB.AsyncExtensions.ToDictionaryAsync(Unwrap(source), keySelector, elementSelector, comparer, token);
        }
        #endregion
    }
}
