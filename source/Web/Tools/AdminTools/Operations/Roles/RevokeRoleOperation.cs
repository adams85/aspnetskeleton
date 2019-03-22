using AspNetSkeleton.Service.Contract.Commands;
using Karambolo.Common;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;

namespace AspNetSkeleton.AdminTools.Operations.Roles
{
    [HandlerFor(Name)]
    public class RevokeRoleOperation : ApiOperation
    {
        public const string Name = "revoke-role";

        public RevokeRoleOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 2;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <role-name> <user-name>";
        }        

        protected override void ExecuteCore()
        {
            var roleName = MandatoryArgs[0];
            var userName = MandatoryArgs[1];

            Command(new RemoveUsersFromRolesCommand
            {
                UserNames = new[] { userName },
                RoleNames = new[] { roleName },
            });

            Context.Out.WriteLine($"Role revoked successfully.");
        }
    }
}
