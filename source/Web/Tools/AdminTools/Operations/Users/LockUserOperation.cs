using AspNetSkeleton.Service.Contract.Commands;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;

namespace AspNetSkeleton.AdminTools.Operations.Users
{
    [HandlerFor(Name)]
    public class LockUserOperation : ApiOperation
    {
        public const string Name = "lock-user";

        public LockUserOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 1;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <user-name>";
        }        

        protected override void ExecuteCore()
        {
            var userName = MandatoryArgs[0];
            Command(new LockUserCommand
            {
                UserName = userName,                
            });

            Context.Out.WriteLine($"User locked successfully.");
        }
    }
}
