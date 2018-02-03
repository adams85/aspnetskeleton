using AspNetSkeleton.Core;
using AspNetSkeleton.Core.Infrastructure;
using Autofac;
using Autofac.Integration.WebApi;

namespace AspNetSkeleton.Api.Infrastructure
{
#if DISTRIBUTED
    public class ApiModule : CoreModule
#else
    public class ApiModule : Module
#endif
    {
        public static void Configure(ContainerBuilder builder)
        {
            // Register our dependencies
            builder.RegisterModule(new ApiModule());

            builder.RegisterApiControllers(typeof(WebApiApplication).Assembly);

            // useless because of filter caching
            // http://stackoverflow.com/questions/23659108/webapi-autofac-system-web-http-filters-actionfilterattribute-instance-per-requ
            // builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<ApiSettings>()
                .As<IApiSettings>()
                .As<ICoreSettings>()
                .SingleInstance();
        }
    }
}