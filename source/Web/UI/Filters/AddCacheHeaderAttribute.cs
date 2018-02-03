using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.UI.Filters
{
    // WORKAROUND: using ResponseCacheAttribute there is no way to disable appending cache headers
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AddCacheHeaderAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        class NoopFilter : IPageFilter
        {
            public static readonly NoopFilter Instance = new NoopFilter();

            NoopFilter() { }

            public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
            public void OnPageHandlerExecuting(PageHandlerExecutingContext context) { }
            public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
        }

        private int? _duration;
        private ResponseCacheLocation? _location;
        private bool? _noStore;

        /// <summary>
        /// Gets or sets the duration in seconds for which the response is cached.
        /// This sets "max-age" in "Cache-control" header.
        /// </summary>
        public int Duration
        {
            get => _duration ?? 0;
            set => _duration = value;
        }

        /// <summary>
        /// Gets or sets the location where the data from a particular URL must be cached.
        /// </summary>
        public ResponseCacheLocation Location
        {
            get => _location ?? ResponseCacheLocation.Any;
            set => _location = value;
        }

        /// <summary>
        /// Gets or sets the value which determines whether the data should be stored or not.
        /// When set to <see langword="true"/>, it sets "Cache-control" header to "no-store".
        /// Ignores the "Location" parameter for values other than "None".
        /// Ignores the "duration" parameter.
        /// </summary>
        public bool NoStore
        {
            get => _noStore ?? false;
            set => _noStore = value;
        }

        /// <summary>
        /// Gets or sets the value for the Vary response header.
        /// </summary>
        public string VaryByHeader { get; set; }

        /// <summary>
        /// Gets or sets the query keys to vary by.
        /// </summary>
        /// <remarks>
        /// <see cref="VaryByQueryKeys"/> requires the response cache middleware.
        /// </remarks>
        public string[] VaryByQueryKeys { get; set; }

        /// <summary>
        /// Gets or sets the value of the cache profile name.
        /// </summary>
        public string CacheProfileName { get; set; }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public bool IsReusable => true;

        /// <summary>
        /// Gets the <see cref="CacheProfile"/> for this attribute.
        /// </summary>
        /// <returns></returns>
        public CacheProfile GetCacheProfile(MvcOptions options)
        {
            CacheProfile selectedProfile = null;
            if (CacheProfileName != null)
            {
                options.CacheProfiles.TryGetValue(CacheProfileName, out selectedProfile);
                if (selectedProfile == null)
                    throw new InvalidOperationException("Cache profile not found.");
            }

            _duration = _duration ?? selectedProfile?.Duration;
            _noStore = _noStore ?? selectedProfile?.NoStore;
            _location = _location ?? selectedProfile?.Location;
            VaryByHeader = VaryByHeader ?? selectedProfile?.VaryByHeader;
            VaryByQueryKeys = VaryByQueryKeys ?? selectedProfile?.VaryByQueryKeys;

            return new CacheProfile
            {
                Duration = _duration,
                Location = _location,
                NoStore = _noStore,
                VaryByHeader = VaryByHeader,
                VaryByQueryKeys = VaryByQueryKeys,
            };
        }

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var settings = serviceProvider.GetRequiredService<IOptions<UISettings>>().Value;
            if (!settings.EnableResponseCaching)
                return NoopFilter.Instance;

            var optionsAccessor = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();
            var cacheProfile = GetCacheProfile(optionsAccessor.Value);

            return new ResponseCacheFilter(cacheProfile);
        }
    }
}