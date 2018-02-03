using System.Data.Entity;
using System.Data.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Karambolo.Common;

namespace AspNetSkeleton.DataAccess
{
    public interface IReadOnlyDbContext : IDisposable
    {
        object[] GetKey(object entity);
        TEntity GetByKey<TEntity>(params object[] keyValues) where TEntity : class;
        Task<TEntity> GetByKeyAsync<TEntity>(CancellationToken cancellationToken, params object[] keyValues) where TEntity : class;
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    }

    public interface IReadWriteDbContext : IReadOnlyDbContext
    {
        TEntity GetByKeyTracking<TEntity>(params object[] keyValues) where TEntity : class;
        Task<TEntity> GetByKeyTrackingAsync<TEntity>(CancellationToken cancellationToken, params object[] keyValues) where TEntity : class;
        IQueryable<TEntity> QueryTracking<TEntity>() where TEntity : class;

        void Create<TEntity>(TEntity entity) where TEntity : class;
        void Update<TEntity>(TEntity entity) where TEntity : class;
        void Delete<TEntity>(TEntity entity) where TEntity : class;
    }

    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }

    public interface IDbContext : IReadWriteDbContext, IUnitOfWork { }

    public abstract class DbContextBase : DbContext, IDbContext
    {
        protected DbContextBase() { }

        protected DbContextBase(DbConnection connection, bool ownsConnection)
            : base(connection, ownsConnection) { }

        protected DbContextBase(string nameOrConnectionString)
            : base(nameOrConnectionString) { }

        protected abstract PropertyInfo[] GetKeyProperties(Type type);

        Expression<Func<TEntity, bool>> BuildGetByIdWhere<TEntity>(params object[] keys) where TEntity : class
        {
            var keyProperties = GetKeyProperties(typeof(TEntity));

            if (keys.Length != keyProperties.Length)
                throw new ArgumentException(null, nameof(keys));

            var builder = PredicateBuilder<TEntity>.True();

            var count = keyProperties.Length;
            Expression body = Expression.Constant(true);
            for (var i = 0; i < count; i++)
            {
                var property = keyProperties[i];
                var propertyAccess = Expression.Property(builder.Param, property);
                var equalityCheck = Expression.Equal(propertyAccess, Expression.Constant(keys[i], property.PropertyType));
                builder.And(equalityCheck);
            }

            return builder.Build();
        }

        public object[] GetKey(object entity)
        {
            var keyProperties = GetKeyProperties(entity.GetType());

            var count = keyProperties.Length;
            var result = new object[count];
            for (var i = 0; i < count; i++)
                result[i] = keyProperties[i].GetValue(entity, null);

            return result;
        }

        public TEntity GetByKey<TEntity>(params object[] keyValues) where TEntity : class
        {
            return Query<TEntity>().Where(BuildGetByIdWhere<TEntity>(keyValues)).FirstOrDefault();
        }

        public Task<TEntity> GetByKeyAsync<TEntity>(CancellationToken cancellationToken, params object[] keyValues) where TEntity : class
        {
            return Query<TEntity>().Where(BuildGetByIdWhere<TEntity>(keyValues)).FirstOrDefaultAsync(cancellationToken);
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        {
            return Set<TEntity>().AsNoTracking();
        }

        public TEntity GetByKeyTracking<TEntity>(params object[] keyValues) where TEntity : class
        {
            return Set<TEntity>().Find(keyValues);
        }

        public Task<TEntity> GetByKeyTrackingAsync<TEntity>(CancellationToken cancellationToken, params object[] keyValues) where TEntity : class
        {
            return Set<TEntity>().FindAsync(cancellationToken, keyValues);
        }

        public IQueryable<TEntity> QueryTracking<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }

        bool IsAttached<TEntity>(TEntity entity) where TEntity : class
        {
            return Entry(entity).State != EntityState.Detached;
        }

        public void Create<TEntity>(TEntity entity) where TEntity : class
        {
            if (IsAttached(entity))
                throw new InvalidOperationException();

            Set<TEntity>().Add(entity);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            if (!IsAttached(entity))
                throw new InvalidOperationException();
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            if (!IsAttached(entity))
                throw new InvalidOperationException();

            Set<TEntity>().Remove(entity);
        }
    }

    public abstract class DbContextBase<TContext> : DbContextBase
        where TContext : DbContextBase<TContext>
    {
        static readonly ConcurrentDictionary<Type, PropertyInfo[]> keyPropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        protected DbContextBase() { }

        protected DbContextBase(DbConnection connection, bool ownsConnection)
            : base(connection, ownsConnection) { }

        protected DbContextBase(string nameOrConnectionString)
            : base(nameOrConnectionString) { }

        // https://romiller.com/2014/10/07/ef6-1-getting-key-properties-for-an-entity/
        IEnumerable<EdmProperty> GetKeyEdmProperties(Type entityType)
        {
            var metadata = ((IObjectContextAdapter)this).ObjectContext.MetadataWorkspace;

            // Get the mapping between CLR types and metadata OSpace
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get metadata for given CLR type
            var entityMetadata = metadata
                .GetItems<EntityType>(DataSpace.OSpace)
                .First(e => objectItemCollection.GetClrType(e) == entityType);

            return entityMetadata.KeyProperties;
        }

        protected override PropertyInfo[] GetKeyProperties(Type type)
        {
            return keyPropertyCache.GetOrAdd(type,
                t => GetKeyEdmProperties(t).Select(p => t.GetProperty(p.Name)).ToArray());
        }
    }
}