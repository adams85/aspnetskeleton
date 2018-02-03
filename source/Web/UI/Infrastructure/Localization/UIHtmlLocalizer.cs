using Karambolo.Common;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetSkeleton.UI.Infrastructure.Localization
{
    public class UIHtmlLocalizer : IHtmlLocalizer
    {
        public static readonly UIHtmlLocalizer Null = new UIHtmlLocalizer(NullStringLocalizer.Instance);

        readonly IExtendedStringLocalizer _localizer;

        public UIHtmlLocalizer(IExtendedStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        LocalizedHtmlString Localize(string name, object[] arguments)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            bool notFound;
            if (notFound = !_localizer.TryGetTranslation(name, arguments, out string translation) && translation == null)
                translation = name;

            return
                ArrayUtils.IsNullOrEmpty(arguments) ?
                new LocalizedHtmlString(name, translation, notFound) :
                new LocalizedHtmlString(name, translation, notFound, arguments);
        }

        public LocalizedHtmlString this[string name] => Localize(name, null);

        public LocalizedHtmlString this[string name, params object[] arguments] => Localize(name, arguments);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _localizer.GetAllStrings(includeParentCultures);
        }

        public LocalizedString GetString(string name)
        {
            return _localizer[name];
        }

        public LocalizedString GetString(string name, params object[] arguments)
        {
            return _localizer[name, arguments];
        }

        public IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            return new UIHtmlLocalizer(_localizer.WithCulture(culture));
        }
    }
}
