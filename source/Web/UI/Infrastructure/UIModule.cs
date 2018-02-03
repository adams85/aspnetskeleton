using Autofac;
using Autofac.Integration.Mvc;
using System.Web.Mvc;
using AspNetSkeleton.Core.Infrastructure;
using AspNetSkeleton.Core;
using AspNetSkeleton.UI.Infrastructure.Security;
using AspNetSkeleton.UI.Infrastructure.Theming;
using AspNetSkeleton.UI.Infrastructure.Models;
using Karambolo.Common.Localization;
using System.Collections.Generic;
using AspNetSkeleton.UI.Infrastructure.Localization;

namespace AspNetSkeleton.UI.Infrastructure
{
#if DISTRIBUTED
    public class UIModule : CoreModule
    {
#else
    public class UIModule : AspNetSkeleton.Service.Host.Core.Infrastructure.ServiceHostCoreModule
    {
#endif
        public static void Configure(ContainerBuilder builder)
        {
            // Register our dependencies
            builder.RegisterModule(new UIModule());

            // Register dependencies in controllers
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // won't work with global filters
            // Register dependencies in filter attributes
            // builder.RegisterFilterProvider();

            // Register dependencies in custom views
            builder.RegisterSource(new ViewRegistrationSource());

            // Register HTTP request lifetime scoped registrations for the web abstraction classes
            builder.RegisterModule<AutofacWebTypesModule>();

            #region Model Metadata
            builder.RegisterAssemblyTypes(typeof(MvcApplication).Assembly)
                .As<IModelAttributesProviderConfigurer>();

            builder.Register(ctx =>
            {
                var providerBuilder = new ModelAttributesProviderBuilder();

                foreach (var configurer in ctx.Resolve<IEnumerable<IModelAttributesProviderConfigurer>>())
                    configurer.Configure(providerBuilder);

                return providerBuilder.Build();
            })
            .As<IModelAttributesProvider>()
            .SingleInstance();

            builder.RegisterType<DynamicModelMetadataProvider>()
                .As<ModelMetadataProvider>()
                .SingleInstance();

            builder.RegisterType<DynamicModelValidatorProvider>()
                .As<ModelValidatorProvider>()
                .SingleInstance();

            builder.RegisterType<DataErrorInfoModelValidatorProvider>()
                .As<ModelValidatorProvider>()
                .SingleInstance();

            builder.RegisterType<ClientDataTypeModelValidatorProvider>()
                .As<ModelValidatorProvider>()
                .SingleInstance(); 
            #endregion
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

#if DISTRIBUTED
            builder.RegisterType<UISettings>()
#else
            builder.RegisterType<UIServiceHostSettings>()
                .As<Service.Host.Core.IServiceHostCoreSettings>()
#endif
                .As<IUISettings>()
                .As<ICoreSettings>()
                .SingleInstance();

#if DISTRIBUTED
            builder.RegisterType<WebEnvironment>()
#else
            builder.RegisterType<WebServiceHostEnvironment>()
                .As<Service.Host.Core.Infrastructure.IServiceHostEnvironment>()
#endif
                .As<IEnvironment>()
                .SingleInstance();

#if !DISTRIBUTED
            builder.RegisterInstance(BackgroundWork.BackgroundTaskManager.Current)
                .As<Service.Host.Core.Infrastructure.BackgroundWork.IShutDownTokenAccessor>();
#endif

            builder.RegisterType<AccountManager>()
                .As<IAccountManager>()
                .SingleInstance();

            #region Localization
            if (MvcApplication.EnableLocalization)
            {
                builder.RegisterType<LocalizationProvider>()
                    .As<ILocalizationProvider>()
                    .As<IStartable>()
                    .SingleInstance();

                builder.RegisterType<LocalizationManager>()
                    .As<ILocalizationManager>()
                    .As<IViewLocalizer>()
                    .As<ITextLocalizer>()
                    .InstancePerRequest();
            }
            else
            {
                builder.RegisterType<NullLocalizationProvider>()
                    .As<ILocalizationProvider>()
                    .SingleInstance();

                builder.RegisterType<NullLocalizationManager>()
                    .As<ILocalizationManager>()
                    .As<IViewLocalizer>()
                    .As<ITextLocalizer>()
                    .SingleInstance();
            }
            #endregion

            #region Theming
            if (MvcApplication.EnableTheming)
            {
                builder.RegisterType<ThemeProvider>()
                    .As<IThemeProvider>()
                    .As<IStartable>()
                    .SingleInstance();

                builder.RegisterType<ThemeManager>()
                    .As<IThemeManager>()
                    .InstancePerRequest();
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
        }
    }
}