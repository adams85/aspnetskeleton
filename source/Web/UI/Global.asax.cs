using System;
using Autofac;
using AspNetSkeleton.Core.Infrastructure;
using AspNetSkeleton.UI.Infrastructure;
using System.Web.Mvc;
using AspNetSkeleton.UI.Filters;
using Autofac.Integration.Mvc;
using System.Web.Routing;
using System.Web.Optimization;
using AspNetSkeleton.UI.Helpers;
using AspNetSkeleton.UI.Infrastructure.Theming;
using Karambolo.Common;

namespace AspNetSkeleton.UI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        static MvcApplication()
        {
            var builder = new ContainerBuilder();

#if !DISTRIBUTED
            Api.Infrastructure.ApiModule.Configure(builder);
#endif
            UIModule.Configure(builder);

            builder.Build();
        }

        public static bool EnableLocalization => false;
        public static bool EnableTheming => false;

        static void RegisterRoutes(RouteCollection routes, IComponentContext context)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.IgnoreRoute("Files/{*pathInfo}");

            routes.MapUIRoute
            (
                name: "DashboardDefault",
                url: "Dashboard/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { typeof(Areas.Dashboard.Controllers.HomeController).Namespace }
            )
            .ForArea("Dashboard");

            routes.MapUIRoute
            (
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { typeof(Controllers.HomeController).Namespace }
            );
        }

        static void RegisterFilters(GlobalFilterCollection filters, IComponentContext context)
        {
            filters.Add(new ExceptionHandlingAttribute());

            if (EnableLocalization)
                filters.Add(new CultureHandlerAttribute());
        }

        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles, IComponentContext context)
        {
            // virtual path must not match an existing directory!!!
            // https://support.appharbor.com/discussions/problems/4700-403-access-denied-when-using-scriptbundle

            bundles.Add(new ScriptBundle("~/Static/Scripts/jquery")
                .Include(
                    "~/Static/Scripts/jquery-{version}.js",
                    "~/Static/Scripts/jquery.validate.js",
                    "~/Static/Scripts/jquery.validate.unobtrusive.js"));

            bundles.Add(new ScriptBundle("~/Static/Scripts/iefix")
                .Include("~/Static/Scripts/iefix.js"));

            bundles.Add(new ScriptBundle("~/Static/Scripts/Site")
                .Include(
                    "~/Static/Scripts/bootstrap.js",
                    "~/Static/Scripts/bootstrap-validation.js",
                    "~/Static/Scripts/site.js"));

            var themeProvider = context.Resolve<IThemeProvider>();
            Array.ForEach(themeProvider.Themes, t =>
                bundles.Add(new LessBundle("~/Static/Stylesheets/Site/" + t)
                    .Include(
                        UriUtils.BuildPath(ThemeProvider.BaseUrl, t, "bootstrap.less"),
                        UriUtils.BuildPath(ThemeProvider.BaseUrl, t, "site.less"))));
        }

        public static void Configure(ILifetimeScope lifetimeScope)
        {
            // Set the dependency resolver for MVC.
            DependencyResolver.SetResolver(new AutofacDependencyResolver(lifetimeScope));

            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes, lifetimeScope);
            RegisterFilters(GlobalFilters.Filters, lifetimeScope);
            RegisterBundles(BundleTable.Bundles, lifetimeScope);

            // DynamicModelMetadataProvider and DynamicModelValidatorProvider are registered in the container
            ModelValidatorProviders.Providers.Clear();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            var lifetimeScope = CoreModule.RootLifetimeScope;

#if !DISTRIBUTED
            // order matters: webapi routes needs to be registered first!
            Api.WebApiApplication.Configure(lifetimeScope);
#endif

            Configure(lifetimeScope);

#if !DISTRIBUTED
            foreach (var backgroundProcess in lifetimeScope.Resolve<System.Collections.Generic.IEnumerable<Service.Host.Core.Infrastructure.BackgroundWork.IBackgroundProcess>>())
                Infrastructure.BackgroundWork.BackgroundTaskManager.Current.Run(backgroundProcess.ExecuteAsync);
#endif
        }
    }
}