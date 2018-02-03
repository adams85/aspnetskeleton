using Karambolo.Common;
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
    public class XmlModelAttributesProvider : IModelAttributesProvider
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

        readonly Dictionary<Type, Dictionary<string, Attribute[]>> _attributeCache = new Dictionary<Type, Dictionary<string, Attribute[]>>();

        public XmlModelAttributesProvider(string[] modelMetadataFilePaths)
        {
            if (modelMetadataFilePaths == null)
                throw new ArgumentNullException(nameof(modelMetadataFilePaths));

            Array.ForEach(modelMetadataFilePaths, LoadModelAttributes);
        }

        void LoadModelAttributes(string modelMetadataFilePath)
        {
            var xml = XDocument.Load(modelMetadataFilePath);

            var root = xml.Root;
            CheckElementName(root, "types");

            var typeCache = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var typeElement in root.Elements("type"))
            {
                var typeNameAttribute = typeElement.Attribute("name");
                if (typeNameAttribute == null)
                    throw new FormatException("Missing type name.");

                var type = Type.GetType(typeNameAttribute.Value, throwOnError: true);

                var propertyAttributes = new Dictionary<string, Attribute[]>();
                Dictionary<string, Attribute[]> inheritedPropertyAttributes;

                var inheritFromTypeNameAttribute = typeElement.Attribute("inheritFrom");
                if (inheritFromTypeNameAttribute != null)
                {
                    var inheritFromType = Type.GetType(inheritFromTypeNameAttribute.Value, throwOnError: true);

                    if (!_attributeCache.TryGetValue(inheritFromType, out inheritedPropertyAttributes))
                        throw new InvalidOperationException($"Undefined type {inheritFromTypeNameAttribute.Value}.");
                }
                else
                    inheritedPropertyAttributes = null;

                foreach (var propertyElement in typeElement.Elements("property"))
                {
                    var propertyNameAttribute = propertyElement.Attribute("name");
                    if (propertyNameAttribute == null)
                        throw new FormatException($"Missing property name in type {typeNameAttribute.Value}.");

                    var attributes = new Dictionary<Type, Attribute>();

                    foreach (var attributeElement in propertyElement.Elements())
                    {
                        string attributeTypeHint;
                        Type attributeType;

                        if (attributeElement.Name == "attribute")
                        {
                            var attributeTypeHintAttribute = attributeElement.Attribute("type-name");
                            if (attributeTypeHintAttribute == null)
                                throw new FormatException($"Missing attribute type name in property {typeNameAttribute.Value}.{propertyNameAttribute.Value}.");

                            attributeTypeHint = attributeTypeHintAttribute.Value;
                            attributeType = Type.GetType(attributeTypeHint, throwOnError: false, ignoreCase: true);
                        }
                        else
                        {
                            attributeTypeHint = attributeElement.Name.ToString();
                            attributeType = GetWellKnownType(attributeTypeHint + "Attribute", WellKnownTypeKind.Attribute);
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
                                        throw new FormatException($"Missing attribute constructor argument type name in property {typeNameAttribute.Value}.{propertyNameAttribute.Value}.");

                                    ctorArgTypeHint = ctorArgTypeHintAttribute.Value;
                                    ctorArgType = Type.GetType(ctorArgTypeHint, throwOnError: false, ignoreCase: true);
                                }
                                else
                                {
                                    ctorArgTypeHint = ctorArgElement.Name.ToString();
                                    ctorArgType = GetWellKnownType(ctorArgTypeHint, WellKnownTypeKind.AttributeParam);
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

                        attributes.Add(attributeType, attribute);
                    }

                    if (attributes.Count > 0)
                    {
                        if (inheritedPropertyAttributes != null && inheritedPropertyAttributes.TryGetValue(propertyNameAttribute.Value, out Attribute[] inheritedAttributes))
                        {
                            Attribute inheritedAttribute;
                            Type inheritedAttributeType;
                            var count = inheritedAttributes.Length;
                            for (var i = 0; i < count; i++)
                                if (!attributes.ContainsKey(inheritedAttributeType = (inheritedAttribute = inheritedAttributes[i]).GetType()))
                                    attributes.Add(inheritedAttributeType, inheritedAttribute);
                        }

                        propertyAttributes.Add(propertyNameAttribute.Value, attributes.Values.ToArray());
                    }
                }

                if (inheritedPropertyAttributes != null)
                    foreach (var inheritedProperty in inheritedPropertyAttributes)
                        if (!propertyAttributes.ContainsKey(inheritedProperty.Key))
                            propertyAttributes.Add(inheritedProperty.Key, inheritedProperty.Value);

                if (propertyAttributes.Count > 0)
                    _attributeCache.Add(type, propertyAttributes);
            }

            #region Local methods
            void CheckElementName(XElement element, string expectedName)
            {
                if (element.Name != expectedName)
                    throw new FormatException($"Invalid element name: {element.Name}.");
            }

            Type GetWellKnownType(string typeName, WellKnownTypeKind kind)
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

            object ConvertFrom(string value, Type type)
            {
                return
                    type.IsEnum ? Enum.Parse(type, value) :
                    type == typeof(Type) ? Type.GetType(value, throwOnError: true) :
                    Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            #endregion
        }

        public Attribute[] GetAttributes(Type containerType, string propertyName)
        {
            return
                _attributeCache.TryGetValue(containerType, out Dictionary<string, Attribute[]> propertyAttributes) &&
                propertyAttributes.TryGetValue(propertyName, out Attribute[] result) ?
                result :
                ArrayUtils.Empty<Attribute>();
        }
    }
}