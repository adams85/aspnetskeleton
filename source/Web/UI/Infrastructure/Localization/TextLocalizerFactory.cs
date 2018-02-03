using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;

namespace AspNetSkeleton.UI.Infrastructure.Localization
{
    public class NullTextLocalizerFactory : IStringLocalizerFactory, IHtmlLocalizerFactory
    {
        IStringLocalizer IStringLocalizerFactory.Create(Type resourceSource)
        {
            return NullStringLocalizer.Instance;
        }

        IStringLocalizer IStringLocalizerFactory.Create(string baseName, string location)
        {
            return NullStringLocalizer.Instance;
        }

        IHtmlLocalizer IHtmlLocalizerFactory.Create(Type resourceSource)
        {
            return UIHtmlLocalizer.Null;
        }

        IHtmlLocalizer IHtmlLocalizerFactory.Create(string baseName, string location)
        {
            return UIHtmlLocalizer.Null;
        }
    }

    public class TextLocalizerFactory : IStringLocalizerFactory, IHtmlLocalizerFactory
    {
        readonly ILocalizationManager _localizationManager;

        public TextLocalizerFactory(ILocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
        }

        IStringLocalizer IStringLocalizerFactory.Create(Type resourceSource)
        {
            return _localizationManager;
        }

        IStringLocalizer IStringLocalizerFactory.Create(string baseName, string location)
        {
            return _localizationManager;
        }

        IHtmlLocalizer IHtmlLocalizerFactory.Create(Type resourceSource)
        {
            return new UIHtmlLocalizer(_localizationManager);
        }

        IHtmlLocalizer IHtmlLocalizerFactory.Create(string baseName, string location)
        {
            return new UIHtmlLocalizer(_localizationManager);
        }
    }
}
