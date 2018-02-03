using System.Collections.Generic;
using System.Data.Entity;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;

namespace AspNetSkeleton.DeployTools.Operations
{
    [HandlerFor(Name)]
    class DropDbOperation : DbOperation
    {
        public const string Name = "drop-db";

        public DropDbOperation(string[] args, IOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 0;

        protected override void ExecuteCore()
        {
            using (var conn = CreateConnection())
            {
                if (!Database.Exists(conn))
                    throw new OperationErrorException("Database doesn't exist.");

                if (!PromptForConfirmation())
                    throw new OperationErrorException("Command cancelled.");

                Database.Delete(conn);

                Context.Out.WriteLine("Database dropped.");
            }
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
        }
    }
}
