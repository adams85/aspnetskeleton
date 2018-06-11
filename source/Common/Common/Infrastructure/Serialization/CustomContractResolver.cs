using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using Karambolo.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AspNetSkeleton.Common.Infrastructure.Serialization
{
    public class CustomContractResolver : DefaultContractResolver
    {
        public static bool ExcludeDelegateProperty(JsonProperty property)
        {
            return property.PropertyType.IsDelegate();
        }

        public static bool ExcludeReadOnlyProperty(JsonProperty property)
        {
            return !property.Writable;
        }

        readonly Predicate<JsonProperty>[] _propertyFilters;

        public CustomContractResolver() : this(null) { }

        public CustomContractResolver(Predicate<JsonProperty>[] propertyFilters)
        {
            _propertyFilters = propertyFilters ?? ArrayUtils.Empty<Predicate<JsonProperty>>();
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var result = base.CreateObjectContract(objectType);
            if (result.Converter == null)
            {
                var properties = result.Properties;
                JsonProperty property;
                for (var i = properties.Count - 1; i >= 0; i--)
                {
                    property = properties[i];
                    if (Array.FindIndex(_propertyFilters, f => f(property)) >= 0)
                        properties.RemoveAt(i);
                }
            }
            return result;
        }
    }
}
