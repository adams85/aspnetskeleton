using Karambolo.Common;
using Karambolo.Common.Logging;
using System;
using System.Runtime.ExceptionServices;
using System.ServiceProcess;

namespace AspNetSkeleton.Service.Host
{
    class WindowsService : ServiceBase
    {
        public const string Name = "AspNetSkeleton.Service";
        public const string Description = "WebApi service for AspNetSkeleton application.";

        IServiceHost _serviceHost;

        public ILogger Logger { get; set; }

        public WindowsService(IServiceHost serviceHost)
        {
            Logger = NullLogger.Instance;

            _serviceHost = serviceHost;

            ServiceName = Name;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            try
            {
                _serviceHost.StartUpAsync().WaitAndUnwrap();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to start windows service. Details: {0}", ex);
                throw;
            }

            Logger.LogInfo("Windows service started.");
        }

        protected override void OnStop()
        {
            base.OnStop();

            try
            {
                _serviceHost.ShutDownAsync().WaitAndUnwrap();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to stop windows service. Details: {0}", ex);
                throw;
            }

            Logger.LogInfo("Windows service stopped.");
        }
    }
}
