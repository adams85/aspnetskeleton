using System.Collections.Generic;
using System.IO;
using AspNetSkeleton.Core;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.UI
{
#if !DISTRIBUTED
    using System.Linq;
    using System.Threading.Tasks;
    using Service.Host.Core.Infrastructure.BackgroundWork;
#endif

    public class App : AppBase
    {
        readonly string _listenUrl;

        public App(IEnumerable<IAppConfiguration> configurations, TextWriter statusWriter, IComponentContext context)
            : base(configurations, statusWriter, context)
        {
            var settings = context.Resolve<IOptions<UISettings>>().Value;
            _listenUrl = settings.ListenUrl;

#if !DISTRIBUTED
            _backgroundProcesses = context.Resolve<IEnumerable<IBackgroundProcess>>();
#endif
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(_listenUrl);
        }

#if !DISTRIBUTED
        readonly IEnumerable<IBackgroundProcess> _backgroundProcesses;
        Task[] _backgroundTasks;

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
#endif
    }
}
