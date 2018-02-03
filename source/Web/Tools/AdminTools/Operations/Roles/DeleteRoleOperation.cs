using AspNetSkeleton.Service.Contract.Commands;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;

namespace AspNetSkeleton.AdminTools.Operations.Roles
{
    [HandlerFor(Name)]
    public class DeleteRoleOperation : ApiOperation
    {
        public const string Name = "delete-role";

        public DeleteRoleOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 1;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <role-name>";
        }        

        protected override void ExecuteCore()
        {
            var roleName = MandatoryArgs[0];

            if (!PromptForConfirmation())
                throw new OperationErrorException("Command cancelled.");

            Command(new DeleteRoleCommand
            {
                RoleName = roleName,
            });

            Context.Out.WriteLine($"Role deleted successfully.");
        }
    }
}
