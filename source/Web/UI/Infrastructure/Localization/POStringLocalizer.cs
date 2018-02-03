using Karambolo.Common.Localization;
using System.Globalization;
using System;
using Karambolo.PO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSkeleton.UI.Infrastructure.Localization
{
    public abstract class POStringLocalizerBase : NullStringLocalizer
    {
        public ILogger Logger { get; set; }

        protected POStringLocalizerBase()
        {
            Logger = NullLogger.Instance;
        }

        protected abstract POCatalog Catalog { get; }

        bool TryGetTranslationCore(POKey key, int pluralCount, out string value)
        {
            var catalog = Catalog;
            if (catalog != null)
            {
                var translation = catalog.GetTranslation(key, pluralCount);
                if (translation != null)
                {
                    value = translation;
                    return true;
                }
            }

            value = null;
            return false;
        }

        protected override bool TryGetTranslation(string name, Plural plural, TextContext context, out string value)
        {
            var key = new POKey(name, plural.Id, context.Id);
            if (!TryGetTranslationCore(key, plural.Count, out string translation))
            {
                Logger.LogTrace("No translation for key {KEY}.", POStringLocalizer.FormatKey(key));

                base.TryGetTranslation(name, plural, context, out value);
                return false;
            }

            value = translation;
            return true;
        }
    }

    public class POStringLocalizer : POStringLocalizerBase
    {
        public static string FormatKey(POKey key)
        {
            var result = string.Concat("'", key.Id, "'");
            if (key.PluralId != null)
                result = string.Concat(result, "-'", key.PluralId, "'");
            if (key.ContextId != null)
                result = string.Concat(result, "@'", key.ContextId, "'");

            return result;
        }

        readonly POCatalog _catalog;
        readonly Func<CultureInfo, POCatalog> _getCatalogForCulture;

        public POStringLocalizer(CultureInfo culture, Func<CultureInfo, POCatalog> getCatalogForCulture)
        {
            _catalog = getCatalogForCulture(culture);
            _getCatalogForCulture = getCatalogForCulture;
        }

        protected override POCatalog Catalog => _catalog;

        public override IExtendedStringLocalizer WithCulture(CultureInfo culture)
        {
            return new POStringLocalizer(culture, _getCatalogForCulture) { Logger = Logger };
        }
    }
}