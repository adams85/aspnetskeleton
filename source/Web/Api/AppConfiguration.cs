using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.Api.Handlers;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Common.Utils;
using AspNetSkeleton.Core;
using AspNetSkeleton.Core.Infrastructure;
using AspNetSkeleton.Core.Infrastructure.Security;
using AspNetSkeleton.Service.Contract;
using Autofac;
using Karambolo.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Api
{
    public class AppConfiguration : AppConfigurationBase
    {
        public AppConfiguration(IConfigurationRoot configuration) : base(configuration) { }

        public override void RegisterCommonServices(IServiceCollection services)
        {
            base.RegisterCommonServices(services);

            services.ConfigureByConvention<ApiSettings>(Configuration);
        }

        public override void RegisterAppComponents(ContainerBuilder builder)
        {
            base.RegisterAppComponents(builder);

#if DISTRIBUTED
            builder.RegisterType<App>()
                .WithParameter(TypedParameter.From(Console.Out))
                .As<IApp>()
                .SingleInstance();

            builder.RegisterType<ServiceProxyQueryDispatcher>()
                .As<IQueryDispatcher>()
                .SingleInstance();

            builder.RegisterType<ServiceProxyCommandDispatcher>()
                .As<ICommandDispatcher>()
                .SingleInstance();
#endif
        }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IExceptionHandler, ExceptionHandler>();

            // TODO: configure data protection API if necessary
            // http://www.paraesthesia.com/archive/2016/06/15/set-up-asp-net-dataprotection-in-a-farm/

            services.AddAuthentication(ApiAuthenticationHandler.AuthenticationScheme)
                .AddScheme<TokenAuthenticationOptions, ApiAuthenticationHandler>(ApiAuthenticationHandler.AuthenticationScheme, null);
            services.AddSingleton<IPostConfigureOptions<TokenAuthenticationOptions>, TokenAuthenticationOptions.Configurer>();

            services.AddMvc(o => o.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>())
                .ConfigureApplicationPartManager(m =>
                {
                    // restricting controller discovery to the current assembly
                    m.ApplicationParts.Clear();
                    m.ApplicationParts.Add(new AssemblyPart(typeof(App).Assembly));
                })
                .AddControllersAsServices()
                .AddJsonOptions(o => SerializationUtils.ConfigureDataTransferSerializerSettings(o.SerializerSettings));

            return null;
        }

        public override void Configure(IApplicationBuilder app)
        {
            var exceptionHandler = app.ApplicationServices.GetRequiredService<IExceptionHandler>();
            app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = exceptionHandler.Handle });

            app.UseAuthentication();

            app.UseMvc();
        }

        public override BranchPredicate GetBranchPredicate(IComponentContext context)
        {
            var settings = context.Resolve<IOptions<ApiSettings>>().Value;

            if (!string.IsNullOrEmpty(settings.ApiBasePath))
                return ctx =>
                {
                    var request = ctx.Request;
                    PathString prefix = settings.ApiBasePath;
                    if (request.Path.StartsWithSegments(prefix, out PathString remaining))
                    {
                        request.PathBase += prefix;
                        request.Path = remaining;
                        return true;
                    }
                    else
                        return false;
                };
            else
                return null;
        }
    }
}
