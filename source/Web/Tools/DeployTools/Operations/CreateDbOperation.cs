using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.DataAccess;

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
            using (var conn = CreateConnection())
                if (Database.Exists(conn))
                    throw new OperationErrorException("Database already exists.");

            Database.SetInitializer<DataContext>(null);

            var migrationConfiguration = new Migrations.Configuration(CreateConnectionInfo());
            var migrator = new DbMigrator(migrationConfiguration);

            migrator.Update();

            Context.Out.WriteLine("Database created.");
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
        }
    }
}
