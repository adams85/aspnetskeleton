using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using System.Collections.Generic;
using System.ComponentModel;
using AspNetSkeleton.Core.Infrastructure;
using System;
using DasMulli.Win32.ServiceUtils;

namespace AspNetSkeleton.Core.Hosting.Operations
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class StopOperation : Operation
    {
        public const string Name = "stop";
        public const string Hint = "Stops app windows service.";

        readonly IWindowsServiceManager _serviceManager;

        public StopOperation(string[] args, IOperationContext context, IHostWindowsService service,
            Func<ServiceDefinition, IWindowsServiceManager> serviceManagerFactory) : base(args, context)
        {
            _serviceManager = serviceManagerFactory(service.Definition);
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

            if (!_serviceManager.IsRunning)
                throw new OperationErrorException("Windows service is not running.");

            _serviceManager.Stop();
            Context.Out.WriteLine("Windows service stopped successfully.");
        }
    }
}
