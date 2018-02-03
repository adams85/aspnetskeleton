using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace AspNetSkeleton.UI.Infrastructure
{
    public static class RouteCollectionExtensions
    {
        public static Route MapUIRoute(this RouteCollection routes, string name, string url)
        {
            return routes.MapUIRoute(name, url, null, null);
        }

        public static Route MapUIRoute(this RouteCollection routes, string name, string url, object defaults)
        {
            return routes.MapUIRoute(name, url, defaults, null);
        }

        public static Route MapUIRoute(this RouteCollection routes, string name, string url, object defaults, object constraints)
        {
            return routes.MapUIRoute(name, url, defaults, constraints, null);
        }

        public static Route MapUIRoute(this RouteCollection routes, string name, string url, string[] namespaces)
        {
            return routes.MapUIRoute(name, url, null, null, namespaces);
        }

        public static Route MapUIRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
        {
            return routes.MapUIRoute(name, url, defaults, null, namespaces);
        }

        public static Route MapUIRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
        {
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));

            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var defaultsDictionary = CreateRouteValueDictionary(defaults);           
            var constraintsDictionary = CreateRouteValueDictionary(constraints);

            if (MvcApplication.EnableLocalization)
            {
                url = "{culture}/" + url;
                defaultsDictionary["culture"] = UIConstants.DefaultCulture.Name;
                constraintsDictionary["culture"] = @"^[a-z]{2,3}(?:-[A-Z][a-z]*)?(?:-[A-Z]{2,3})?$";
            }

            var route = new Route(url, new UIRouteHandler())
            {
                Defaults = defaultsDictionary,
                Constraints = constraintsDictionary,
                DataTokens = new RouteValueDictionary()
            };

            ValidateConstraints(route);

            if (namespaces != null && namespaces.Length > 0)
                route.DataTokens["Namespaces"] = namespaces;

            routes.Add(name, route);

            return route;
        }

        static void ValidateConstraints(Route route)
        {
            if (route.Constraints == null)
                return;

            foreach (var current in route.Constraints)
                if (!(current.Value is string) && !(current.Value is IRouteConstraint))
                    throw new InvalidOperationException($"The constraint entry '{current.Key}' on the route with route template '{route.Url}' must have a string value or be of a type which implements '{typeof(IRouteConstraint).FullName}'.");
        }

        static RouteValueDictionary CreateRouteValueDictionary(object values)
        {
            if (values is IDictionary<string, object> dictionary)
                return new RouteValueDictionary(dictionary);

            return new RouteValueDictionary(values);
        }
    }
}