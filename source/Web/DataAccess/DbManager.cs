using AspNetSkeleton.Common;
using AspNetSkeleton.Common.Utils;
using Karambolo.Common;
using Karambolo.Common.Collections;
using LinqToDB;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.DataAccess
{
    public interface IDbMigrationProvider
    {
        Task<string> GetCommitScriptAsync(DbContext context, string migration, CancellationToken cancellationToken);
        Task<string> GetRevertScriptAsync(DbContext context, string migration, CancellationToken cancellationToken);
    }

    public interface IDbManager
    {
        DbContext Context { get; }

        Task<bool> ExistsAsync(CancellationToken cancellationToken);
        Task CreateAsync(CancellationToken cancellationToken);
        Task DropAsync(CancellationToken cancellationToken);
        Task<string> GetCurrentMigrationAsync(CancellationToken cancellationToken);
        Task<int> MigrateAsync(string targetMigration, IDbMigrationProvider migrationProvider, CancellationToken cancellationToken);
    }

    public abstract class DbManager : IDbManager
    {
        protected const string migrationInfoTableName = "_MigrationInfo";

        [Table(migrationInfoTableName)]
        protected class MigrationInfo
        {
            [Column, PrimaryKey]
            public int Index { get; set; }
            [Column(Length = 64), NotNull]
            public string Name { get; set; }
            [Column(DataType = DataType.Text), NotNull]
            public string RevertScript { get; set; }
            [Column, NotNull]
            public DateTime AppliedAt { get; set; }
        }

        public DbManager(DbContext context, IClock clock)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
            Clock = clock;
        }

        protected IClock Clock { get; }
        public DbContext Context { get; }

        public abstract Task<bool> ExistsAsync(CancellationToken cancellationToken);

        public abstract Task CreateAsync(CancellationToken cancellationToken);

        public abstract Task DropAsync(CancellationToken cancellationToken);

        protected abstract Task<bool> HasMigrationInfoAsync(CancellationToken cancellationToken);

        protected abstract Task ExecuteScriptAsync(string content, CancellationToken cancellationToken);

        public async Task<string> GetCurrentMigrationAsync(CancellationToken cancellationToken)
        {
            if (await HasMigrationInfoAsync(cancellationToken).ConfigureAwait(false))
            {
                var migrationInfo = await Context.GetTable<MigrationInfo>().OrderByDescending(mi => mi.Index).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                return migrationInfo?.Name;
            }
            else
                return null;
        }

        public async Task<int> MigrateAsync(string targetMigration, IDbMigrationProvider migrationProvider, CancellationToken cancellationToken)
        {
            if (migrationProvider == null)
                throw new ArgumentNullException(nameof(migrationProvider));

            var contextHistory = Context.MigrationHistory.ToOrderedDictionary(Identity<string>.Func, Default<string, object>.Func, StringComparer.OrdinalIgnoreCase);

            var dbHistory = new OrderedDictionary<string, MigrationInfo>(StringComparer.OrdinalIgnoreCase);
            if (await HasMigrationInfoAsync(cancellationToken).ConfigureAwait(false))
                await Context.GetTable<MigrationInfo>().OrderBy(mi => mi.Index).ForEachAsync(mi => dbHistory.Add(mi.Name, mi), cancellationToken).ConfigureAwait(false);
            else
                await Context.CreateTableAsync<MigrationInfo>(token: cancellationToken).ConfigureAwait(false);

            var n = Math.Min(contextHistory.Count, dbHistory.Count);
            for (var i = 0; i < n; i++)
                if (!string.Equals(contextHistory.GetKeyAt(i), dbHistory.GetKeyAt(i), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Migration history is corrupt at index {i}.");

            var executers = new List<Func<CancellationToken, Task>>();

            var targetIndex = targetMigration != null ? dbHistory.IndexOfKey(targetMigration) : -1;

            var revert = targetMigration == null || targetIndex >= 0;
            if (revert)
            {
                if (targetIndex == dbHistory.Count - 1)
                    return 0;

                // revert
                for (var i = dbHistory.Count - 1; i > targetIndex; i--)
                {
                    var migration = dbHistory.GetKeyAt(i);
                    var migrationInfo = dbHistory[migration];
                    var revertScript = migrationInfo.RevertScript;

                    executers.Add(async ct =>
                    {
                        await ExecuteScriptAsync(revertScript, ct).ConfigureAwait(false);

                        await Context.DeleteAsync(migrationInfo, token: ct).ConfigureAwait(false);
                    });
                }
            }
            else
            {
                targetIndex = contextHistory.IndexOfKey(targetMigration);
                if (targetIndex < 0)
                    throw new ArgumentException("Unknown migration.", nameof(targetMigration));

                // commit
                for (var i = dbHistory.Count; i <= targetIndex; i++)
                {
                    var migration = contextHistory.GetKeyAt(i);

                    var getCommitScriptTask = migrationProvider.GetCommitScriptAsync(Context, migration, cancellationToken);
                    var getRevertScriptTask = migrationProvider.GetRevertScriptAsync(Context, migration, cancellationToken);

                    await Task.WhenAll(getCommitScriptTask, getRevertScriptTask).ConfigureAwait(false);

                    var commitScript = await getCommitScriptTask.ConfigureAwait(false);
                    var revertScript = await getRevertScriptTask.ConfigureAwait(false);

                    executers.Add(async ct =>
                    {
                        await ExecuteScriptAsync(commitScript, ct).ConfigureAwait(false);

                        var migrationInfo = new MigrationInfo
                        {
                            Index = i,
                            Name = migration,
                            AppliedAt = Clock.UtcNow,
                            RevertScript = revertScript,
                        };

                        await Context.InsertAsync(migrationInfo, token: ct).ConfigureAwait(false);
                    });
                }
            }

            n = executers.Count;
            for (var i = 0; i < n; i++)
                using (var transaction = Context.BeginTransaction())
                    try
                    {
                        await executers[i](cancellationToken).ConfigureAwait(false);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }

            return revert ? -n : n;
        }
    }
}
