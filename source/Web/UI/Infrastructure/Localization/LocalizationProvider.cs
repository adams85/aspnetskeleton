using AspNetSkeleton.Core.Infrastructure;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Karambolo.PO;
using Karambolo.Common;
using System.Threading.Tasks;
using Karambolo.Common.Logging;
using Autofac;
using System;

namespace AspNetSkeleton.UI.Infrastructure.Localization
{
    public interface ILocalizationProvider
    {
        string[] Cultures { get; }
        IReadOnlyDictionary<string, POCatalog> TextCatalogs { get; }
    }

    public class NullLocalizationProvider : ILocalizationProvider
    {
        public string[] Cultures { get; } = ArrayUtils.FromElement(UIConstants.DefaultCulture.Name);

        public IReadOnlyDictionary<string, POCatalog> TextCatalogs { get; } = new Dictionary<string, POCatalog>();
    }

    public class LocalizationProvider : ILocalizationProvider, IStartable
    {
        struct CatalogInfo
        {
            public string FileName;
            public string Culture;
            public POCatalog Catalog;
        }

        public const string BaseUrl = "~/App_Data/Localization";

        public ILogger Logger { get; set; }

        readonly IEnvironment _environment;

        public LocalizationProvider(IEnvironment environment)
        {
            Logger = NullLogger.Instance;

            _environment = environment;
        }

        public string[] Cultures { get; private set; }

        public IReadOnlyDictionary<string, POCatalog> TextCatalogs { get; private set; }

        public void Start()
        {
            var cultures = Directory.EnumerateDirectories(_environment.MapPath(BaseUrl), "*", SearchOption.TopDirectoryOnly)
                .Select(p => Path.GetFileName(p))
                .ToArray();

            var textCatalogFiles = cultures.SelectMany(
                c => Directory.EnumerateFiles(Path.Combine(_environment.MapPath(BaseUrl), c), "*.po", SearchOption.TopDirectoryOnly),
                (c, p) => new KeyValuePair<string, string>(c, p));

            var textCatalogs = new List<CatalogInfo>();

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
                    using (var reader = new StreamReader(it.Value))
                        result = p.Parse(reader);

                    if (result.Success)
                    {
                        lock (textCatalogs)
                            textCatalogs.Add(new CatalogInfo { FileName = it.Value, Culture = it.Key, Catalog = result.Catalog });
                    }
                    else
                        Logger.LogWarning($"Translation file \"{it.Value}\" has errors.");

                    return p;
                },
                Noop<POParser>.Action);

            Cultures = cultures;
            TextCatalogs = textCatalogs
                .GroupBy(it => it.Culture, Identity<CatalogInfo>.Func)
                .ToDictionary(g => g.Key, g =>
                {
                    var catalogs = g.OrderBy(it => it.FileName).Select(it => it.Catalog).ToArray();
                    return catalogs.Skip(1).Aggregate(catalogs[0], (acc, src) =>
                    {
                        foreach (var entry in src)
                            try { acc.Add(entry); }
                            catch (ArgumentException) { Logger.LogWarning("Multiple translations for key {KEY}.", LocalizationManager.FormatKey(entry.Key)); }

                        return acc;
                    });
                });
        }
    }
}