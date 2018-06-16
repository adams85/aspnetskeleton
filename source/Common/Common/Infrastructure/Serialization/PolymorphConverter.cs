using System;
using System.Collections.Generic;
using System.Text;
using Karambolo.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspNetSkeleton.Common.Infrastructure.Serialization
{
    class PolymorphConverter : JsonConverter
    {
        const string typePropertyName = "Type";
        const string valuePropertyName = "Value";

        static int GetAssemblyDelimiterIndex(string typeName)
        {
            var level = 0;
            char c;
            for (int i = 0; i < typeName.Length; i++)
                switch (c = typeName[i])
                {
                    case '[':
                        level++;
                        break;
                    case ']':
                        level--;
                        break;
                    case ',':
                        if (level == 0)
                            return i;
                        break;
                }

            return -1;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token;
            if (reader.TokenType == JsonToken.Null)
                return Activator.CreateInstance(objectType, null);
            else if (reader.TokenType == JsonToken.StartObject && (token = JToken.ReadFrom(reader)) is JObject obj)
            {
                var property = obj.Property(typePropertyName);
                var fullTypeName = property != null ? property.Value.ToObject<string>(serializer) : null;

                Type type;
                if (fullTypeName != null)
                {
                    string assemblyName, typeName;
                    var index = GetAssemblyDelimiterIndex(fullTypeName);
                    if (index >= 0)
                    {
                        assemblyName = fullTypeName.Remove(0, index + 1).TrimStart();
                        typeName = fullTypeName.Substring(0, index).TrimEnd();
                    }
                    else
                    {
                        assemblyName = null;
                        typeName = fullTypeName;
                    }

                    type = serializer.SerializationBinder.BindToType(assemblyName, typeName);
                    if (type == null)
                        throw new JsonSerializationException($"Unrecognized type: {fullTypeName}");
                }
                else
                    type = objectType.GetGenericArguments()[0];

                property = obj.Property(valuePropertyName);
                var value = property != null ? property.Value.ToObject(type, serializer) : null;

                return Activator.CreateInstance(objectType, value);
            }
            else
                throw new JsonSerializationException($"Unexpected token or value when parsing {nameof(Polymorph<object>)}. Token: {reader.TokenType}, Value: {reader.Value}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type objectType;
            if (value != null && CanConvert(objectType = value.GetType()))
            {
                value = ((IPolymorphValueAccessor)value).Value;
                if (value != null)
                {
                    var type = value.GetType();
                    var baseType = objectType.GetGenericArguments()[0];

                    var obj = new JObject();

                    serializer.SerializationBinder.BindToName(type, out string assemblyName, out string fullTypeName);
                    if (assemblyName != null)
                        fullTypeName = string.Concat(fullTypeName, ", ", assemblyName);

                    obj.Add(typePropertyName, new JValue(fullTypeName));

                    obj.Add(valuePropertyName, JToken.FromObject(value, serializer));

                    obj.WriteTo(writer);
                }
                else
                    writer.WriteNull();
            }
            else
                throw new JsonSerializationException($"Expected {nameof(Polymorph<object>)} object value");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Polymorph<>);
        }
    }
}
