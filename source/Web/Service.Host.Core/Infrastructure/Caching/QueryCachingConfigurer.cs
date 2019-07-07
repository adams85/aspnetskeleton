using AspNetSkeleton.Service.Contract;
using Autofac;
using System;
using System.Collections.Generic;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.Caching
{
    public interface IQueryCachingConfigurer
    {
        IQueryCachingConfigurer<TQuery> Cache<TQuery>()
            where TQuery : IQuery;

        IQueryCachingConfigurer<TQuery> Cache<TQuery, TInterceptor>()
            where TInterceptor : QueryCacherInterceptor
            where TQuery : IQuery;

        void Configure(ContainerBuilder builder);
    }

    public interface IQueryCachingConfigurer<TQuery> where TQuery : IQuery
    {
        IQueryCachingConfigurer<TQuery> When(Func<TQuery, bool> filter);

        IQueryCachingConfigurer<TQuery> WithScope(Func<TQuery, string> scopeGetter);
        IQueryCachingConfigurer<TQuery> WithAbsoluteExpiration(TimeSpan value);
        IQueryCachingConfigurer<TQuery> WithSlidingExpiration(TimeSpan value);

        IQueryCachingConfigurer<TQuery> InvalidatedBy<TCommand>()
            where TCommand : ICommand;

        IQueryCachingConfigurer<TQuery> InvalidatedBy<TCommand, TInterceptor>()
            where TInterceptor : CachedQueryInvalidatorInterceptor
            where TCommand : ICommand;
    }

    public class QueryCachingConfigurer : IQueryCachingConfigurer
    {
        public abstract class QueryConfiguration : QueryCachingOptions
        {
            public Type QueryInterceptorType { get; protected set; }
            public Dictionary<Type, Type> Invalidators { get; } = new Dictionary<Type, Type>();
        }

        class QueryConfigurer<TQuery> : QueryConfiguration, IQueryCachingConfigurer<TQuery>
            where TQuery : IQuery
        {
            Func<TQuery, bool> _filter;
            List<Func<TQuery, string>> _scopeSelectors = new List<Func<TQuery, string>>();

            public IQueryCachingConfigurer<TQuery> When(Func<TQuery, bool> filter)
            {
                if (filter == null)
                    throw new ArgumentNullException(nameof(filter));

                _filter = filter;
                return this;
            }

            public IQueryCachingConfigurer<TQuery> WithScope(Func<TQuery, string> selector)
            {
                if (selector == null)
                    throw new ArgumentNullException(nameof(selector));

                _scopeSelectors.Add(selector);
                return this;
            }

            public IQueryCachingConfigurer<TQuery> WithAbsoluteExpiration(TimeSpan value)
            {
                AbsoluteExpiration = value;
                return this;
            }

            public IQueryCachingConfigurer<TQuery> WithSlidingExpiration(TimeSpan value)
            {
                SlidingExpiration = value;
                return this;
            }

            public IQueryCachingConfigurer<TQuery> InvalidatedBy<TCommand>()
                where TCommand : ICommand
            {
                return InvalidatedBy<TCommand, CachedQueryInvalidatorInterceptor>();
            }

            public IQueryCachingConfigurer<TQuery> InvalidatedBy<TCommand, TInterceptor>()
                where TCommand : ICommand
                where TInterceptor : CachedQueryInvalidatorInterceptor
            {
                Invalidators.Add(typeof(TCommand), typeof(TInterceptor));
                return this;
            }

            public override bool IsCached(QueryInterceptorContext context)
            {
                return _filter?.Invoke((TQuery)context.Query) ?? true;
            }

            public override IEnumerable<string> GetScopes(QueryInterceptorContext context)
            {
                yield return QueryCacherInterceptor.GetCacheScope(context.QueryType);

                var query = (TQuery)context.Query;
                int n = _scopeSelectors.Count;
                for (var i = 0; i < n; i++)
                        yield return QueryCacherInterceptor.GetCacheScope(context.QueryType, _scopeSelectors[i](query));
            }

            public QueryConfigurer(Type interceptorType)
            {
                QueryInterceptorType = interceptorType;
            }
        }

        readonly Dictionary<Type, QueryConfiguration> _configs = new Dictionary<Type, QueryConfiguration>();

        public IQueryCachingConfigurer<TQuery> Cache<TQuery>()
            where TQuery : IQuery
        {
            return Cache<TQuery, QueryCacherInterceptor>();
        }

        public IQueryCachingConfigurer<TQuery> Cache<TQuery, TInterceptor>()
            where TQuery : IQuery
            where TInterceptor : QueryCacherInterceptor
        {
            var config = new QueryConfigurer<TQuery>(typeof(TInterceptor));
            _configs.Add(typeof(TQuery), config);
            return config;
        }

        public void Configure(ContainerBuilder builder)
        {
            var invalidatorDescriptors = new Dictionary<KeyValuePair<Type, Type>, List<Type>>();

            foreach (var configKvp in _configs)
            {
                var queryType = configKvp.Key;
                var config = configKvp.Value;

                builder.RegisterType(config.QueryInterceptorType)
                    .As<IQueryInterceptor>()
                    .WithMetadata<QueryInterceptorMetadata>(cfg => cfg.For(metadata => metadata.LimitType, queryType))
                    .WithParameter(TypedParameter.From<QueryCachingOptions>(config));

                foreach (var invalidatorKvp in config.Invalidators)
                {
                    if (!invalidatorDescriptors.TryGetValue(invalidatorKvp, out List<Type> invalidatorDescriptor))
                        invalidatorDescriptors.Add(invalidatorKvp, invalidatorDescriptor = new List<Type>());

                    invalidatorDescriptor.Add(queryType);
                }
            }

            foreach (var invalidatorDescriptorKvp in invalidatorDescriptors)
            {
                var commandType = invalidatorDescriptorKvp.Key.Key;
                var commandInterceptorType = invalidatorDescriptorKvp.Key.Value;
                var queryTypes = invalidatorDescriptorKvp.Value;

                builder.RegisterType(commandInterceptorType)
                    .As<ICommandInterceptor>()
                    .WithMetadata<CommandInterceptorMetadata>(cfg => cfg.For(metadata => metadata.LimitType, commandType))
                    .WithParameter(TypedParameter.From(queryTypes.ToArray()));
            }
        }
    }
}
