using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSkeleton.Core.Middlewares
{
    public class PathAdjustment
    {
        public string OriginalPrefix { get; set; }
        public string NewPrefix { get; set; }
    }

    public class PathAdjusterOptions
    {
        public IEnumerable<PathAdjustment> Adjustments { get; set; }
    }

    public class PathAdjusterMiddleware
    {
        static bool AdjustPath(HttpRequest request, PathString originalPrefix, PathString newPrefix)
        {
            bool match;
            if (originalPrefix.HasValue)
            {
                if (request.PathBase.HasValue)
                {
                    match = request.PathBase.StartsWithSegments(originalPrefix, out PathString remaining);
                    if (match)
                        request.PathBase = remaining;
                }
                else
                {
                    match = request.Path.StartsWithSegments(originalPrefix, out PathString remaining);
                    if (match)
                        request.Path = remaining;
                }
            }
            else
                match = true;

            if (match && newPrefix.HasValue)
                request.PathBase = newPrefix + request.PathBase;

            return match;
        }

        readonly RequestDelegate _next;
        readonly Func<HttpRequest, bool>[] _adjusters;

        public ILogger Logger { get; set; }

        public PathAdjusterMiddleware(RequestDelegate next, PathAdjusterOptions options)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            Logger = NullLogger.Instance;

            _next = next;

            _adjusters = (options?.Adjustments ?? Enumerable.Empty<PathAdjustment>())
                .Select(adj => new Func<HttpRequest, bool>((req) => AdjustPath(req, adj.OriginalPrefix, adj.NewPrefix)))
                .ToArray();
        }

        public Task Invoke(HttpContext context)
        {
            var n = _adjusters.Length;
            for (var i = 0; i < n; i++)
                if (_adjusters[i](context.Request))
                    break;

            return _next(context);
        }
    }
}
