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

namespace AspNetSkeleton.Api
{
    public class App : AppBase
    {
        readonly string _baseUrl;

        public App(IEnumerable<IAppConfiguration> configurations, TextWriter statusWriter, IComponentContext context)
            : base(configurations, statusWriter, context)
        {
            var settings = context.Resolve<IOptions<ApiSettings>>().Value;
            _baseUrl = settings.ApiBaseUrl;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(_baseUrl);
        }
    }
}
