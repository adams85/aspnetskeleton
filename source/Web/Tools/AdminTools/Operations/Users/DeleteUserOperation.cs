using AspNetSkeleton.Service.Contract.Commands;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;

namespace AspNetSkeleton.AdminTools.Operations.Users
{
    [HandlerFor(Name)]
    public class DeleteUserOperation : ApiOperation
    {
        public const string Name = "delete-user";

        public DeleteUserOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 1;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <user-name>";
        }        

        protected override void ExecuteCore()
        {
            var userName = MandatoryArgs[0];

            if (!PromptForConfirmation())
                throw new OperationErrorException("Command cancelled.");

            Command(new DeleteUserCommand
            {
                UserName = userName,
            });

            Context.Out.WriteLine($"User deleted successfully.");
        }
    }
}
