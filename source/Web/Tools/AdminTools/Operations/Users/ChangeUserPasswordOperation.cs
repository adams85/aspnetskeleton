using AspNetSkeleton.Service.Contract.Commands;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;

namespace AspNetSkeleton.AdminTools.Operations.Users
{
    [HandlerFor(Name)]
    public class ChangeUserPasswordOperation : ApiOperation
    {
        public const string Name = "change-pwd";

        public ChangeUserPasswordOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 2;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <user-name> <new-password>";
        }

        protected override void ExecuteCore()
        {
            var userName = MandatoryArgs[0];
            var newPassword = MandatoryArgs[1];

            Command(new ChangePasswordCommand
            {
                UserName = userName,
                NewPassword = newPassword,
                Verify = false,
            });

            Context.Out.WriteLine($"User password changed successfully.");
        }
    }
}
