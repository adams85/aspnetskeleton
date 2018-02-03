using System.Web.Routing;
using PagedList;
using AspNetSkeleton.Service.Contract;
using System.Web.Mvc;

namespace AspNetSkeleton.UI.Helpers
{
    public static class MvcUtils
    {
        public static IPagedList<T> AsPagedList<T>(this ListResult<T> @this)
        {
            return new StaticPagedList<T>(@this.Rows, @this.PageIndex + 1, @this.PageSize, @this.TotalRowCount);
        }

        public static Route ForArea(this Route @this, string area)
        {
            @this.DataTokens["area"] = area;
            return @this;
        }

        public static Route UseDefaultHandler(this Route @this)
        {
            @this.RouteHandler = new MvcRouteHandler();
            return @this;
        }

        public static RouteValueDictionary Merge(this RouteValueDictionary @this, RouteValueDictionary values)
        {
            foreach (var item in values)
                @this[item.Key] = item.Value;

            return @this;
        }

        public static RouteValueDictionary Merge(this RouteValueDictionary @this, object values)
        {
            return @this.Merge(new RouteValueDictionary(values));
        }

        public static RouteValueDictionary Merge(object values, object otherValues)
        {
            return new RouteValueDictionary(values).Merge(otherValues);
        }

        public static RouteValueDictionary MergeIf(this RouteValueDictionary @this, RouteValueDictionary values, bool condition)
        {
            return condition ? @this.Merge(values) : @this;
        }

        public static RouteValueDictionary MergeIf(this RouteValueDictionary @this, object values, bool condition)
        {
            return @this.MergeIf(new RouteValueDictionary(values), condition);
        }

        public static RouteValueDictionary MergeIf(object values, object otherValues, bool condition)
        {
            return new RouteValueDictionary(values).MergeIf(new RouteValueDictionary(otherValues), condition);
        }
    }
}