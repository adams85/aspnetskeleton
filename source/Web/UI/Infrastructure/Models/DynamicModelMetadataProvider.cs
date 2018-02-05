using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public class DynamicModelMetadataProvider : DefaultModelMetadataProvider
    {
        readonly IDynamicModelAttributesProvider _modelAttributesProvider;

        public DynamicModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IDynamicModelAttributesProvider modelAttributesProvider)
            : base(detailsProvider)
        {
            _modelAttributesProvider = modelAttributesProvider;
        }

        public DynamicModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor, IDynamicModelAttributesProvider modelAttributesProvider)
            : base(detailsProvider, optionsAccessor)
        {
            _modelAttributesProvider = modelAttributesProvider;
        }

        protected override DefaultMetadataDetails CreateTypeDetails(ModelMetadataIdentity key)
        {
            var result = base.CreateTypeDetails(key);

            Attribute[] dynamicAttributes;
            if (_modelAttributesProvider != null && // method is called from base constructor...
                (dynamicAttributes = _modelAttributesProvider.GetTypeAttributes(result.Key.ModelType)).Length > 0)
            {
                var attributes = result.ModelAttributes;
                attributes = new ModelAttributes(attributes.TypeAttributes.Concat(dynamicAttributes));

                result = new DefaultMetadataDetails(result.Key, attributes);
            }

            return result;
        }

        protected override DefaultMetadataDetails[] CreatePropertyDetails(ModelMetadataIdentity key)
        {
            var result = base.CreatePropertyDetails(key);

            var n = result.Length;
            for (var i = 0; i < n; i++)
            {
                var propertyEntry = result[i];

                var dynamicAttributes = _modelAttributesProvider.GetPropertyAttributes(propertyEntry.Key.ContainerType, propertyEntry.Key.Name);
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