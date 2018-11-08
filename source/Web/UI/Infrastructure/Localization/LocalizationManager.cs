using System.Globalization;
using System;
using Karambolo.PO;

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
            while (culture != null && !culture.Equals(CultureInfo.InvariantCulture))
                if (Provider.TextCatalogs.TryGetValue(culture.Name, out var catalog))
                    return catalog;
                else
                    culture = culture.Parent;

            return null;
        }

        public override IExtendedStringLocalizer WithCulture(CultureInfo culture)
        {
            return new POStringLocalizer(culture, GetCatalogForCulture) { Logger = Logger };
        }
    }
}