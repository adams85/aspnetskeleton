using AspNetSkeleton.Common.Infrastructure;
using Karambolo.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AspNetSkeleton.Service.Contract
{
    public interface IQuery { }

    public interface IQuery<out TResult> : IQuery { }

    public class ListResult<T>
    {
        public T[] Rows { get; set; }
        public int TotalRowCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class ListQuery<TList, T> : IQuery<TList>
        where TList : ListResult<T>
    {
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public string[] OrderColumns { get; set; }

        public bool IsPaged => PageIndex != null || PageSize != null;
        public bool IsOrdered => !ArrayUtils.IsNullOrEmpty(OrderColumns);
    }

    public class ListQuery<T> : ListQuery<ListResult<T>, T> { }

    public static class Query
    {
        static readonly Dictionary<string, Type> queryTypes = typeof(IQuery).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetClosedInterfaces(typeof(IQuery<>)).SingleOrDefault() != null)
            .ToDictionary(t => t.Name, Identity<Type>.Func);

        static readonly ConcurrentDictionary<Type, Type> interfaceTypes = new ConcurrentDictionary<Type, Type>();
        static readonly ConcurrentDictionary<Type, Type> actualTypes = new ConcurrentDictionary<Type, Type>();

        public static Type GetTypeBy(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return queryTypes.TryGetValue(typeName, out Type result) ? result : null;
        }

        public static Type GetInterfaceTypeFor(Type queryType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            return interfaceTypes.GetOrAdd(queryType, t => t.GetClosedInterfaces(typeof(IQuery<>)).SingleOrDefault());
        }

        public static Type GetActualTypeFor(Type queryType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            return actualTypes.GetOrAdd(queryType, t =>
            {
                if (t.GetClosedInterfaces(typeof(IQuery<>)).SingleOrDefault() == null)
                    throw new ArgumentException(nameof(queryType));

                var attribute = t.GetAttributes<HandledAsAttribute>().FirstOrDefault();
                Type actualType;
                if (attribute == null || (actualType = attribute.Type) == t)
                    return t;

                if (!t.IsSubclassOf(actualType))
                    throw new InvalidOperationException();

                return actualType;
            });
        }
    }
}
