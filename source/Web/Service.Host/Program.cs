using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Common.Utils;
using AspNetSkeleton.Service.Host.Filters;
using AspNetSkeleton.Service.Host.Operations;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Karambolo.Common;
using AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork;
using AspNetSkeleton.Service.Host.Infrastructure;

namespace AspNetSkeleton.Service.Host
{
    public interface IServiceHost : IShutDownTokenAccessor
    {
        Task StartUpAsync();
        Task ShutDownAsync();
        string BaseUrl { get; }
    }

    class Program : OperationHost, IServiceHost
    {
        enum Status
        {
            Stopped,
            Starting,
            Started,
            Stopping
        }

        public static readonly string AssemblyName = typeof(Program).Assembly.GetName().Name;
        public static readonly string AssemblyPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            ServiceHostModule.Configure(builder);

            using (var container = builder.Build())
                container.Resolve<IOperationHost>().Execute(args);
        }

        readonly IServiceHostSettings _settings;
        readonly ILifetimeScope _lifetimeScope;

        int _statusFlag;
        CancellationTokenSource _shutDownCts;
        IDisposable _webAppToken;
        Task[] _backgroundTasks;

        public Program(IEnumerable<OperationDescriptor> operationDescriptors, IOperationHostIO io,
            IServiceHostSettings settings, ILifetimeScope lifetimeScope) :
            base(operationDescriptors, io)
        {
            _settings = settings;
            _lifetimeScope = lifetimeScope;
        }

        protected override string DefaultOperationName => ConsoleOperation.Name;

        public override string AppName => AssemblyName;

        public string BaseUrl => _settings.ServiceBaseUrl;

        public CancellationToken ShutDownToken => _shutDownCts.Token;

        public void Configure(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();

            // Set the dependency resolver for Web API.
            config.DependencyResolver = new AutofacWebApiDependencyResolver(_lifetimeScope);

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings = SerializationUtils.DataTransferSerializerSettings;

            config.Filters.Add(new ExceptionHandlingAttribute());

            // TODO: authentication/authorization if required

            config.Routes.MapHttpRoute(
                name: "ServiceApi",
                routeTemplate: "{action}",
                defaults: new { controller = "Service" }
            );

            appBuilder.UseWebApi(config);
        }

        public async Task StartUpAsync()
        {
            if (Interlocked.CompareExchange(ref _statusFlag, (int)Status.Starting, (int)Status.Stopped) != (int)Status.Stopped)
                throw new InvalidOperationException("Service host has been already started.");

            _shutDownCts = new CancellationTokenSource();

            _webAppToken = await Task.Run(() => WebApp.Start(_settings.ServiceBaseUrl, Configure))
                .ConfigureAwait(false);

            _backgroundTasks = _lifetimeScope.Resolve<IEnumerable<IBackgroundProcess>>()
                .Select(t => Task.Run(t.ExecuteAsync))
                .ToArray();

            Interlocked.Exchange(ref _statusFlag, (int)Status.Started);
        }

        public async Task ShutDownAsync()
        {
            if (Interlocked.CompareExchange(ref _statusFlag, (int)Status.Stopping, (int)Status.Started) != (int)Status.Started)
                throw new InvalidOperationException("Service host has not been started yet.");

            _shutDownCts.Cancel();

            await Task.WhenAll(_backgroundTasks)
                .WithTimeout(_settings.ShutDownTimeout)
                .ConfigureAwait(false);

            _backgroundTasks = null;

            _webAppToken.Dispose();
            _webAppToken = null;

            _shutDownCts.Dispose();
            _shutDownCts = null;

            Interlocked.Exchange(ref _statusFlag, (int)Status.Stopped);
        }
    }
}
