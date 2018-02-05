using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public class DynamicModelMetadataProvider : CachedDataAnnotationsModelMetadataProvider
    {
        readonly IDynamicModelAttributesProvider _modelAttributesProvider;

        public DynamicModelMetadataProvider(IDynamicModelAttributesProvider modelAttributesProvider)
        {
            _modelAttributesProvider = modelAttributesProvider;
        }

        protected override CachedDataAnnotationsModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName)
        {
            if (containerType != null && propertyName != null)
            {
                var dynamicAttributes = _modelAttributesProvider.GetPropertyAttributes(containerType, propertyName);

                if (dynamicAttributes.Length > 0)
                    attributes = attributes.Concat(dynamicAttributes);
            }

            return base.CreateMetadataPrototype(attributes, containerType, modelType, propertyName);
        }
    }

    public class DynamicModelValidatorProvider : DataAnnotationsModelValidatorProvider
    {
        readonly IDynamicModelAttributesProvider _modelAttributesProvider;

        public DynamicModelValidatorProvider(IDynamicModelAttributesProvider modelAttributesProvider)
        {
            _modelAttributesProvider = modelAttributesProvider;
        }

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
        {
            if (metadata.ContainerType != null && metadata.PropertyName != null)
            {
                var dynamicAttributes = _modelAttributesProvider.GetPropertyAttributes(metadata.ContainerType, metadata.PropertyName);

                if (dynamicAttributes.Length > 0)
                    attributes = attributes.Concat(dynamicAttributes);
            }

            return base.GetValidators(metadata, context, attributes);
        }
    }
}