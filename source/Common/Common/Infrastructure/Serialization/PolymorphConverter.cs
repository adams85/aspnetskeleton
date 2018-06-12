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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token;
            if (reader.TokenType == JsonToken.Null)
                return Activator.CreateInstance(objectType, null);
            else if (reader.TokenType == JsonToken.StartObject && (token = JToken.ReadFrom(reader)) is JObject obj)
            {
                var property = obj.Property(typePropertyName);
                var typeName = property != null ? property.Value.ToObject<string>(serializer) : null;

                Type type;
                if (typeName != null)
                {
                    var builder = new TypeNameBuilder(typeName);
                    type = serializer.SerializationBinder.BindToType(builder.AssemblyName, builder.GetFullName());
                    if (type == null)
                        throw new JsonSerializationException($"Unrecognized type: {typeName}");
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

                    serializer.SerializationBinder.BindToName(type, out string assemblyName, out string typeName);
                    if (assemblyName != null)
                        typeName = string.Concat(typeName, ", ", assemblyName);

                    obj.Add(typePropertyName, new JValue(typeName));

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
