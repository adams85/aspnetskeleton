using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSkeleton.Core.Middlewares
{
    public class PathAdjustment
    {
        public string Prefix { get; set; }
        public bool Add { get; set; }
    }

    public class PathAdjusterMiddleware
    {
        readonly RequestDelegate _next;
        readonly PathString _prefix;
        readonly Action<HttpRequest> _adjuster;

        public ILogger Logger { get; set; }

        public PathAdjusterMiddleware(RequestDelegate next, PathAdjustment adjustment)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (adjustment == null)
                throw new ArgumentNullException(nameof(adjustment));

            Logger = NullLogger.Instance;

            _next = next;
            _prefix = adjustment.Prefix;
            _adjuster = adjustment.Add ? AddPrefix : new Action<HttpRequest>(RemovePrefix);
        }

        void AddPrefix(HttpRequest request)
        {
            request.PathBase = _prefix + request.PathBase;
        }

        void RemovePrefix(HttpRequest request)
        {
            if (request.PathBase.HasValue)
            {
                if (request.PathBase.StartsWithSegments(_prefix, out PathString remaining))
                {
                    request.PathBase = remaining;
                    return;
                }
            }
            else if (request.Path.StartsWithSegments(_prefix, out PathString remaining))
            {
                request.Path = remaining;
                return;
            }

            Logger.LogWarning("Path does not start with prefix {PREFIX}.", _prefix);
        }

        public Task Invoke(HttpContext context)
        {
            _adjuster(context.Request);
            return _next(context);
        }
    }
}
