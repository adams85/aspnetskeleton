using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Service.Host.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel;

namespace AspNetSkeleton.Service.Host.Operations
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class UninstallOperation : Operation
    {
        public const string Name = "uninstall";
        public const string Hint = "Uninstalls host as windows service.";

        readonly IWindowsServiceManager _serviceManager;

        public UninstallOperation(string[] args, IOperationContext context, IWindowsServiceManager serviceManager) : base(args, context)
        {
            _serviceManager = serviceManager;
        }

        protected override int MandatoryArgCount => 0;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
            yield return Hint;
        }

        public override void Execute()
        {
            if (!_serviceManager.IsInstalled)
                throw new OperationErrorException("Windows service is not installed.");

            _serviceManager.Uninstall();
            Context.Out.WriteLine("Windows service uninstalled successfully.");
        }
    }
}
