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
    public class InstallOperation : Operation
    {
        public const string Name = "install";
        public const string Hint = "Installs app as a windows service.";

        readonly IWindowsServiceManager _serviceManager;

        public InstallOperation(string[] args, IOperationContext context, IHostWindowsService service,
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
            if (_serviceManager.IsInstalled)
                throw new OperationErrorException("Windows service is already installed.");

            _serviceManager.Install();
            Context.Out.WriteLine("Windows service installed successfully.");
        }
    }
}
