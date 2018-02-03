using System;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetSkeleton.Common.Utils
{
    public static class QueryableUtils
    {
        static IOrderedQueryable<T> OrderByCore<T>(this IQueryable<T> source, string keyPath, bool descending, bool nested)
        {
            if (keyPath == null)
                throw new ArgumentNullException(nameof(keyPath));

            if (keyPath.Length == 0)
                throw new ArgumentException(null, nameof(keyPath));

            var type = typeof(T);
            var @param = Expression.Parameter(type);

            Expression propertyAccess = @param;
            var propertyNames = keyPath.Split('.');
            var propertyCount = propertyNames.Length;
            for (var i = 0; i < propertyCount; i++)
                propertyAccess = Expression.Property(propertyAccess, propertyNames[i]);

            var keySelector = Expression.Lambda(propertyAccess, @param);

            var orderQuery = Expression.Call(
                typeof(Queryable),
                !nested ?
                (!descending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending)) :
                (!descending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending)),
                new[] { type, propertyAccess.Type },
                source.Expression, Expression.Quote(keySelector));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(orderQuery);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string keyPath, bool descending = false)
        {
            return source.OrderByCore(keyPath, descending, nested: false);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string keyPath, bool descending = false)
        {
            return source.OrderByCore(keyPath, descending, nested: true);
        }
    }
}
