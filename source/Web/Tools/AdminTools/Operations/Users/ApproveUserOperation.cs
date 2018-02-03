using AspNetSkeleton.Service.Contract.Commands;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;

namespace AspNetSkeleton.AdminTools.Operations.Users
{
    [HandlerFor(Name)]
    public class ApproveUserOperation : ApiOperation
    {
        public const string Name = "approve-user";

        public ApproveUserOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 1;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <user-name>";
        }

        protected override void ExecuteCore()
        {
            var userName = MandatoryArgs[0];
            Command(new ApproveUserCommand
            {
                UserName = userName,
                Verify = false,
            });

            Context.Out.WriteLine($"User approved successfully.");
        }
    }
}
