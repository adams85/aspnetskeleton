using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using Karambolo.Common;
using System.Collections.Generic;
using System.ComponentModel;

namespace AspNetSkeleton.Core.Hosting.Operations
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class ConsoleOperation : Operation
    {
        public const string Name = "console";
        public const string Hint = "Runs app as a console application.";

        readonly IHost _host;

        public ConsoleOperation(string[] args, IOperationContext context, IHost host) : base(args, context)
        {
            _host = host;
        }

        protected override int MandatoryArgCount => 0;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
            yield return Hint;
        }

        public override void Execute()
        {
            using (var appScope = _host.CreateAppScope())
            {
                appScope.App.StartUpAsync().WaitAndUnwrap();

                Context.Out.WriteLine();

                Context.Out.WriteLine("Press ENTER to terminate.");
                Context.In.ReadLine();

                appScope.App.ShutDownAsync().WaitAndUnwrap();
            }
        }
    }
}
