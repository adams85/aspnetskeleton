using AspNetSkeleton.Service.Contract.Commands;
using Karambolo.Common;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;

namespace AspNetSkeleton.AdminTools.Operations.Roles
{
    [HandlerFor(Name)]
    public class GrantRoleOperation : ApiOperation
    {
        public const string Name = "grant-role";

        public GrantRoleOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 2;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <role-name> <user-name>";
        }        

        protected override void ExecuteCore()
        {
            var roleName = MandatoryArgs[0];
            var userName = MandatoryArgs[1];

            Command(new AddUsersToRolesCommand
            {
                UserNames = ArrayUtils.FromElement(userName),
                RoleNames = ArrayUtils.FromElement(roleName),
            });

            Context.Out.WriteLine($"Role granted successfully.");
        }
    }
}
