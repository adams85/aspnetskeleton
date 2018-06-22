using AspNetSkeleton.UI.Infrastructure.Localization;
using Microsoft.Extensions.Localization;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public interface IModelMetadataConfigurer
    {
        void Configure(IModelMetadataBuilder builder);
    }

    public abstract class ModelMetadataConfigurer : IModelMetadataConfigurer
    {
        // injected by the DI container
        public IStringLocalizer T { get; set; }

        public ModelMetadataConfigurer()
        {
            T = NullStringLocalizer.Instance;
        }

        protected abstract void Configure(IModelMetadataBuilder builder);

        void IModelMetadataConfigurer.Configure(IModelMetadataBuilder builder)
        {
            // during configuration no actual localization is needed,
            // but property T is used to support POTools so that it can extract data annotation texts
            var localizer = T;
            T = NullStringLocalizer.Instance;
            try { Configure(builder); }
            finally { T = localizer; }
        }
    }
}
