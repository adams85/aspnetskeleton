using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public interface IModelAttributesProvider
    {
        Attribute[] GetAttributes(Type containerType, string propertyName);
    }

    public class DynamicModelMetadataProvider : CachedDataAnnotationsModelMetadataProvider
    {
        readonly IModelAttributesProvider _modelAttributesProvider;

        public DynamicModelMetadataProvider(IModelAttributesProvider modelAttributesProvider)
        {
            _modelAttributesProvider = modelAttributesProvider;
        }

        protected override CachedDataAnnotationsModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName)
        {
            if (containerType != null && propertyName != null)
            {
                var dynamicAttributes = _modelAttributesProvider.GetAttributes(containerType, propertyName);

                if (dynamicAttributes.Length > 0)
                    attributes = attributes.Concat(dynamicAttributes);
            }

            return base.CreateMetadataPrototype(attributes, containerType, modelType, propertyName);
        }
    }

    public class DynamicModelValidatorProvider : DataAnnotationsModelValidatorProvider
    {
        readonly IModelAttributesProvider _modelAttributesProvider;

        public DynamicModelValidatorProvider(IModelAttributesProvider modelAttributesProvider)
        {
            _modelAttributesProvider = modelAttributesProvider;
        }

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
        {
            if (metadata.ContainerType != null && metadata.PropertyName != null)
            {
                var dynamicAttributes = _modelAttributesProvider.GetAttributes(metadata.ContainerType, metadata.PropertyName);

                if (dynamicAttributes.Length > 0)
                    attributes = attributes.Concat(dynamicAttributes);
            }

            return base.GetValidators(metadata, context, attributes);
        }
    }
}