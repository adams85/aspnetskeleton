using Karambolo.Common.Localization;
using System.Globalization;
using System.Web;
using System;
using System.Threading;
using Karambolo.PO;
using Karambolo.Common.Logging;
using Karambolo.Common;

namespace AspNetSkeleton.UI.Infrastructure.Localization
{
    public interface IViewLocalizer
    {
        IHtmlString this[string id, params object[] formatArgs] { get; }
    }

    public interface ILocalizationManager : ITextLocalizer, IViewLocalizer
    {
        ILocalizationProvider Provider { get; }
        CultureInfo CurrentCulture { get; set; }
    }

    public class NullLocalizationManager : ILocalizationManager
    {
        public NullLocalizationManager(ILocalizationProvider provider)
        {
            Provider = provider;
        }

        public ILocalizationProvider Provider { get; }

        public CultureInfo CurrentCulture
        {
            get => Thread.CurrentThread.CurrentUICulture;
            set => throw new NotSupportedException();
        }

        public string this[string id, params object[] formatArgs] => NullTextLocalizer.Instance[id, formatArgs];

        IHtmlString IViewLocalizer.this[string id, params object[] formatArgs] => new HtmlString(this[id, formatArgs]);

        public string Localize(ILocalizableText localizableObject)
        {
            return NullTextLocalizer.Instance.Localize(localizableObject);
        }
    }

    public class LocalizationManager : TextLocalizerBase, ILocalizationManager
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

        public ILogger Logger { get; set; }

        public LocalizationManager(ILocalizationProvider provider)
        {
            Logger = NullLogger.Instance;

            Provider = provider;
        }

        public ILocalizationProvider Provider { get; }

        IHtmlString IViewLocalizer.this[string id, params object[] formatArgs] => new HtmlString(this[id, formatArgs]);

        public CultureInfo CurrentCulture
        {
            get => Thread.CurrentThread.CurrentUICulture;
            set => throw new NotSupportedException();
        }

        public override string Localize(ILocalizableText localizableObject)
        {
            if (localizableObject == null)
                throw new ArgumentNullException(nameof(localizableObject));

            if (string.IsNullOrEmpty(localizableObject.Id))
                return string.Empty;

            string translation;
            POKey key;
            if (!Provider.TextCatalogs.TryGetValue(localizableObject.Culture.Name, out POCatalog catalog) ||
                (translation = catalog.GetTranslation(
                    key = new POKey(localizableObject.Id, localizableObject.Plural.Id, localizableObject.Context.Id), 
                    localizableObject.Plural.Count)) == null)
            {
                Logger.LogVerbose($"No translation for key {FormatKey(key)}.");

                return NullTextLocalizer.Instance.Localize(localizableObject);
            }

            return
                ArrayUtils.IsNullOrEmpty(localizableObject.FormatArgs) ?
                translation :
                string.Format(translation, localizableObject.FormatArgs);
        }
    }
}