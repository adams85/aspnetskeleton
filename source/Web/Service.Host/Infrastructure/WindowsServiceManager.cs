using Karambolo.Common;
using System;
using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.ServiceProcess;
using System.Threading;

namespace AspNetSkeleton.Service.Host.Infrastructure
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

            using (var installer = new AssemblyInstaller(Assembly.GetExecutingAssembly(), ArrayUtils.Empty<string>()))
            {
                IDictionary state = new Hashtable();
                installer.UseNewContext = true;
                try
                {
                    installer.Install(state);
                    installer.Commit(state);
                }
                catch (Exception ex)
                {
                    try { installer.Rollback(state); }
                    catch { }

                    ExceptionDispatchInfo.Capture(ex).Throw();
                }
            }
        }

        public void Uninstall()
        {
            if (!IsInstalled)
                throw new InvalidOperationException();
            
            using (var installer = new AssemblyInstaller(Assembly.GetExecutingAssembly(), ArrayUtils.Empty<string>()))
            {
                IDictionary state = new Hashtable();
                installer.UseNewContext = true;

                installer.Uninstall(state);
            }
        }

        public void Start()
        {
            if (!IsInstalled)
                throw new InvalidOperationException();

            using (var serviceController = new ServiceController(WindowsService.Name))
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

            using (var serviceController = new ServiceController(WindowsService.Name))
            {
                if (serviceController.Status != ServiceControllerStatus.Running)
                    throw new InvalidOperationException();

                serviceController.Stop();
                WaitForStatusChange(serviceController, ServiceControllerStatus.Stopped);
            }
        }

        public bool IsInstalled
        {
            get { return ServiceController.GetServices().Where(s => s.ServiceName == WindowsService.Name).Any(); }
        }

        public bool IsRunning
        {
            get
            {
                if (!IsInstalled)
                    return false;

                using (ServiceController serviceController = new ServiceController(WindowsService.Name))
                    return (serviceController.Status == ServiceControllerStatus.Running);
            }
        }
    }
}
