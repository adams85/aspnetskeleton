using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Core;
using AspNetSkeleton.UI.Infrastructure.Localization;
using AspNetSkeleton.UI.Infrastructure.Models;
using AspNetSkeleton.UI.Infrastructure.Security;
using AspNetSkeleton.UI.Infrastructure.Theming;
using AspNetSkeleton.UI.Middlewares;
using Autofac;
using Karambolo.AspNetCore.Bundling.Internal.Caching;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using WebMarkupMin.AspNetCore2;

namespace AspNetSkeleton.UI
{
    public class AppConfiguration :
#if DISTRIBUTED
        AppConfigurationBase
#else
        AspNetSkeleton.Service.Host.Core.ServiceHostCoreAppConfiguration
#endif
    {
        public AppConfiguration(IConfigurationRoot configuration) : base(configuration) { }

        public override void RegisterCommonServices(IServiceCollection services)
        {
            base.RegisterCommonServices(services);

            services.ConfigureByConvention<UISettings>(Configuration);
        }

        public override void RegisterAppComponents(ContainerBuilder builder)
        {
            base.RegisterAppComponents(builder);

            builder.RegisterType<App>()
                .WithParameter(TypedParameter.From(Console.Out))
                .As<IApp>()
                .SingleInstance();

#if DISTRIBUTED
            builder.RegisterType<Core.Infrastructure.ServiceProxyQueryDispatcher>()
                .As<Service.Contract.IQueryDispatcher>()
                .SingleInstance();

            builder.RegisterType<Core.Infrastructure.ServiceProxyCommandDispatcher>()
                .As<Service.Contract.ICommandDispatcher>()
                .SingleInstance();
#endif
        }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            var env = WebHostServices.GetRequiredService<IHostingEnvironment>();
            var settings = CommonContext.Resolve<IOptions<UISettings>>().Value;

            #region Response compression & minification
            if (settings.EnableResponseMinification.HasFlag(ResponseKind.Views) || settings.EnableResponseCompression)
            {
                var webMarkupMin = services.AddWebMarkupMin(o =>
                {
                    o.AllowCompressionInDevelopmentEnvironment = true;
                    o.AllowMinificationInDevelopmentEnvironment = true;
                    o.DisablePoweredByHttpHeaders = true;
                });

                if (settings.EnableResponseMinification.HasFlag(ResponseKind.Views))
                {
                    webMarkupMin.AddHtmlMinification(o => o.SupportedMediaTypes = new HashSet<string>() { "text/html" });
                    services.Configure<HtmlMinificationOptions>(Configuration.GetSection("Response").GetSection("HtmlMinification"));
                }

                if (settings.EnableResponseCompression)
                {
                    webMarkupMin.AddHttpCompression();
                    services.Configure<HttpCompressionOptions>(Configuration.GetSection("Response").GetSection("HttpCompression"));
                }
            }
            #endregion

            #region Bundling
            var bundling = services.AddBundling()
                .UseWebMarkupMin()
                .UseHashVersioning()
                .AddCss()
                .AddJs()
                .AddLess();

            if (settings.UsePersistentCache.HasFlag(ResponseKind.Bundles))
            {
                bundling.UseFileSystemCaching();
                services.Configure<FileSystemBundleCacheOptions>(Configuration.GetSection("Response").GetSection("BundleCaching"));
            }
            else
                bundling.UseMemoryCaching();

            if (settings.EnableResponseMinification.HasFlag(ResponseKind.Bundles))
                bundling.EnableMinification();

            if (settings.EnableResponseCaching.HasFlag(ResponseKind.Bundles))
                bundling.EnableCacheHeader(settings.CacheHeaderMaxAge);

            if (env.IsDevelopment())
                bundling.EnableChangeDetection();
            #endregion

            #region Authentication
            // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o => o.EventsType = typeof(UICookieAuthenticationEvents));

            services.AddScoped<UICookieAuthenticationEvents>();

            services.Configure<CookieAuthenticationOptions>(Configuration.GetSection("Authentication"));
            #endregion

            #region View caching
            if (settings.EnableResponseCaching.HasFlag(ResponseKind.Views))
            {
                if (settings.UsePersistentCache.HasFlag(ResponseKind.Views))
                    throw new NotImplementedException("Persistent view caching is not implemented.");

                services.AddResponseCaching();
                services.Configure<ResponseCachingOptions>(Configuration.GetSection("Response").GetSection("ViewCaching"));
            }
            #endregion

            #region MVC
            services.AddMvc()
                .ConfigureApplicationPartManager(m =>
                {
                    // restricting controller discovery to the current assembly
                    m.ApplicationParts.Clear();
                    m.ApplicationParts.Add(new AssemblyPart(typeof(App).Assembly));
                })
                .AddControllersAsServices()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();
            #endregion

            return null;
        }

        public override void RegisterBranchComponents(ContainerBuilder builder)
        {
            base.RegisterBranchComponents(builder);

            var settings = AppContext.Resolve<IOptions<UISettings>>().Value;

            builder.RegisterType<AccountManager>()
                .As<IAccountManager>()
                .SingleInstance();

            #region Localization
            // overriding default IStringLocalizerFactory and IHtmlLocalizerFactory
            builder.RegisterType<TextLocalizerFactory>()
                .As<IStringLocalizerFactory>()
                .As<IHtmlLocalizerFactory>()
                .SingleInstance();

            if (settings.EnableLocalization)
            {
                builder.RegisterType<LocalizationProvider>()
                    .As<ILocalizationProvider>()
                    .As<IAppBranchInitializer>()
                    .SingleInstance();

                builder.RegisterType<LocalizationManager>()
                    .As<ILocalizationManager>()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<NullLocalizationProvider>()
                    .As<ILocalizationProvider>()
                    .SingleInstance();

                builder.RegisterType<NullLocalizationManager>()
                    .As<ILocalizationManager>()
                    .SingleInstance();
            }
            #endregion

            #region Theming
            if (settings.EnableTheming)
            {
                builder.RegisterType<ThemeProvider>()
                    .As<IThemeProvider>()
                    .As<IAppBranchInitializer>()
                    .SingleInstance();

                builder.RegisterType<ThemeManager>()
                    .As<IThemeManager>()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<NullThemeProvider>()
                    .As<IThemeProvider>()
                    .SingleInstance();

                builder.RegisterType<NullThemeManager>()
                    .As<IThemeManager>()
                    .SingleInstance();
            }
            #endregion

            #region Model Metadata
            // overriding default IModelMetadataProvider
            builder.RegisterType<DynamicModelMetadataProvider>()
                .As<IModelMetadataProvider>()
                .SingleInstance();

            builder.RegisterAssemblyTypes(typeof(App).Assembly)
                .As<IModelAttributesProviderConfigurer>();

            builder.Register(ctx =>
            {
                var providerBuilder = new ModelAttributesProviderBuilder();

                foreach (var configurer in ctx.Resolve<IEnumerable<IModelAttributesProviderConfigurer>>())
                    configurer.Configure(providerBuilder);

                return providerBuilder.Build();
            })
            .As<IDynamicModelAttributesProvider>()
            .SingleInstance();
            #endregion
        }

        public override void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var settings = app.ApplicationServices.GetRequiredService<IOptions<UISettings>>().Value;

            var themeProvider = app.ApplicationServices.GetRequiredService<IThemeProvider>();
            var localizationProvider = app.ApplicationServices.GetRequiredService<ILocalizationProvider>();

            base.Configure(app);

            #region Exception handling
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
                app.UseExceptionHandler("/Home/Error");

            app.UseStatusCodePages();

            app.UseMiddleware<ExceptionFilterMiddleware>();
            #endregion

            #region Localization
            if (settings.EnableLocalization)
            {
                var supportedCultures = localizationProvider.Cultures
                    .Select(c => CultureInfo.CreateSpecificCulture(c))
                    .ToArray();

                app.UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture(settings.DefaultCulture, settings.DefaultCulture),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures,
                    RequestCultureProviders = new[] { localizationProvider }
                });
            }
            else
            {
                var defaultCulture = CultureInfo.CreateSpecificCulture(settings.DefaultCulture);
                app.UseMiddleware<DefaultCultureMiddleware>(defaultCulture);
            }
            #endregion

            #region Response compression & minification
            if (settings.EnableResponseMinification.HasFlag(ResponseKind.Views) || settings.EnableResponseCompression)
                app.UseWebMarkupMin();
            #endregion

            #region Bundling
            app.UseBundling(bundles =>
            {
                bundles.AddJs("/js/site.js")
                    .Include("/js/*.js");

                bundles.AddJs("/js/bootstrap/bootstrap-validation.js")
                    .Include("/js/bootstrap/bootstrap-validation.js");

                Array.ForEach(themeProvider.Themes, t =>
                {
                    var basePath = "/css/themes/" + t;

                    bundles.AddLess(basePath + "/bootstrap.css")
                        .Include(basePath + "/bootstrap.less");

                    bundles.AddLess(basePath + "/site.css")
                        .Include(basePath + "/site.less");
                });
            });
            #endregion

            #region Static files
            var staticFileOptions = new StaticFileOptions();
            if (settings.EnableResponseCaching.HasFlag(ResponseKind.StaticFiles))
                staticFileOptions.OnPrepareResponse = ctx =>
                {
                    var headers = ctx.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new CacheControlHeaderValue { MaxAge = settings.CacheHeaderMaxAge };
                };

            app.UseStaticFiles(staticFileOptions);
            #endregion

            #region Authentication
            app.UseAuthentication();
            #endregion

            #region View caching
            if (settings.EnableResponseCaching.HasFlag(ResponseKind.Views))
                app.UseResponseCaching();
            #endregion

            #region MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "AreaDefault",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "Default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            #endregion
        }
    }
}
