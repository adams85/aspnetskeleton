using Karambolo.Common;
using System;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public class NullModelAttributesProvider : IModelAttributesProvider
    {
        public static readonly NullModelAttributesProvider Instance = new NullModelAttributesProvider();

        NullModelAttributesProvider() { }

        public Attribute[] GetAttributes(Type containerType, string propertyName)
        {
            return ArrayUtils.Empty<Attribute>();
        }
    }
}