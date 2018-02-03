using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using System.Collections.Generic;
using System.ComponentModel;
using DasMulli.Win32.ServiceUtils;
using System.Runtime.InteropServices;
using Karambolo.Common;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Core.Hosting.Operations
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class ServiceOperation : Operation
    {
        public const string Name = "service";
        public const string Hint = "Runs app as a background service. (Do not call manually!)";

        readonly IHost _host;
        readonly IHostWindowsService _service;

        public ServiceOperation(string[] args, IOperationContext context, IHost host, IHostWindowsService service) : base(args, context)
        {
            _host = host;
            _service = service;
        }

        protected override int MandatoryArgCount => 0;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
            yield return Hint;
        }

        public override void Execute()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                new Win32ServiceHost(_service).Run();
            else
                using (var appScope = _host.CreateAppScope())
                {
                    var tcs = new TaskCompletionSource<object>();

                    // shutting down on SIGTERM signal
                    // https://stackoverflow.com/questions/38291567/killing-gracefully-a-net-core-daemon-running-on-linux
                    AssemblyLoadContext.Default.Unloading += ctx =>
                    {
                        appScope.App.ShutDownAsync().WaitAndUnwrap();
                        tcs.SetResult(null);
                    };

                    appScope.App.StartUpAsync().WaitAndUnwrap();
                    tcs.Task.WaitAndUnwrap();
                };
        }
    }
}
