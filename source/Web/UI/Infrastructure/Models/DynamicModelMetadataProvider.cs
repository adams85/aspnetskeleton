using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public interface IModelAttributesProvider
    {
        Attribute[] GetAttributes(Type containerType, string propertyName);
    }

    public class DynamicModelMetadataProvider : DefaultModelMetadataProvider
    {
        readonly IModelAttributesProvider _modelAttributesProvider;

        public DynamicModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IModelAttributesProvider modelAttributesProvider)
            : base(detailsProvider)
        {
            _modelAttributesProvider = modelAttributesProvider;
        }

        public DynamicModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor, IModelAttributesProvider modelAttributesProvider)
            : base(detailsProvider, optionsAccessor)
        {
            _modelAttributesProvider = modelAttributesProvider;
        }

        protected override DefaultMetadataDetails[] CreatePropertyDetails(ModelMetadataIdentity key)
        {
            var result = base.CreatePropertyDetails(key);

            var n = result.Length;
            for (var i = 0; i < n; i++)
            {
                var propertyEntry = result[i];

                var dynamicAttributes = _modelAttributesProvider.GetAttributes(propertyEntry.Key.ContainerType, propertyEntry.Key.Name);
                if (dynamicAttributes.Length > 0)
                {
                    var attributes = propertyEntry.ModelAttributes;
                    attributes = new ModelAttributes(attributes.TypeAttributes, attributes.PropertyAttributes.Concat(dynamicAttributes));

                    result[i] = new DefaultMetadataDetails(propertyEntry.Key, attributes)
                    {                        
                        PropertyGetter = propertyEntry.PropertyGetter,
                        PropertySetter = propertyEntry.PropertySetter,
                    };
                }
            }

            return result;
        }
    }
}