using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Core.Infrastructure;
using AspNetSkeleton.Core.Utils;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Karambolo.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Core
{
    public interface IApp
    {
        CancellationToken ShutDownToken { get; }

        Task StartUpAsync();
        Task ShutDownAsync();
    }

    public abstract class AppBase : IApp, IStartup, IDisposable
    {
        static void RunInitializers(IEnumerable<IInitializer> initializers)
        {
            Task.WhenAll(initializers.Select(it => Task.Run((Action)it.Initialize))).GetAwaiter().GetResult();
        }

        static void ConfigureBranch(IApplicationBuilder app, IStartup configuration)
        {
            RunInitializers(app.ApplicationServices.GetRequiredService<IEnumerable<IAppBranchInitializer>>());
            configuration.Configure(app);
        }

        static IApplicationBuilder CreateBranch(IApplicationBuilder app, BranchPredicate predicate, IServiceProvider serviceProvider, IStartup configuration)
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            return app.MapWhen(new Func<HttpContext, bool>(predicate), ab =>
            {
                ab.ApplicationServices = serviceProvider;

                // adding middleware responsible for replacing request service provider
                ab.Use((ctx, next) =>
                {
                    var requestServicesFeature = ctx.Features.Get<IServiceProvidersFeature>() as RequestServicesFeature;
                    requestServicesFeature?.Dispose();

                    requestServicesFeature = new RequestServicesFeature(scopeFactory);
                    ctx.Features.Set<IServiceProvidersFeature>(requestServicesFeature);

                    return next();
                });

                ConfigureBranch(ab, configuration);
            });
        }

        enum Status
        {
            Stopped,
            Starting,
            Started,
            Stopping
        }

        int _statusFlag;
        CancellationTokenSource _shutDownCts;

        IServiceCollection _branchCommonServices;
        IApplicationLifetime _appLifetime;

        readonly IReadOnlyList<(IAppConfiguration Configuration, BranchPredicate BranchPredicate)> _branches;
        readonly IAppConfiguration _mainBranch;

        ISet<ServiceDescriptor> _sharedServices;
        readonly ILifetimeScopeFactory _lifetimeScopeFactory;
        readonly CoreSettings _settings;

        IServiceProvider _builderServices;

        public AppBase(IEnumerable<IAppConfiguration> configurations, TextWriter statusWriter, IComponentContext context)
        {
            var branches = new List<(IAppConfiguration, BranchPredicate)>();

            BranchPredicate branchPredicate;
            foreach (var configuration in configurations)
                if ((branchPredicate = configuration.GetBranchPredicate(context)) != null)
                    branches.Add((configuration, branchPredicate));
                else if (_mainBranch == null)
                    _mainBranch = configuration;
                else
                    throw new ArgumentException(null, nameof(configuration));

            _branches = branches;

            StatusWriter = statusWriter ?? TextWriter.Null;

            var commonServices = context.Resolve<ICommonServicesAccessor>().Services;
            _sharedServices = Enumerable.ToHashSet(commonServices, ServiceCollectionUtils.DescriptorEqualityComparer);
            _lifetimeScopeFactory = context.Resolve<ILifetimeScopeFactory>();
            _settings = context.Resolve<IOptions<CoreSettings>>().Value;

            _webHost = new Lazy<IWebHost>(CreateWebHost, isThreadSafe: false);

            RunInitializers(context.Resolve<IEnumerable<IAppInitializer>>());
        }

        protected abstract void ConfigureWebHost(IWebHostBuilder builder);

        protected virtual bool IsSharedService(ServiceDescriptor descriptor)
        {
            return _sharedServices.Contains(descriptor);
        }

        IWebHost CreateWebHost()
        {
            var builder = new WebHostBuilder();

            ConfigureWebHost(builder);

            builder
                .UseSetting(WebHostDefaults.ApplicationKey, GetType().Assembly.FullName)                
                .ConfigureServices(sc => sc.Remove(IsSharedService)) // removing shared registrations
                .ConfigureServices(sc => sc.AddSingleton(AsStartup));

            return builder.Build();
        }

        protected virtual void DisposeCore() { }

        public void Dispose()
        {
            DisposeCore();

            if (_webHost.IsValueCreated)
                _webHost.Value.Dispose();
        }

        protected TextWriter StatusWriter { get; }

        readonly Lazy<IWebHost> _webHost;
        protected IWebHost WebHost => _webHost.Value;

        public CancellationToken ShutDownToken => _shutDownCts?.Token ?? throw new InvalidOperationException();

        IStartup AsStartup(IServiceProvider serviceProvider)
        {
            _builderServices = serviceProvider;
            return this;
        }

        IServiceProvider CreateServiceProvider(IServiceCollection services, IAppConfiguration configuration, bool isBranch)
        {
            if (configuration != null)
            {
                configuration.OnConfigureWebHost(_builderServices);
                configuration.ConfigureServices(services);
            }

            // sharing app lifetime instance so that events fire in branches
            if (isBranch)
                services.Replace(ServiceDescriptor.Singleton(_appLifetime));

            var lifetimeScope = _lifetimeScopeFactory.CreateChildScope(new object(), (cb, ctx) =>
            {
                configuration?.OnConfigureBranch(ctx);
                cb.Populate(services);
                configuration?.RegisterBranchComponents(cb);
            });
            var result = new AutofacServiceProvider(lifetimeScope);

            if (!isBranch)
                _appLifetime = result.GetService<IApplicationLifetime>();

            _appLifetime.ApplicationStopped.Register(lifetimeScope.Dispose);

            return result;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // make a copy for branch configuration
            _branchCommonServices = services.Clone();

            return CreateServiceProvider(services, _mainBranch, isBranch: false);
        }

        public void Configure(IApplicationBuilder app)
        {
            var n = _branches.Count;
            for (var i = 0; i < n; i++)
            {
                var cfg = _branches[i];
                var services = _branchCommonServices.Clone();
                var serviceProvider = CreateServiceProvider(services, cfg.Configuration, isBranch: true);

                CreateBranch(app, cfg.BranchPredicate, serviceProvider, cfg.Configuration);
            }

            if (_mainBranch != null)
                ConfigureBranch(app, _mainBranch);
        }

        protected virtual Task StartUpCoreAsync()
        {
            return WebHost.StartAsync(ShutDownToken);
        }

        public async Task StartUpAsync()
        {
            if (Interlocked.CompareExchange(ref _statusFlag, (int)Status.Starting, (int)Status.Stopped) != (int)Status.Stopped)
                throw new InvalidOperationException("Application has been already started.");

            _shutDownCts = new CancellationTokenSource();

            await StartUpCoreAsync().ConfigureAwait(false);

            Interlocked.Exchange(ref _statusFlag, (int)Status.Started);

            OnStarted();
        }

        protected virtual void OnStarted()
        {
            var env = WebHost.Services.GetService<IHostingEnvironment>();

            StatusWriter.WriteLine($"Hosting environment: {env.EnvironmentName}");
            StatusWriter.WriteLine($"Content root path: {env.ContentRootPath}");
            var addresses = WebHost.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
            if (addresses != null)
                foreach (var address in addresses)
                    StatusWriter.WriteLine($"Now listening on: {address}");
        }

        protected virtual Task ShutDownCoreAsync()
        {
            return WebHost.WaitForShutdownAsync(ShutDownToken);
        }

        public async Task ShutDownAsync()
        {
            if (Interlocked.CompareExchange(ref _statusFlag, (int)Status.Stopping, (int)Status.Started) != (int)Status.Started)
                throw new InvalidOperationException("Application has not been started yet.");

            _shutDownCts.Cancel();

            await ShutDownCoreAsync()
                .WithTimeout(_settings.ShutDownTimeOut)
                .ConfigureAwait(false);

            _shutDownCts.Dispose();
            _shutDownCts = null;

            Interlocked.Exchange(ref _statusFlag, (int)Status.Stopped);

            OnStopped();
        }

        protected virtual void OnStopped() { }
    }
}
