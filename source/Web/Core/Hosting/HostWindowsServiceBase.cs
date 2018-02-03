using System;
using System.Runtime.ExceptionServices;
using DasMulli.Win32.ServiceUtils;
using Karambolo.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSkeleton.Core.Hosting
{
    public interface IHostWindowsService : IWin32Service
    {
        ServiceDefinition Definition { get; }
    }

    public abstract class HostWindowsServiceBase : IHostWindowsService
    {
        readonly IHost _host;
        IAppScope _appScope;

        public ILogger Logger { get; set; }

        public string ServiceName => Definition.ServiceName;

        public abstract ServiceDefinition Definition { get; }

        public HostWindowsServiceBase(IHost host)
        {
            Logger = NullLogger.Instance;

            _host = host;
        }

        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            try
            {
                _appScope = _host.CreateAppScope();

                _appScope.App.StartUpAsync().WaitAndUnwrap();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to start windows service.");
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            Logger.LogInformation("Windows service started.");
        }

        public void Stop()
        {
            try
            {
                _appScope.App.ShutDownAsync().WaitAndUnwrap();

                _appScope.Dispose();
                _appScope = null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to stop windows service.");
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            Logger.LogInformation("Windows service stopped.");
        }
    }
}
