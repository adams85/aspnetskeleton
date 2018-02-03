using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using System.Threading;
using Karambolo.Common;

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
            if (!PromptForConfirmation())
                throw new OperationErrorException("Command cancelled.");

            using (var dataContext = CreateDataContext())
            {
                var dbManager = CreateDbManager(dataContext);
                if (!dbManager.ExistsAsync(CancellationToken.None).WaitAndUnwrap())
                    throw new OperationErrorException("Database doesn't exist.");

                dbManager.DropAsync(CancellationToken.None).WaitAndUnwrap();
            }

            Context.Out.WriteLine("Database dropped.");
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
        }
    }
}
