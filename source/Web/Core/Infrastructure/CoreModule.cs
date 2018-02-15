using System;
using System.Linq;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Common.Infrastructure;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AspNetSkeleton.Base;
using AspNetSkeleton.Core.Hosting;
using Karambolo.Extensions.Logging.File;
using Microsoft.AspNetCore.Hosting;
using AspNetSkeleton.Core.Utils;
using Microsoft.AspNetCore.DataProtection;

namespace AspNetSkeleton.Core.Infrastructure
{
    public interface IContainerConfiguration
    {
        IConfigurationRoot Configuration { get; }

        void RegisterCommonServices(IServiceCollection services);
        void RegisterCommonComponents(ContainerBuilder builder);
    }

    public interface ICommonServicesAccessor
    {
        ServiceDescriptor[] Services { get; }
    }

    public class CoreModule : Module, IContainerConfiguration
    {
        class CommonServicesAccessor : ICommonServicesAccessor
        {
            public ServiceDescriptor[] Services { get; set; }
        }

        readonly IHostConfiguration _hostConfiguration;
        readonly IAppConfiguration[] _appConfigurations;

        public CoreModule(IHostConfiguration hostConfiguration, params IAppConfiguration[] appConfigurations)
        {
            _hostConfiguration = hostConfiguration;
            _appConfigurations = appConfigurations;
        }

        public IConfigurationRoot Configuration => _hostConfiguration.Configuration;

        protected virtual PropertyInjectorModule CreatePropertyInjectorModule()
        {
            return new PropertyInjectorModule();
        }

        protected virtual void ConfigureLogging(ILoggingBuilder builder)
        {
            var config = Configuration.GetSection("Logging");
            if (config != null)
                builder.AddConfiguration(config);

            config = Configuration.GetSection("Logging")?.GetSection(FileLoggerProvider.Alias);
            if (config != null)
            {
                builder.Services.Configure<FileLoggerOptions>(config);
                builder.AddFile(new FileLoggerContext(AppEnvironment.Instance.AppBasePath, "default.log"));
            }

            if (ConfigurationHelper.EnvironmentName == EnvironmentName.Development)
                builder.AddConsole();
        }

        public virtual void RegisterCommonServices(IServiceCollection services)
        {
            #region Core Infrastructure
            services.AddOptions();
            services.AddLogging(ConfigureLogging);

            services.ConfigureByConvention<CoreSettings>(Configuration);
            services.Configure<DataProtectionSettings>(Configuration.GetSection("DataProtection"));
            #endregion

            _hostConfiguration.RegisterCommonServices(services);
            Array.ForEach(_appConfigurations, cfg => cfg.RegisterCommonServices(services));
        }

        public virtual void RegisterCommonComponents(ContainerBuilder builder)
        {
            #region Core Infrastructure
            builder
                .RegisterGeneric(typeof(AutofacKeyedProvider<>))
                .As(typeof(IKeyedProvider<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<SystemClock>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterInstance(AppEnvironment.Instance)
                .As<IAppEnvironment>();
            #endregion

            #region Host
            builder.RegisterType<HostScope>()
                .As<IHostScope>()
                .ExternallyOwned()
                .SingleInstance();

            _hostConfiguration.RegisterCommonComponents(builder);

            builder.RegisterInstance(_hostConfiguration)
                .As<IHostConfiguration>();
            #endregion

            #region Apps
            // app scope lives beside (not within) host scope (since configured as singleton)
            // because app needs no components registered for host
            builder.RegisterType<AppScope>()
                .As<IAppScope>()
                .ExternallyOwned()
                .SingleInstance();

            Array.ForEach(_appConfigurations, cfg => cfg.RegisterCommonComponents(builder));

            Array.ForEach(_appConfigurations, cfg =>
                builder.RegisterInstance(cfg)
                    .As<IAppConfiguration>());
            #endregion
        }

        protected sealed override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var propertyInjectorModule = CreatePropertyInjectorModule();

            builder.RegisterModule(propertyInjectorModule);

            builder.RegisterType<LifetimeScopeFactory>()
                .WithParameter(TypedParameter.From(propertyInjectorModule))
                .As<ILifetimeScopeFactory>()
                .InstancePerLifetimeScope();

            var services = new ServiceCollection();
            RegisterCommonServices(services);
            builder.Populate(services);

            builder.RegisterInstance(new CommonServicesAccessor { Services = services.ToArray() })
                .As<ICommonServicesAccessor>();

            RegisterCommonComponents(builder);
        }
    }
}
