using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.DataAccess;

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
            if (!OptionalArgs.TryGetValue("m", out string migrationName))
                migrationName = null;

            using (var conn = CreateConnection())
                if (!Database.Exists(conn))
                    throw new OperationErrorException("Database doesn't exist.");
            
            Database.SetInitializer<DataContext>(null);

            var migrationConfiguration = new Migrations.Configuration(CreateConnectionInfo());
            var migrator = new DbMigrator(migrationConfiguration);

            if (string.IsNullOrEmpty(migrationName))
                migrator.Update();
            else
                migrator.Update(migrationName);

            Context.Out.WriteLine("Database updated.");
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} [/m=<migration-name>]";
        }
    }
}
