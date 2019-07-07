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

        public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> source, params string[] orderColumns)
        {
            if (orderColumns == null)
                throw new ArgumentNullException(nameof(orderColumns));

            var n = orderColumns.Length;
            for (var i = 0; i < n; i++)
            {
                var orderColumn = orderColumns[i];

                if (string.IsNullOrEmpty(orderColumn))
                    throw new ArgumentException(null, nameof(orderColumns));

                var descending = ParseOrderColumn(orderColumn, out string columnName);
                source = ApplyColumnOrder(source, columnName, descending, nested: i > 0);
            }

            return source;

            bool ParseOrderColumn(string value, out string cn)
            {
                var c = value[0];
                switch (c)
                {
                    case '+':
                    case '-':
                        cn = value.Substring(1);
                        return c == '-';
                    default:
                        cn = value;
                        return false;
                }
            }

            IOrderedQueryable<T> ApplyColumnOrder(IQueryable<T> src, string columnName, bool descending, bool nested)
            {
                return nested ? ((IOrderedQueryable<T>)src).ThenBy(columnName, descending) : src.OrderBy(columnName, descending);
            }
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            return source.Skip(pageIndex * pageSize).Take(pageSize);
        }
    }
}
