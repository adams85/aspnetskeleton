using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Core.Infrastructure;
using Autofac;
using Karambolo.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        public virtual void RegisterBranchComponents(ContainerBuilder builder) { }

        public abstract IServiceProvider ConfigureServices(IServiceCollection services);

        public abstract void Configure(IApplicationBuilder app);
    }
}
