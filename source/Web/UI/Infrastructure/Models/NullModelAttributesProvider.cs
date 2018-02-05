using Karambolo.Common;
using System;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public class NullModelAttributesProvider : IDynamicModelAttributesProvider
    {
        public static readonly NullModelAttributesProvider Instance = new NullModelAttributesProvider();

        NullModelAttributesProvider() { }

        public Attribute[] GetTypeAttributes(Type containerType)
        {
            return ArrayUtils.Empty<Attribute>();
        }

        public Attribute[] GetPropertyAttributes(Type containerType, string propertyName)
        {
            return ArrayUtils.Empty<Attribute>();
        }
    }
}