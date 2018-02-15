using System;
using System.IO;
using AspNetSkeleton.Core.Infrastructure;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Core
{
    public delegate bool BranchPredicate(HttpContext httpContext);

    public interface IAppConfiguration : IContainerConfiguration, IStartup
    {
        void OnConfigureApp(IComponentContext context);
        void RegisterAppComponents(ContainerBuilder builder);
        BranchPredicate GetBranchPredicate(IComponentContext context);
        void OnConfigureWebHost(IServiceProvider builderServices);
        void OnConfigureBranch(IComponentContext context);
        void RegisterBranchComponents(ContainerBuilder builder);
    }

    public abstract class AppConfigurationBase : IAppConfiguration
    {
        protected AppConfigurationBase(IConfigurationRoot configuration)
        {
            Configuration = configuration;
        }

        public IConfigurationRoot Configuration { get; }

        protected IComponentContext CommonContext { get; private set; }
        protected IServiceProvider WebHostServices { get; private set; }
        protected IComponentContext AppContext { get; private set; }

        public virtual void RegisterCommonServices(IServiceCollection services) { }

        public virtual void RegisterCommonComponents(ContainerBuilder builder) { }

        public void OnConfigureApp(IComponentContext context)
        {
            CommonContext = context;
        }

        public virtual void RegisterAppComponents(ContainerBuilder builder) { }

        public virtual BranchPredicate GetBranchPredicate(IComponentContext context)
        {
            return null;
        }

        public void OnConfigureWebHost(IServiceProvider builderServices)
        {
            WebHostServices = builderServices;
        }

        public void OnConfigureBranch(IComponentContext context)
        {
            AppContext = context;
        }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // http://www.paraesthesia.com/archive/2016/06/15/set-up-asp-net-dataprotection-in-a-farm/

            var dataProtectionSettings = CommonContext.Resolve<IOptions<DataProtectionSettings>>().Value;

            var dataProtection = services.AddDataProtection();

            if (dataProtectionSettings.ApplicationName != null)
                dataProtection.SetApplicationName(dataProtectionSettings.ApplicationName);

            if (dataProtectionSettings.KeyLifetime != null)
                dataProtection.SetDefaultKeyLifetime(dataProtectionSettings.KeyLifetime.Value);

            if (dataProtectionSettings.KeyStorePath != null)
                dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionSettings.KeyStorePath));

            if (dataProtectionSettings.DisableAutomaticKeyGeneration)
                dataProtection.DisableAutomaticKeyGeneration();

            return null;
        }

        public virtual void RegisterBranchComponents(ContainerBuilder builder) { }

        public abstract void Configure(IApplicationBuilder app);
    }
}
