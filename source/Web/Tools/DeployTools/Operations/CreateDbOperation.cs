using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using System.Threading;
using Karambolo.Common;

namespace AspNetSkeleton.DeployTools.Operations
{
    [HandlerFor(Name)]
    class CreateDbOperation : DbOperation
    {
        public const string Name = "create-db";

        public CreateDbOperation(string[] args, IOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 0;

        protected override void ExecuteCore()
        {
            string migration;
            using (var dataContext = CreateDataContext())
            {
                var dbManager = CreateDbManager(dataContext);
                if (dbManager.ExistsAsync(CancellationToken.None).WaitAndUnwrap())
                    throw new OperationErrorException("Database already exists.");

                dbManager.CreateAsync(CancellationToken.None).WaitAndUnwrap();

                var migrations = dataContext.MigrationHistory;
                var migrationCount = migrations.Count;
                if (migrationCount > 0)
                {
                    migration = migrations[migrationCount - 1];
                    dbManager.MigrateAsync(migration, CreateDbMigrationProvider(dataContext), CancellationToken.None).WaitAndUnwrap();
                }
                else
                    migration = null;
            }

            Context.Out.WriteLine("Database created.");
            Context.Out.WriteLine($"Current migration is {(migration != null ? migration : "(none)")}.");
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
        }
    }
}
