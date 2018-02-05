using Karambolo.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public interface IDynamicModelAttributesProvider
    {
        Attribute[] GetPropertyAttributes(Type containerType, string propertyName);
    }

    public struct DynamicModelMetadata
    {
        public DynamicModelMetadata(Type[] baseTypes, IReadOnlyDictionary<string, Attribute[]> propertyAttributes)
        {
            BaseTypes = baseTypes;
            PropertyAttributes = propertyAttributes;
        }

        public Type[] BaseTypes { get; }
        public IReadOnlyDictionary<string, Attribute[]> PropertyAttributes { get; }
    }

    public abstract class DynamicModelAttributesProvider : IDynamicModelAttributesProvider
    {
        protected abstract bool TryGetModelMetadata(Type containerType, out DynamicModelMetadata value);

        DynamicModelMetadata GetBaseModelMetadata(Type baseType)
        {
            if (!TryGetModelMetadata(baseType, out var baseModelMetadata))
                throw new InvalidOperationException($"Undefined type {baseType.FullName}.");

            return baseModelMetadata;
        }

        public Attribute[] GetPropertyAttributes(Type containerType, string propertyName)
        {
            if (!TryGetModelMetadata(containerType, out var modelMetadata))
                return ArrayUtils.Empty<Attribute>();

            if (!modelMetadata.PropertyAttributes.TryGetValue(propertyName, out var attributes))
                attributes = ArrayUtils.Empty<Attribute>();

            var n = modelMetadata.BaseTypes.Length;
            if (n == 0)
                return attributes;

            IEnumerable<Attribute> result = attributes;
            for (var i = 0; i < n; i++)
            {
                var baseModelMetadata = GetBaseModelMetadata(modelMetadata.BaseTypes[i]);

                if (baseModelMetadata.PropertyAttributes.TryGetValue(propertyName, out var baseAttributes) &&
                    baseAttributes.Length > 0)
                    result = result.Concat(baseAttributes);
            }

            return result.ToArray();
        }
    }
}