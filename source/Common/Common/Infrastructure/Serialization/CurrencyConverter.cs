using System;
using Karambolo.Common.Finances;
using Newtonsoft.Json;

namespace AspNetSkeleton.Common.Infrastructure.Serialization
{
    public class CurrencyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Currency currency)
                writer.WriteValue(currency.Code);
            else
                throw new JsonSerializationException($"Expected {nameof(Currency)} object value");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return Currency.None;
            else if (reader.TokenType == JsonToken.String)
            {
                try
                {
                    return Currency.FromCode((string)reader.Value);
                }
                catch (Exception ex)
                {
                    throw new JsonSerializationException($"Error parsing {nameof(Currency)} string: {reader.Value}", ex);
                }
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token or value when parsing {nameof(Currency)}. Token: {reader.TokenType}, Value: {reader.Value}");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Currency);
        }
    }
}
