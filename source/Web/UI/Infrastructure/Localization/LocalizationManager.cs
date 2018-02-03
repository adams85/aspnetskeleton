using System.Globalization;
using System.Web;
using System;
using System.Threading;
using Karambolo.Common;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using AspNetSkeleton.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Localization;
using Karambolo.Common.Localization;
using Karambolo.PO;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.UI.Infrastructure.Localization
{
    public interface ILocalizationManager : IExtendedStringLocalizer
    {
        ILocalizationProvider Provider { get; }
        CultureInfo CurrentCulture { get; set; }
    }

    public class NullLocalizationManager : NullStringLocalizer, ILocalizationManager
    {
        public NullLocalizationManager(ILocalizationProvider provider)
        {
            Provider = provider;
        }

        public ILocalizationProvider Provider { get; }

        public CultureInfo CurrentCulture
        {
            get => CultureInfo.CurrentUICulture;
            set => throw new NotSupportedException();
        }
    }

    public class LocalizationManager : POStringLocalizerBase, ILocalizationManager
    {
        public LocalizationManager(ILocalizationProvider provider)
        {
            Provider = provider;
        }

        public ILocalizationProvider Provider { get; }

        protected override POCatalog Catalog => GetCatalogForCulture(CurrentCulture);

        public CultureInfo CurrentCulture
        {
            get => CultureInfo.CurrentUICulture;
            set => throw new NotSupportedException();
        }

        POCatalog GetCatalogForCulture(CultureInfo culture)
        {
            return Provider.TextCatalogs.TryGetValue((culture ?? CultureInfo.InvariantCulture).Name, out POCatalog catalog) ? catalog : null;
        }

        public override IExtendedStringLocalizer WithCulture(CultureInfo culture)
        {
            return new POStringLocalizer(culture, GetCatalogForCulture) { Logger = Logger };
        }
    }
}