using System.IO;
using System.Linq;
using System.Collections.Generic;
using Karambolo.PO;
using Karambolo.Common;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using AspNetSkeleton.Core;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using AspNetSkeleton.UI.Helpers;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.UI.Infrastructure.Localization
{
    public interface ILocalizationProvider : IRequestCultureProvider
    {
        string[] Cultures { get; }
        IReadOnlyDictionary<string, POCatalog> TextCatalogs { get; }
    }

    public class NullLocalizationProvider : ILocalizationProvider
    {
        readonly Task<ProviderCultureResult> _cachedDetermineResult;

        public NullLocalizationProvider(IOptions<UISettings> settings)
        {
            Cultures = new[] { settings.Value.DefaultCulture };
            _cachedDetermineResult = Task.FromResult(new ProviderCultureResult(settings.Value.DefaultCulture));
        }

        public string[] Cultures { get; }

        public IReadOnlyDictionary<string, POCatalog> TextCatalogs { get; } = new Dictionary<string, POCatalog>();

        public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            return _cachedDetermineResult;
        }
    }

    public class LocalizationProvider : ILocalizationProvider, IAppBranchInitializer
    {
        public const string BasePath = "/App_Data/Localization";

        public ILogger Logger { get; set; }

        readonly IHostingEnvironment _env;

        public LocalizationProvider(IHostingEnvironment env)
        {
            Logger = NullLogger.Instance;

            _env = env;
        }

        public string[] Cultures { get; private set; }

        public IReadOnlyDictionary<string, POCatalog> TextCatalogs { get; private set; }

        public void Initialize()
        {
            var cultures = _env.ContentRootFileProvider.GetDirectoryContents(BasePath)
                .Where(fi => fi.IsDirectory)
                .Select(fi => fi.Name)
                .ToArray();

            var textCatalogFiles = cultures.SelectMany(
                c => _env.ContentRootFileProvider.GetDirectoryContents(Path.Combine(BasePath, c))
                    .Where(fi => !fi.IsDirectory && ".po".Equals(Path.GetExtension(fi.Name), StringComparison.OrdinalIgnoreCase)),
                (c, f) => (Culture: c, FileInfo: f));

            var textCatalogs = new List<(string FileName, string Culture, POCatalog Catalog)>();

            var parserSettings = new POParserSettings
            {
                SkipComments = true,
                SkipInfoHeaders = true,
            };

            Parallel.ForEach(textCatalogFiles,
                () => new POParser(parserSettings),
                (it, s, p) =>
                {
                    POParseResult result;
                    using (var stream = it.FileInfo.CreateReadStream())
                        result = p.Parse(new StreamReader(stream));

                    if (result.Success)
                    {
                        lock (textCatalogs)
                            textCatalogs.Add((it.FileInfo.Name, it.Culture, result.Catalog));
                    }
                    else
                        Logger.LogWarning("Translation file \"{FILE}\" has errors.", Path.Combine(BasePath, it.Culture, it.FileInfo.Name));

                    return p;
                },
                Noop<POParser>.Action);

            Cultures = cultures;

            TextCatalogs = textCatalogs
                .GroupBy(it => it.Culture, it => (it.FileName, it.Catalog))
                .ToDictionary(g => g.Key, g => g
                    .OrderBy(it => it.FileName)
                    .Select(it => it.Catalog)
                    .Aggregate((acc, src) =>
                    {
                        foreach (var entry in src)
                            try { acc.Add(entry); }
                            catch (ArgumentException) { Logger.LogWarning("Multiple translations for key {KEY}.", POStringLocalizer.FormatKey(entry.Key)); }

                        return acc;
                    }));
        }

        public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var prefix = UrlUtils.GetPrefix(httpContext.Request.Path);

            StringSegment culture;
            if (!prefix.HasValue || Array.IndexOf(Cultures, (culture = prefix.Substring(1)).ToString()) < 0)
                return Task.FromResult<ProviderCultureResult>(null);

            httpContext.Request.Path = new StringSegment(prefix.Buffer).Substring(prefix.Length);
            httpContext.Request.PathBase += prefix;

            return Task.FromResult(new ProviderCultureResult(culture));
        }
    }
}