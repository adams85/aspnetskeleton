using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using DasMulli.Win32.ServiceUtils;

namespace AspNetSkeleton.Core.Hosting.Operations
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class UninstallOperation : Operation
    {
        public const string Name = "uninstall";
        public const string Hint = "Uninstalls app as a windows service.";

        readonly IWindowsServiceManager _serviceManager;

        public UninstallOperation(string[] args, IOperationContext context, IHostWindowsService service,
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

            if (_serviceManager.IsRunning)
                _serviceManager.Stop();

            _serviceManager.Uninstall();
            Context.Out.WriteLine("Windows service uninstalled successfully.");
        }
    }
}
