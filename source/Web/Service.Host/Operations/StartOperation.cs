using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Service.Host.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel;

namespace AspNetSkeleton.Service.Host.Operations
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class StartOperation : Operation
    {
        public const string Name = "start";
        public const string Hint = "Starts host windows service.";

        readonly IWindowsServiceManager _serviceManager;

        public StartOperation(string[] args, IOperationContext context, IWindowsServiceManager serviceManager) : base(args, context)
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

            if (_serviceManager.IsRunning)
                throw new OperationErrorException("Windows service is already running.");

            _serviceManager.Start();
            Context.Out.WriteLine("Windows service started successfully.");
        }
    }
}
