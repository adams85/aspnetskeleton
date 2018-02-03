using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using System.Threading;
using Karambolo.Common;
using System;
using System.Linq;

namespace AspNetSkeleton.DeployTools.Operations
{
    [HandlerFor(Name)]
    class UpdateDbOperation : DbOperation
    {
        public const string Name = "update-db";

        public UpdateDbOperation(string[] args, IOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 0;

        protected override void ExecuteCore()
        {
            if (!OptionalArgs.TryGetValue("m", out string migration))
                migration = null;

            int result;
            using (var dataContext = CreateDataContext())
            {
                var dbManager = CreateDbManager(dataContext);
                if (!dbManager.ExistsAsync(CancellationToken.None).WaitAndUnwrap())
                    throw new OperationErrorException("Database doesn't exist.");

                if (migration == null)
                {
                    var migrations = dataContext.MigrationHistory;
                    var migrationCount = migrations.Count;
                    migration = migrations[migrationCount - 1];
                }
                else if (migration == string.Empty)
                    migration = null;

                var currentMigration = dbManager.GetCurrentMigrationAsync(CancellationToken.None).WaitAndUnwrap();
                if (currentMigration != null && !string.Equals(migration, currentMigration, StringComparison.OrdinalIgnoreCase))
                {
                    Context.Out.WriteLine($"Current migration is {currentMigration}.");
                    if (!PromptForConfirmation())
                        throw new OperationErrorException("Command cancelled.");
                }

                result = dbManager.MigrateAsync(migration, CreateDbMigrationProvider(dataContext), CancellationToken.None).WaitAndUnwrap();
            }

            if (result != 0)
                Context.Out.WriteLine($"Database updated by {(result > 0 ? "committing" : "reverting")} {Math.Abs(result)} migrations.");
            else
                Context.Out.WriteLine($"Database is already up-to-date.");

            Context.Out.WriteLine($"Current migration is {(migration != null ? migration : "(none)")}.");
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} [/m=<migration-name>]";
        }
    }
}
