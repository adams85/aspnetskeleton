using System;
using Karambolo.Common.Finances;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspNetSkeleton.Common.Infrastructure.Serialization
{
    public class ConversionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else if (value is Conversion conversion)
            {
                var obj = new JObject();

                obj.Add(nameof(Conversion.From), JToken.FromObject(conversion.From, serializer));
                obj.Add(nameof(Conversion.To), JToken.FromObject(conversion.To, serializer));
                obj.Add(nameof(Conversion.Rate), JToken.FromObject(conversion.Rate, serializer));

                obj.WriteTo(writer);
            }
            else
                throw new JsonSerializationException($"Expected {nameof(Conversion)} object value");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token;
            if (reader.TokenType == JsonToken.Null)
                return null;
            else if (reader.TokenType == JsonToken.StartObject && (token = JToken.ReadFrom(reader)) is JObject obj)
            {
                var property = obj.Property(nameof(Conversion.From));
                var from = property != null ? property.Value.ToObject<Currency>(serializer) : Currency.None;

                property = obj.Property(nameof(Conversion.To));
                var to = property != null ? property.Value.ToObject<Currency>(serializer) : Currency.None;

                property = obj.Property(nameof(Conversion.Rate));
                var rate = property != null ? property.Value.ToObject<decimal>(serializer) : 0;

                return new Conversion(from, to, rate);
            }
            else
                throw new JsonSerializationException($"Unexpected token or value when parsing {nameof(Conversion)}. Token: {reader.TokenType}, Value: {reader.Value}");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Conversion);
        }
    }
}
