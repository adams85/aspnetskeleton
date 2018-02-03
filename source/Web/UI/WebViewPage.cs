using System.Web.Mvc;
using AspNetSkeleton.UI.Infrastructure.Localization;
using AspNetSkeleton.UI.Infrastructure.Theming;

namespace AspNetSkeleton.UI
{
    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {
        public WebViewPage()
        {
            // WORKAROUND: dependency injection won't work with layout pages
            // https://code.google.com/p/autofac/issues/detail?id=349
            Settings = DependencyResolver.Current.GetService<IUISettings>();
            LocalizationManager = DependencyResolver.Current.GetService<ILocalizationManager>();
            ThemeManager = DependencyResolver.Current.GetService<IThemeManager>();
        }

        public IUISettings Settings { get; }
        public ILocalizationManager LocalizationManager { get; }
        public IThemeManager ThemeManager { get; }

        public IViewLocalizer T => LocalizationManager;
    }
}