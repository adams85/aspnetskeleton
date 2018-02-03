using System.Globalization;
using System.Web.Mvc;
using System.Threading;
using AspNetSkeleton.UI.Infrastructure.Localization;
using System;

namespace AspNetSkeleton.UI.Filters
{
    public class CultureHandlerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var success = true;

            CultureInfo culture;
            try { culture = new CultureInfo(filterContext.RouteData.Values["culture"].ToString()); }
            catch (CultureNotFoundException)
            {
                success = false;
                culture = UIConstants.DefaultCulture;
            }

            if (success)
            {
                var localizationProvider = DependencyResolver.Current.GetService<ILocalizationProvider>();
                if (Array.IndexOf(localizationProvider.Cultures, culture.Name) < 0)
                    culture = UIConstants.DefaultCulture;
            }

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}