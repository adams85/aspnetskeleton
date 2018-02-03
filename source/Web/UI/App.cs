using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Common.Utils;
using AspNetSkeleton.Core;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Karambolo.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspNetSkeleton.Core.Infrastructure;

namespace AspNetSkeleton.UI
{
    public class App : AppBase
    {
        readonly string _listenUrl;

        public App(IEnumerable<IAppConfiguration> configurations, TextWriter statusWriter, IComponentContext context)
            : base(configurations, statusWriter, context)
        {
            var settings = context.Resolve<IOptions<UISettings>>().Value;
            _listenUrl = settings.ListenUrl;

#if !DISTRIBUTED
            _backgroundProcesses = context.Resolve<IEnumerable<Service.Host.Core.Infrastructure.BackgroundWork.IBackgroundProcess>>();
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
        readonly IEnumerable<Service.Host.Core.Infrastructure.BackgroundWork.IBackgroundProcess> _backgroundProcesses;
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
