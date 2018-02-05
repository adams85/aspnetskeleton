using Karambolo.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public class XmlModelAttributesOptions
    {
        public IFileProvider FileProvider { get; set; }
        public string[] ModelMetadataFilePaths { get; set; }
    }

    public class XmlModelAttributesProvider : DynamicModelAttributesProvider
    {
        enum WellKnownTypeKind
        {
            Attribute,
            AttributeParam,
        }

        class WellKnownTypeInfo
        {
            public WellKnownTypeInfo(Type baseType, Dictionary<Assembly, string[]> namespaces)
            {
                BaseType = baseType;
                Namespaces = namespaces;
            }

            public Type BaseType { get; }
            public Dictionary<Assembly, string[]> Namespaces { get; }
        }

        static readonly Dictionary<Assembly, string[]> wellKnownAttributeNamespaces = new Dictionary<Assembly, string[]>
        {
            { typeof(DisplayNameAttribute).Assembly, new[] { "System.ComponentModel" } },
            { typeof(RequiredAttribute).Assembly, new[] { "System.ComponentModel.DataAnnotations" } },
        };

        static readonly Dictionary<Assembly, string[]> wellKnownAttributeParamNamespaces = new Dictionary<Assembly, string[]>(wellKnownAttributeNamespaces)
        {
            { typeof(string).Assembly, new[] { "System" } },
        };

        static readonly Dictionary<WellKnownTypeKind, WellKnownTypeInfo> wellKnownTypes = new Dictionary<WellKnownTypeKind, WellKnownTypeInfo>
        {
            { WellKnownTypeKind.Attribute, new WellKnownTypeInfo(typeof(Attribute), wellKnownAttributeNamespaces) },
            { WellKnownTypeKind.AttributeParam, new WellKnownTypeInfo(typeof(object), wellKnownAttributeParamNamespaces) },
        };

        readonly Dictionary<Type, DynamicModelMetadata> _attributeCache = new Dictionary<Type, DynamicModelMetadata>();

        readonly IFileProvider _fileProvider;

        public XmlModelAttributesProvider(IHostingEnvironment hostingEnvironment, IOptions<XmlModelAttributesOptions> options)
        {
            var optionsUnwrapped = options.Value;
            _fileProvider = optionsUnwrapped.FileProvider ?? hostingEnvironment.ContentRootFileProvider;

            if (!ArrayUtils.IsNullOrEmpty(optionsUnwrapped.ModelMetadataFilePaths))
                Array.ForEach(optionsUnwrapped.ModelMetadataFilePaths, LoadModelAttributes);
        }

        static Type GetWellKnownType(string typeName, WellKnownTypeKind kind, Dictionary<string, Type> typeCache)
        {
            var key = string.Concat(kind.ToString(), "|", typeName);

            if (!typeCache.TryGetValue(key, out Type resultLocal))
                typeCache[key] = resultLocal = GetWellKnownTypeCore(wellKnownTypes[kind]);

            return resultLocal;

            Type GetWellKnownTypeCore(WellKnownTypeInfo info)
            {
                foreach (var kvp in info.Namespaces)
                {
                    var assembly = kvp.Key;
                    var namespaces = kvp.Value;
                    Type type;
                    var count = namespaces.Length;
                    for (var i = 0; i < count; i++)
                        if ((type = assembly.GetType(string.Concat(namespaces[i], ".", typeName), throwOnError: false, ignoreCase: true)) != null &&
                            info.BaseType.IsAssignableFrom(type))
                            return type;
                }
                return null;
            }
        }

        static object ConvertFrom(string value, Type type)
        {
            return
                type.IsEnum ? Enum.Parse(type, value) :
                type == typeof(Type) ? Type.GetType(value, throwOnError: true) :
                Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        static Attribute ParseAttribute(XElement attributeElement, XAttribute typeNameAttribute, XAttribute propertyNameAttribute, Dictionary<string, Type> typeCache)
        {
            string attributeTypeHint;
            Type attributeType;
            if (attributeElement.Name == "attribute")
            {
                var attributeTypeHintAttribute = attributeElement.Attribute("type-name");
                if (attributeTypeHintAttribute == null)
                    throw new FormatException("Missing attribute type name in " +
                        propertyNameAttribute != null ?
                        $"property {typeNameAttribute.Value}.{propertyNameAttribute.Value}." :
                        $"type {typeNameAttribute.Value}.");

                attributeTypeHint = attributeTypeHintAttribute.Value;
                attributeType = Type.GetType(attributeTypeHint, throwOnError: false, ignoreCase: true);
            }
            else
            {
                attributeTypeHint = attributeElement.Name.ToString();
                attributeType = GetWellKnownType(attributeTypeHint + "Attribute", WellKnownTypeKind.Attribute, typeCache);
            }

            if (attributeType == null)
                throw new InvalidOperationException($"Invalid attribute type {attributeTypeHint}.");

            object[] ctorArgs;
            if (attributeElement.HasElements)
                ctorArgs = attributeElement.Elements().Select(ctorArgElement =>
                {
                    string ctorArgTypeHint;
                    Type ctorArgType;

                    if (ctorArgElement.Name == "arg")
                    {
                        var ctorArgTypeHintAttribute = ctorArgElement.Attribute("type-name");
                        if (ctorArgTypeHintAttribute == null)
                            throw new FormatException("Missing attribute constructor argument type name in " +
                                propertyNameAttribute != null ?
                                $"property {typeNameAttribute.Value}.{propertyNameAttribute.Value}." :
                                $"type {typeNameAttribute.Value}.");

                        ctorArgTypeHint = ctorArgTypeHintAttribute.Value;
                        ctorArgType = Type.GetType(ctorArgTypeHint, throwOnError: false, ignoreCase: true);
                    }
                    else
                    {
                        ctorArgTypeHint = ctorArgElement.Name.ToString();
                        ctorArgType = GetWellKnownType(ctorArgTypeHint, WellKnownTypeKind.AttributeParam, typeCache);
                    }

                    if (ctorArgType == null)
                        throw new InvalidOperationException($"Invalid attribute constructor argument type {ctorArgTypeHint}.");

                    return ConvertFrom(ctorArgElement.Value, ctorArgType);
                }).ToArray();
            else if (!attributeElement.IsEmpty && !string.IsNullOrEmpty(attributeElement.Value))
                ctorArgs = new[] { (object)attributeElement.Value };
            else
                ctorArgs = ArrayUtils.Empty<object>();

            var attribute = (Attribute)Activator.CreateInstance(attributeType, ctorArgs);
            string attributeAttributeName;
            foreach (var attributeAttribute in attributeElement.Attributes())
                if ((attributeAttributeName = attributeAttribute.Name.ToString()) != "type-name")
                {
                    var property = attributeType.GetProperty(attributeAttributeName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property == null)
                        throw new InvalidOperationException($"Invalid attribute property name {attributeType.Name}.{attributeAttributeName}");

                    var value = ConvertFrom(attributeAttribute.Value, property.PropertyType);
                    property.SetValue(attribute, value);
                }

            return attribute;
        }

        static KeyValuePair<string, List<Attribute>> ParseProperty(XElement propertyElement, XAttribute typeNameAttribute, Dictionary<string, Type> typeCache)
        {
            var propertyNameAttribute = propertyElement.Attribute("name");
            if (propertyNameAttribute == null)
                throw new FormatException($"Missing property name in type {typeNameAttribute.Value}.");

            var attributes = new List<Attribute>();
            foreach (var attributeElement in propertyElement.Elements())
            {
                var attribute = ParseAttribute(attributeElement, typeNameAttribute, propertyNameAttribute, typeCache);
                attributes.Add(attribute);
            }

            return new KeyValuePair<string, List<Attribute>>(propertyNameAttribute.Value, attributes);
        }

        static void CheckElementName(XElement element, string expectedName)
        {
            if (element.Name != expectedName)
                throw new FormatException($"Invalid element name: {element.Name}.");
        }

        void LoadModelAttributes(string modelMetadataFilePath)
        {
            XDocument xml;
            using (var stream = _fileProvider.GetFileInfo(modelMetadataFilePath).CreateReadStream())
                xml = XDocument.Load(stream);

            var root = xml.Root;
            CheckElementName(root, "types");

            var typeCache = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var typeElement in root.Elements("type"))
            {
                var typeNameAttribute = typeElement.Attribute("name");
                if (typeNameAttribute == null)
                    throw new FormatException("Missing type name.");

                var type = Type.GetType(typeNameAttribute.Value, throwOnError: true);

                var baseTypes = new List<Type>();
                foreach (var inheritFromElement in typeElement.Elements("inheritFrom"))
                {
                    var baseTypeHintAttribute = inheritFromElement.Attribute("type-name");
                    if (baseTypeHintAttribute == null)
                        throw new FormatException($"Missing base type name in type {typeNameAttribute.Value}.");

                    var baseType = Type.GetType(baseTypeHintAttribute.Value, throwOnError: true);

                    baseTypes.Add(baseType);
                }

                var typeAttributes = new List<Attribute>();
                foreach (var attributeElement in typeElement.Elements("attribute"))
                {
                    var attribute = ParseAttribute(attributeElement, typeNameAttribute, null, typeCache);
                    typeAttributes.Add(attribute);
                }

                var propertyAttributes = new Dictionary<string, Attribute[]>();
                foreach (var propertyElement in typeElement.Elements("property"))
                {
                    var propertyAttribute = ParseProperty(propertyElement, typeNameAttribute, typeCache);
                    propertyAttributes.Add(propertyAttribute.Key, propertyAttribute.Value.ToArray());
                }

                _attributeCache.Add(type, new DynamicModelMetadata(baseTypes.ToArray(), typeAttributes.ToArray(), propertyAttributes));
            }
        }

        protected override bool TryGetModelMetadata(Type containerType, out DynamicModelMetadata value)
        {
            return _attributeCache.TryGetValue(containerType, out value);
        }
    }
}