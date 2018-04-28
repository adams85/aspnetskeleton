using AspNetSkeleton.DataAccess.Utils;
using Karambolo.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using AspNetSkeleton.Common.Utils;
using System.Runtime.ExceptionServices;
using LinqToDB.Metadata;

namespace AspNetSkeleton.DataAccess
{
    public interface IReadOnlyDbContext : IDisposable
    {
        object[] GetKey<TEntity>(TEntity entity) where TEntity : class;
        object GetRowVersion<TEntity>(TEntity entity) where TEntity : class;
        Task<TEntity> GetByKeyAsync<TEntity>(CancellationToken cancellationToken, params object[] keyValues) where TEntity : class;
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    }

    public interface IReadWriteDbContext : IReadOnlyDbContext
    {
        void Track<TEntity>(TEntity entity) where TEntity : class;
        bool IsDirty<TEntity>(TEntity entity) where TEntity : class;

        IdentityKey Create<TEntity>(TEntity entity) where TEntity : class;
        IdentityKey Create<TEntity>(Expression<Func<TEntity>> setter) where TEntity : class;

        void Update<TEntity>(TEntity entity) where TEntity : class;
        void Update<TEntity>(Expression<Func<TEntity>> setter, object rowVersion, params object[] keyValues) where TEntity : class;

        void Delete<TEntity>(TEntity entity) where TEntity : class;
        void Delete<TEntity>(object rowVersion, params object[] keyValues) where TEntity : class;
    }

    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }

    public interface IDbContext : IReadWriteDbContext, IUnitOfWork
    {
        IReadOnlyList<string> MigrationHistory { get; }
    }

    public class DbConcurrencyException : Exception
    {
        public DbConcurrencyException() : base("Concurrent modification detected.") { }
    }

    public abstract class DbContext : DataConnection, IDbContext
    {
        delegate Task<int> ChangeTaskFactory(DbContext context, CancellationToken cancellationToken);

        class MetadataReader : IMetadataReader
        {
            public T[] GetAttributes<T>(Type type, bool inherit = true) where T : Attribute
            {
                return ArrayUtils.Empty<T>();
            }

            public T[] GetAttributes<T>(Type type, MemberInfo memberInfo, bool inherit = true) where T : Attribute
            {
                ColumnAttribute[] columnAttributes;
                if (typeof(T) == typeof(ColumnAttribute) && memberInfo.Name == RowVersionColumnName &&
                    (columnAttributes = memberInfo.GetAttributes<ColumnAttribute>(inherit).ToArray()).Length > 0)
                {
                    Array.ForEach(columnAttributes, a =>
                    {
                        a.SkipOnInsert = true;
                        a.SkipOnUpdate = true;
                    });

                    return (T[])(object)columnAttributes;
                }

                return ArrayUtils.Empty<T>();
            }
        }

        protected class EntityMetadata
        {
            class DirtyColumnsEnumerable : IEnumerable<ColumnDescriptor>
            {
                readonly EntityMetadata _owner;
                readonly object _entity;
                readonly object _originalEntity;

                public DirtyColumnsEnumerable(EntityMetadata owner, object entity, object originalEntity)
                {
                    _owner = owner;
                    _entity = entity;
                    _originalEntity = originalEntity;
                }

                public IEnumerator<ColumnDescriptor> GetEnumerator()
                {
                    var i = -1;
                    while ((i = _owner._dirtyColumnsEnumerator(_entity, _originalEntity, i)) >= 0)
                        yield return _owner.UpdateColumns[i];
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            static readonly MethodInfo columnChangedMethodDefinition = Lambda.Method(() => ColumnChanged<object>(null, null)).GetGenericMethodDefinition();
            static readonly MethodInfo columnListAddMethod = Lambda.Method((List<ColumnDescriptor> l) => l.Add(null));

            static bool ColumnChanged<T>(T currentValue, T originalValue)
            {
                return !EqualityComparer<T>.Default.Equals(currentValue, originalValue);
            }

            static Func<object, object, int, int> BuildDirtyColumnsEnumerator(Type entityType, ColumnDescriptor[] columns)
            {
                var entityParam = Expression.Parameter(typeof(object));
                var originalEntityParam = Expression.Parameter(typeof(object));
                var indexParam = Expression.Parameter(typeof(int));

                var entityVar = Expression.Variable(entityType);
                var originalEntityVar = Expression.Variable(entityType);

                var blockStatements = new List<Expression>();

                blockStatements.Add(Expression.Assign(entityVar, Expression.Convert(entityParam, entityType)));
                blockStatements.Add(Expression.Assign(originalEntityVar, Expression.Convert(originalEntityParam, entityType)));

                var loopStartLabel = Expression.Label();
                blockStatements.Add(Expression.Label(loopStartLabel));

                blockStatements.Add(Expression.Assign(indexParam, Expression.Increment(indexParam)));

                var columnCount = columns.Length;
                var switchCases = new SwitchCase[columnCount];
                var returnLabel = Expression.Label(typeof(int));

                for (var i = 0; i < columnCount; i++)
                {
                    var column = columns[i];

                    var columnChangedMethod = columnChangedMethodDefinition.MakeGenericMethod(column.MemberType);
                    var columnChanged = Expression.Call(columnChangedMethod,
                        Expression.MakeMemberAccess(entityVar, column.MemberInfo),
                        Expression.MakeMemberAccess(originalEntityVar, column.MemberInfo));

                    var check = Expression.IfThen(columnChanged, Expression.Return(returnLabel, indexParam));

                    switchCases[i] = Expression.SwitchCase(check, Expression.Constant(i));
                }

                var defaultResult = Expression.Constant(-1);
                blockStatements.Add(Expression.Switch(indexParam, Expression.Return(returnLabel, defaultResult), switchCases));

                blockStatements.Add(Expression.Goto(loopStartLabel));

                blockStatements.Add(Expression.Label(returnLabel, defaultResult));

                var body = Expression.Block(ArrayUtils.FromElements(entityVar, originalEntityVar), blockStatements);

                var lambda = Expression.Lambda<Func<object, object, int, int>>(body, entityParam, originalEntityParam, indexParam);
                return lambda.Compile();
            }

            readonly Func<object, object, int, int> _dirtyColumnsEnumerator;

            public EntityMetadata(EntityDescriptor descriptor)
            {
                var columns = descriptor.Columns;
                KeyColumns = columns.Where(cd => cd.IsPrimaryKey).OrderBy(cd => cd.PrimaryKeyOrder).ToArray();
                IdentityColumn = KeyColumns.Where(c => c.IsIdentity && IdentityKey.IsIdentityKeyType(c.MemberType)).SingleOrDefault();
                RowVersionColumn = columns.Where(cd => cd.MemberName == RowVersionColumnName).FirstOrDefault();
                InsertColumns = columns.Where(cd => !cd.SkipOnInsert).ToArray();
                UpdateColumns = columns.Where(cd => !cd.SkipOnUpdate).ToArray();

                _dirtyColumnsEnumerator = BuildDirtyColumnsEnumerator(descriptor.ObjectType, UpdateColumns);
            }

            public ColumnDescriptor[] KeyColumns { get; }
            public ColumnDescriptor IdentityColumn { get; }
            public ColumnDescriptor RowVersionColumn { get; }
            public ColumnDescriptor[] InsertColumns { get; }
            public ColumnDescriptor[] UpdateColumns { get; }

            public IEnumerable<ColumnDescriptor> GetDirtyColumns(object entity, object originalEntity)
            {
                return new DirtyColumnsEnumerable(this, entity, originalEntity);
            }
        }

        const string RowVersionColumnName = "RowVersion";

        static readonly Func<object, object> entityCloner;
        static readonly MappingSchema mappingSchema;

        static DbContext()
        {
            entityCloner = BuildEntityCloner();
            mappingSchema = new MappingSchema(IdentityKey.MappingSchema);
            mappingSchema.AddMetadataReader(new MetadataReader());
        }

        static Func<object, object> BuildEntityCloner()
        {
            var cloneMethod = typeof(object).GetMethod(nameof(DbContext.MemberwiseClone), BindingFlags.Instance | BindingFlags.NonPublic);
            var param = Expression.Parameter(typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(Expression.Call(param, cloneMethod), param);
            return lambda.Compile();
        }

        readonly List<ChangeTaskFactory> _changes = new List<ChangeTaskFactory>();
        readonly Dictionary<object, object> _trackedEntities = new Dictionary<object, object>();

        protected DbContext(string configurationString)
            : base(configurationString, mappingSchema)
        {
            ProviderName = DbConfiguration.GetProviderName(ConfigurationString);
        }

        public string ProviderName { get; }

        public abstract IReadOnlyList<string> MigrationHistory { get; }

        protected abstract EntityMetadata GetEntityMetadata<TEntity>() where TEntity : class;

        Expression<Func<TEntity, bool>> BuildEntityFilterPredicate<TEntity>(object rowVersion, object[] keyValues) where TEntity : class
        {
            var metadata = GetEntityMetadata<TEntity>();

            var keyCount = metadata.KeyColumns.Length;
            if (keyCount == 0)
                throw new InvalidOperationException($"No primary key is defined for type {typeof(TEntity).FullName}.");

            if (keyValues.Length != keyCount)
                throw new ArgumentException($"Key consists of {keyCount} components.", nameof(keyValues));

            var param = Expression.Parameter(typeof(TEntity));

            var predicate = BuildClause(param, metadata.KeyColumns[0], keyValues[0]);
            for (var i = 1; i < keyCount; i++)
                predicate = Expression.AndAlso(predicate, BuildClause(param, metadata.KeyColumns[i], keyValues[i]));

            if (rowVersion != null)
            {
                if (metadata.RowVersionColumn == null)
                    throw new InvalidOperationException($"No row version column is defined for type {typeof(TEntity).FullName}.");

                predicate = Expression.AndAlso(predicate, BuildClause(param, metadata.RowVersionColumn, rowVersion));
            }

            return Expression.Lambda<Func<TEntity, bool>>(predicate, param);

            Expression BuildClause(ParameterExpression p, ColumnDescriptor kc, object kv)
            {
                var member = kc.MemberInfo.Get(pi => Expression.Property(p, pi), fi => Expression.Field(p, fi));

                if (kv != null &&
                    IdentityKey.IsIdentityKeyType(kc.MemberType) &&
                    !kv.GetType().IsSubclassOf(kc.MemberType))
                    kv = IdentityKey.From(kv);

                var constant = Expression.Constant(kv, kc.MemberType);
                return Expression.Equal(member, constant);
            }
        }

        static Expression<Func<TEntity>> BuildSetter<TEntity>(ColumnDescriptor[] columns, TEntity entity) where TEntity : class
        {
            var columnCount = columns.Length;

            var bindings = new MemberBinding[columnCount];

            for (var i = 0; i < columnCount; i++)
            {
                var member = columns[i].MemberInfo;
                var constant = member.Get(
                    pi => Expression.Constant(pi.GetValue(entity), pi.PropertyType),
                    fi => Expression.Constant(fi.GetValue(entity), fi.FieldType));

                bindings[i] = Expression.Bind(member, constant);
            }

            var setter = Expression.MemberInit(Expression.New(typeof(TEntity)), bindings);
            return Expression.Lambda<Func<TEntity>>(setter);
        }

        public object[] GetKey<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var keyColumns = GetEntityMetadata<TEntity>().KeyColumns;
            var keyCount = keyColumns.Length;

            var result = new object[keyCount];
            for (var i = 0; i < keyCount; i++)
                result[i] = keyColumns[i].MemberInfo.Get(pi => pi.GetValue(entity), fi => fi.GetValue(entity));

            return result;
        }

        public object GetRowVersion<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var rowVersionColumn = GetEntityMetadata<TEntity>().RowVersionColumn;
            if (rowVersionColumn == null)
                return null;

            return rowVersionColumn.MemberInfo.Get(pi => pi.GetValue(entity), fi => fi.GetValue(entity));
        }

        public Task<TEntity> GetByKeyAsync<TEntity>(CancellationToken cancellationToken, params object[] keyValues) where TEntity : class
        {
            if (keyValues == null)
                throw new ArgumentNullException(nameof(keyValues));

            var filterPredicate = BuildEntityFilterPredicate<TEntity>(null, keyValues);
            return GetTable<TEntity>().FirstOrDefaultAsync(filterPredicate, cancellationToken);
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        {
            return new LinqToDBQueryableDecorator<TEntity>(GetTable<TEntity>());
        }

        public void Track<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (_trackedEntities.ContainsKey(entity))
                throw new InvalidOperationException("Entity is already tracked.");

            _trackedEntities.Add(entity, entityCloner(entity));
        }

        public bool IsDirty<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!_trackedEntities.TryGetValue(entity, out object originalEntity))
                throw new InvalidOperationException("Entity is not tracked.");

            return GetEntityMetadata<TEntity>().GetDirtyColumns(entity, originalEntity).Any();
        }

        public IdentityKey Create<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var metadata = GetEntityMetadata<TEntity>();

            var columns = metadata.InsertColumns;
            if (columns.Length == 0 && metadata.IdentityColumn == null)
                return null;

            var setter = BuildSetter(columns, entity);

            return Create(setter);
        }

        public IdentityKey Create<TEntity>(Expression<Func<TEntity>> setter) where TEntity : class
        {
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));

            var identityColumn = GetEntityMetadata<TEntity>().IdentityColumn;

            if (identityColumn != null)
            {
                var result = IdentityKey.CreatePromise(identityColumn.MemberType.GetGenericArguments()[0], out IIdentityKeyValueSetter valueSetter);
                _changes.Add(async (ctx, ct) =>
                {
                    valueSetter.SetValue(await ctx.GetTable<TEntity>().InsertWithIdentityAsync(setter, ct).ConfigureAwait(false));
                    return 1;
                });
                return result;
            }
            else
            {
                _changes.Add((ctx, ct) => ctx.GetTable<TEntity>().InsertAsync(setter, ct));
                return null;
            }
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var metadata = GetEntityMetadata<TEntity>();
            var columns = metadata.UpdateColumns;

            if (_trackedEntities.TryGetValue(entity, out object originalEntity))
                columns = columns.Intersect(metadata.GetDirtyColumns(entity, originalEntity)).ToArray();

            if (columns.Length == 0)
                return;

            var setter = BuildSetter(columns, entity);
            var rowVersion = GetRowVersion(entity);
            var keyValues = GetKey(entity);

            Update(setter, rowVersion, keyValues);
        }

        public void Update<TEntity>(Expression<Func<TEntity>> setter, object rowVersion, params object[] keyValues) where TEntity : class
        {
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));

            if (keyValues == null)
                throw new ArgumentNullException(nameof(keyValues));

            var filterPredicate = BuildEntityFilterPredicate<TEntity>(rowVersion, keyValues);

            var setterAdapted = Expression.Lambda<Func<TEntity, TEntity>>(setter.Body, Expression.Parameter(typeof(TEntity)));

            ChangeTaskFactory changeTaskFactory =
                (ctx, ct) => ctx.GetTable<TEntity>().Where(filterPredicate).UpdateAsync(ctx.GetTable<TEntity>(), setterAdapted, ct);

            if (rowVersion != null)
                _changes.Add(async (ctx, ct) =>
                {
                    var affectedRowCount = await changeTaskFactory(ctx, ct).ConfigureAwait(false);
                    if (affectedRowCount != 1)
                        throw new DbConcurrencyException();
                    return affectedRowCount;
                });
            else
                _changes.Add(changeTaskFactory);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var rowVersion = GetRowVersion(entity);
            var keyValues = GetKey(entity);

            Delete<TEntity>(rowVersion, keyValues);
        }

        public void Delete<TEntity>(object rowVersion, params object[] keyValues) where TEntity : class
        {
            if (keyValues == null)
                throw new ArgumentNullException(nameof(keyValues));

            var filterPredicate = BuildEntityFilterPredicate<TEntity>(rowVersion, keyValues);

            ChangeTaskFactory changeTaskFactory =
                (ctx, ct) => ctx.GetTable<TEntity>().Where(filterPredicate).DeleteAsync(ct);

            if (rowVersion != null)
                _changes.Add(async (ctx, ct) =>
                {
                    var affectedRowCount = await changeTaskFactory(ctx, ct).ConfigureAwait(false);
                    if (affectedRowCount != 1)
                        throw new DbConcurrencyException();
                    return affectedRowCount;
                });
            else
                _changes.Add(changeTaskFactory);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var changeCount = _changes.Count;
            if (changeCount > 0)
            {
                using (var transaction = BeginTransaction())
                    try
                    {
                        for (var i = 0; i < changeCount; i++)
                            await _changes[i](this, cancellationToken).ConfigureAwait(false);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }

                _changes.Clear();
            }

            _trackedEntities.Clear();

            return changeCount;
        }
    }

    public abstract class DbContext<TContext> : DbContext
        where TContext : DbContext<TContext>
    {
        static readonly ConcurrentDictionary<Type, EntityMetadata> entityMetadataCache = new ConcurrentDictionary<Type, EntityMetadata>();

        protected DbContext(IDbConfigurationProvider configurationProvider)
            : base(configurationProvider.ProvideFor<TContext>()) { }

        protected override EntityMetadata GetEntityMetadata<TEntity>()
        {
            return entityMetadataCache.GetOrAdd(typeof(TEntity),
                t => new EntityMetadata(MappingSchema.GetEntityDescriptor(t)));
        }
    }
}