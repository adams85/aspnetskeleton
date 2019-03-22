using System;
using Karambolo.Common.Monetary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspNetSkeleton.Common.Infrastructure.Serialization
{
    public class MoneyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Money money)
            {
                var obj = new JObject();

                obj.Add(nameof(Money.Currency), JToken.FromObject(money.Currency, serializer));
                obj.Add(nameof(Money.Amount), JToken.FromObject(money.Amount, serializer));

                obj.WriteTo(writer);
            }
            else
                throw new JsonSerializationException($"Expected {nameof(Money)} object value");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token;
            if (reader.TokenType == JsonToken.StartObject && (token = JToken.ReadFrom(reader)) is JObject obj)
            {
                var property = obj.Property(nameof(Money.Currency));
                var currency = property != null ? property.Value.ToObject<Currency>(serializer) : Currency.None;

                property = obj.Property(nameof(Money.Amount));
                var amount = property != null ? property.Value.ToObject<decimal>(serializer) : 0;

                return new Money(amount, currency);
            }
            else
                throw new JsonSerializationException($"Unexpected token or value when parsing {nameof(Money)}. Token: {reader.TokenType}, Value: {reader.Value}");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Money);
        }
    }
}
