using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetSkeleton.UI.Middlewares
{
    public class DefaultCultureMiddleware
    {
        readonly RequestDelegate _next;
        readonly CultureInfo _culture;

        public DefaultCultureMiddleware(RequestDelegate next, CultureInfo culture)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (culture == null)
                throw new ArgumentNullException(nameof(culture));

            _next = next;
            _culture = culture;
        }

        public Task Invoke(HttpContext context)
        {
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _culture;

            return _next(context);
        }
    }
}
