using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Karambolo.Common;
using Karambolo.Common.Collections;
using Karambolo.Common.Monetary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AspNetSkeleton.Common.Infrastructure.Serialization
{
    public class CustomSerializationBinder : DefaultSerializationBinder
    {
        static readonly Dictionary<string, string> serializationAssemblyNameMapping = new Dictionary<string, string>(0);
        static readonly Dictionary<string, string> deserializationAssemblyNameMapping = serializationAssemblyNameMapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        static readonly HashSet<Type> basicGenericCollectionTypes = new HashSet<Type>
        {
            typeof(List<>), typeof(Dictionary<,>), typeof(OrderedDictionary<,>), typeof(HashSet<>)
        };

        static readonly HashSet<Type> basicValueTypes = new HashSet<Type>
        {
            typeof(bool), typeof(byte), typeof(sbyte),typeof(char), typeof(decimal), typeof(double), typeof(float),
            typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort),
            typeof(Guid), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Currency), typeof(Money)
        };

        static readonly HashSet<Type> basicReferenceTypes = new HashSet<Type>
        {
            typeof(string), typeof(Uri), typeof(Version), typeof(Conversion)
        };

        public static bool AllowBasicTypes(Type type)
        {
            // collections
            if (type.IsArray || type.IsGenericType && basicGenericCollectionTypes.Contains(type.GetGenericTypeDefinition()))
                return true;

            // value types
            if (type.IsValueType)
            {
                if (type.IsEnum)
                    return true;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return AllowBasicTypes(Nullable.GetUnderlyingType(type));

                return basicValueTypes.Contains(type);
            }

            // reference types
            return basicReferenceTypes.Contains(type);
        }

        readonly Predicate<Type>[] _typeFilters;
        readonly ConcurrentDictionary<Type, bool> _allowedTypeCache;
        readonly ConcurrentDictionary<string, KeyValuePair<string, string>> _typeNameMappingCache;

        public CustomSerializationBinder() : this(null) { }

        public CustomSerializationBinder(Predicate<Type>[] typeFilters)
        {
            _typeFilters = typeFilters ?? ArrayUtils.Empty<Predicate<Type>>();
            _allowedTypeCache = _typeFilters.Length > 0 ? new ConcurrentDictionary<Type, bool>() : null;
            _typeNameMappingCache = new ConcurrentDictionary<string, KeyValuePair<string, string>>();
        }

        protected virtual string MapAssemblyName(string value, IDictionary<string, string> assemblyNameMapping)
        {
            if (value == null)
                return null;

            // stripping assembly details and mapping assembly names
            var builder = new AssemblyNameBuilder(value);
            return !assemblyNameMapping.TryGetValue(builder.Name, out value) ? builder.Name : value;
        }

        protected virtual KeyValuePair<string, string> MapTypeName(string value, IDictionary<string, string> assemblyNameMapping)
        {
            var builder = new TypeNameBuilder(value)
                .Transform(b => b.AssemblyName = MapAssemblyName(b.AssemblyName, assemblyNameMapping));

            return new KeyValuePair<string, string>(builder.AssemblyName, builder.GetFullName());
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            base.BindToName(serializedType, out assemblyName, out typeName);

            if (assemblyName != null)
                typeName = string.Concat(typeName, ", ", assemblyName);

            var kvp = _typeNameMappingCache.GetOrAdd(typeName, tn => MapTypeName(tn, serializationAssemblyNameMapping));
            assemblyName = kvp.Key;
            typeName = kvp.Value;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName != null)
                typeName = string.Concat(typeName, ", ", assemblyName);

            var kvp = _typeNameMappingCache.GetOrAdd(typeName, tn => MapTypeName(tn, deserializationAssemblyNameMapping));
            assemblyName = kvp.Key;
            typeName = kvp.Value;

            var type = base.BindToType(assemblyName, typeName);

            // type whitelisting to avoid type name handling vulnerability of JSON.NET
            // https://stackoverflow.com/questions/49038055/external-json-vulnerable-because-of-json-net-typenamehandling-auto

            return
                type == null || _allowedTypeCache == null || _allowedTypeCache.GetOrAdd(type, t => Array.FindIndex(_typeFilters, f => f(t)) >= 0) ?
                type :
                throw new JsonSerializationException($"Deserialization of type {type.AssemblyQualifiedName} is not allowed.");
        }
    }
}
