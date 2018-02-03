using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using AspNetSkeleton.Base;
using DasMulli.Win32.ServiceUtils;

namespace AspNetSkeleton.Core.Hosting
{
    public interface IWindowsServiceManager
    {
        bool IsInstalled { get; }
        bool IsRunning { get; }

        void Install();
        void Uninstall();
        void Start();
        void Stop();
    }

    public class WindowsServiceManager : IWindowsServiceManager
    {
        readonly ServiceDefinition _serviceDefinition;
        readonly IAppEnvironment _environment;

        public WindowsServiceManager(ServiceDefinition serviceDefinition, IAppEnvironment environment)
        {
            _serviceDefinition = serviceDefinition;
            _environment = environment;
        }

        static void WaitForStatusChange(ServiceController serviceController, ServiceControllerStatus newStatus)
        {
            const int maxTryCount = 60;
            var count = 0;

            bool success;
            while (!(success = serviceController.Status == newStatus) && count < maxTryCount)
            {
                Thread.Sleep(1000);
                serviceController.Refresh();
                count++;
            }

            if (!success)
                throw new InvalidOperationException($"Windows service status cannot be changed. Current status: {serviceController.Status}.");
        }

        public void Install()
        {
            if (IsInstalled)
                throw new InvalidOperationException();

            new Win32ServiceManager().CreateService(_serviceDefinition);
        }

        public void Uninstall()
        {
            if (!IsInstalled)
                throw new InvalidOperationException();

            new Win32ServiceManager().DeleteService(_serviceDefinition.ServiceName);
        }

        public void Start()
        {
            if (!IsInstalled)
                throw new InvalidOperationException();

            using (var serviceController = new ServiceController(_serviceDefinition.ServiceName))
            {
                if (serviceController.Status != ServiceControllerStatus.Stopped)
                    throw new InvalidOperationException();

                serviceController.Start();
                WaitForStatusChange(serviceController, ServiceControllerStatus.Running);
            }
        }

        public void Stop()
        {
            if (!IsInstalled)
                throw new InvalidOperationException();

            using (var serviceController = new ServiceController(_serviceDefinition.ServiceName))
            {
                if (serviceController.Status != ServiceControllerStatus.Running)
                    throw new InvalidOperationException();

                serviceController.Stop();
                WaitForStatusChange(serviceController, ServiceControllerStatus.Stopped);
            }
        }

        public bool IsInstalled
        {
            get { return ServiceController.GetServices().Where(s => s.ServiceName == _serviceDefinition.ServiceName).Any(); }
        }

        public bool IsRunning
        {
            get
            {
                if (!IsInstalled)
                    return false;

                using (var serviceController = new ServiceController(_serviceDefinition.ServiceName))
                    return (serviceController.Status == ServiceControllerStatus.Running);
            }
        }
    }
}
