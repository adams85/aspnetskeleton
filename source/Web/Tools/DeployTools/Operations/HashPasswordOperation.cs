using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Base.Utils;

namespace AspNetSkeleton.DeployTools.Operations
{
    [HandlerFor(Name)]
    class HashPasswordOperation : Operation
    {
        public const string Name = "hash-pwd";

        public HashPasswordOperation(string[] args, IOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 1;

        public override void Execute()
        {
            var password = MandatoryArgs[0];

            Context.Out.WriteLine("Hash for password:");
            Context.Out.WriteLine(SecurityUtils.HashPassword(password));
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <password>";
        }
    }
}
