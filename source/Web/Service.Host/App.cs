using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetSkeleton.Core;
using AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Service.Host
{
    public class App : AppBase
    {
        readonly string _listenUrl;
        readonly IEnumerable<IBackgroundProcess> _backgroundProcesses;

        Task[] _backgroundTasks;

        public App(IEnumerable<IAppConfiguration> configurations, TextWriter statusWriter, IComponentContext context)
            : base(configurations, statusWriter, context)
        { 
            var settings = context.Resolve<IOptions<ServiceHostSettings>>().Value;
            _listenUrl = settings.ListenUrl;

            _backgroundProcesses = context.Resolve<IEnumerable<IBackgroundProcess>>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(_listenUrl);
        }

        protected override Task StartUpCoreAsync()
        {
            _backgroundTasks = _backgroundProcesses
                 .Select(t => Task.Run(() => t.ExecuteAsync(ShutDownToken), ShutDownToken))
                 .ToArray();

            return base.StartUpCoreAsync();
        }

        protected override async Task ShutDownCoreAsync()
        {
            await Task.WhenAll(_backgroundTasks.Prepend(base.ShutDownCoreAsync()))
                .ConfigureAwait(false);

            _backgroundTasks = null;
        }
    }
}
