using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using Karambolo.Common;
using System.Collections.Generic;
using System.ComponentModel;

namespace AspNetSkeleton.Service.Host.Operations
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class ConsoleOperation : Operation
    {
        public const string Name = "console";
        public const string Hint = "Runs host as console application.";

        readonly IServiceHost _webApp;

        public ConsoleOperation(string[] args, IOperationContext context, IServiceHost webApp) : base(args, context)
        {
            _webApp = webApp;
        }

        protected override int MandatoryArgCount => 0;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
            yield return Hint;
        }

        public override void Execute()
        {
            _webApp.StartUpAsync().WaitAndUnwrap();

            Context.Out.WriteLine($"Server listening at {_webApp.BaseUrl }...");
            Context.Out.WriteLine("Press ENTER to terminate.");
            Context.In.ReadLine();

            _webApp.ShutDownAsync().WaitAndUnwrap();
        }
    }
}
