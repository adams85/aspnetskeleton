using AspNetSkeleton.Service.Contract.Commands;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;

namespace AspNetSkeleton.AdminTools.Operations.Roles
{
    [HandlerFor(Name)]
    public class CreateRoleOperation : ApiOperation
    {
        public const string Name = "create-role";

        public CreateRoleOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 1;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <role-name>";
        }        

        protected override void ExecuteCore()
        {
            var roleName = MandatoryArgs[0];

            object key = null;
            Command(new CreateRoleCommand
            {
                RoleName = roleName,
                OnKeyGenerated = (c, k) => key = k,
            });

            Context.Out.WriteLine($"Role created successfully with id {key}.");
        }
    }
}
