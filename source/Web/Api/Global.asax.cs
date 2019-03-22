using System;
using Autofac;
using AspNetSkeleton.Core.Infrastructure;
using AspNetSkeleton.Api.Infrastructure;
using System.Web.Http.Filters;
using AspNetSkeleton.Api.Filters;
using System.Net.Http;
using System.Collections.ObjectModel;
using AspNetSkeleton.Api.Handlers;
using System.Net.Http.Formatting;
using AspNetSkeleton.Common.Utils;
using Autofac.Integration.WebApi;
using System.Web.Http;
using System.Web.Routing;
using Karambolo.Common;
using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Api.Contract;
using System.Linq;

namespace AspNetSkeleton.Api
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
#if DISTRIBUTED
        static WebApiApplication()
        {
            var builder = new ContainerBuilder();
            ApiModule.Configure(builder);
            builder.Build();
        }
#endif

        public static void RegisterRoutes(RouteCollection routes, IComponentContext context)
        {
            var settings = context.Resolve<IApiSettings>();

            routes.MapHttpRoute(
                name: "ApiAdmin",
                routeTemplate: UriUtils.BuildPath(settings.ApiBasePath, "Admin/{action}"),
                defaults: new { controller = "Admin" }
            );

            routes.MapHttpRoute(
                name: "ApiDefault",
                routeTemplate: UriUtils.BuildPath(settings.ApiBasePath, "{controller}/{id}"),
                defaults: new { id = RouteParameter.Optional }
            );
        }

        static void RegisterWebApiFilters(HttpFilterCollection filters, IComponentContext context)
        {
            filters.Add(new ExceptionHandlingAttribute());
            filters.Add(new AuthenticationAttribute());
        }

        static void RegisterWebApiHandlers(Collection<DelegatingHandler> handlers, IComponentContext context)
        {
            handlers.Add(new SetAuthHeaderHandler());
        }

        static void SetupWebApiFormatters(MediaTypeFormatterCollection formatters, IComponentContext context)
        {
            formatters.Remove(formatters.XmlFormatter);
            formatters.JsonFormatter.SerializerSettings = SerializationUtils.CreateDataTransferSerializerSettings(new Predicate<Type>[]
            {
                CommonTypes.DataObjectTypes.Contains,
                ServiceContractTypes.DataObjectTypes.Contains,
                ServiceContractTypes.QueryTypes.Contains,
                ServiceContractTypes.CommandTypes.Contains,
                ApiContractTypes.DataObjectTypes.Contains,
            });
        }

        public static void Configure(ILifetimeScope lifetimeScope)
        {
            // Set the dependency resolver for Web API.
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(lifetimeScope);

            RegisterRoutes(RouteTable.Routes, lifetimeScope);
            RegisterWebApiFilters(GlobalConfiguration.Configuration.Filters, lifetimeScope);
            RegisterWebApiHandlers(GlobalConfiguration.Configuration.MessageHandlers, lifetimeScope);

            SetupWebApiFormatters(GlobalConfiguration.Configuration.Formatters, lifetimeScope);
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            Configure(CoreModule.RootLifetimeScope);
        }
    }
}