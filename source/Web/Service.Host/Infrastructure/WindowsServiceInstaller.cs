using AspNetSkeleton.Core.Infrastructure;
using Autofac;
using Karambolo.Common.Logging;
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Collections;

namespace AspNetSkeleton.Service.Host.Infrastructure
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        readonly ILogger _logger;

        public WindowsServiceInstaller()
        {
            _logger =
                CoreModule.RootLifetimeScope.TryResolve(out Func<string, ILogger> loggerFactory) ?
                loggerFactory(Program.AssemblyName) :
                NullLogger.Instance;

            var processInstaller = new ServiceProcessInstaller()
            {
                Account = ServiceAccount.LocalSystem
            };
            
            var serviceInstaller = new ServiceInstaller() 
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = WindowsService.Name,
                DisplayName = WindowsService.Name,
                Description = WindowsService.Description,
            };

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }

        public override void Install(IDictionary stateSaver)
        {
            Context.Parameters["assemblyPath"] = $"\"{Context.Parameters["assemblyPath"]}\" service";

            base.Install(stateSaver);

            _logger.LogInfo("Windows service installed.");
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            _logger.LogInfo("Windows service uninstalled.");
        }
    }
}
