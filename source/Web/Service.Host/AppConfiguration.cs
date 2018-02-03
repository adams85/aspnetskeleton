using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Common.Utils;
using AspNetSkeleton.Core;
using AspNetSkeleton.Service.Host.Core;
using AspNetSkeleton.Service.Host.Handlers;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetSkeleton.Service.Host
{
    public class AppConfiguration : ServiceHostCoreAppConfiguration
    {
        public AppConfiguration(IConfigurationRoot configuration) : base(configuration) { }

        public override void RegisterCommonServices(IServiceCollection services)
        {
            base.RegisterCommonServices(services);

            services.ConfigureByConvention<ServiceHostSettings>(Configuration);
        }

        public override void RegisterAppComponents(ContainerBuilder builder)
        {
            base.RegisterAppComponents(builder);

            builder.RegisterType<App>()
                .WithParameter(TypedParameter.From(Console.Out))
                .As<IApp>()
                .SingleInstance();
        }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IExceptionHandler, ExceptionHandler>();

            services.AddMvc(o => o.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>())
                .AddControllersAsServices()
                .AddJsonOptions(o => SerializationUtils.ConfigureDataTransferSerializerSettings(o.SerializerSettings));

            return null;
        }

        public override void Configure(IApplicationBuilder app)
        {
            var exceptionHandler = app.ApplicationServices.GetRequiredService<IExceptionHandler>();
            app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = exceptionHandler.Handle });

            // TODO: authentication/authorization if required

            app.UseMvc();
        }
    }
}
