using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public abstract class DynamicModelMetadataProvider : IMetadataDetailsProvider, IBindingMetadataProvider, IDisplayMetadataProvider, IValidationMetadataProvider
    {
        protected DynamicModelMetadataProvider() { }

        protected abstract IList<Action<BindingMetadata>> GetBindingMetadataSetters(ModelMetadataIdentity key);
        protected abstract IList<Action<DisplayMetadata>> GetDisplayMetadataSetters(ModelMetadataIdentity key);
        protected abstract IList<Action<ValidationMetadata>> GetValidationMetadataSetters(ModelMetadataIdentity key);

        public void CreateBindingMetadata(BindingMetadataProviderContext context)
        {
            var setters = GetBindingMetadataSetters(context.Key);
            for (int i = 0, n = setters.Count; i < n; i++)
                setters[i](context.BindingMetadata);
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var setters = GetDisplayMetadataSetters(context.Key);
            for (int i = 0, n = setters.Count; i < n; i++)
                setters[i](context.DisplayMetadata);
        }

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            var setters = GetValidationMetadataSetters(context.Key);
            for (int i = 0, n = setters.Count; i < n; i++)
                setters[i](context.ValidationMetadata);
        }
    }
}