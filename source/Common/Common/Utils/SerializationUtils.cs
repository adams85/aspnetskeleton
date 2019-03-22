using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Karambolo.Common;
using AspNetSkeleton.Common.Infrastructure.Serialization;
using Newtonsoft.Json.Converters;

namespace AspNetSkeleton.Common.Utils
{
    public static class SerializationUtils
    {
        public static void ConfigureDataTransferSerializerSettings(JsonSerializerSettings settings, IEnumerable<Predicate<Type>> typeFilters = null)
        {
            // built-in type name handling is disabled, Polymorph<T> is used as a workaround

            settings.DefaultValueHandling = DefaultValueHandling.Ignore;

            settings.SerializationBinder = new CustomSerializationBinder(
                Enumerable.Prepend(typeFilters ?? Enumerable.Empty<Predicate<Type>>(), CustomSerializationBinder.AllowBasicTypes).ToArray());

            settings.ContractResolver = new CustomContractResolver(new Predicate<JsonProperty>[]
            {
                CustomContractResolver.ExcludeDelegateProperty
            });

            settings.Converters.Add(new PolymorphConverter());
            settings.Converters.Add(new VersionConverter());
            settings.Converters.Add(new CurrencyConverter());
            settings.Converters.Add(new MoneyConverter());
            settings.Converters.Add(new ConversionConverter());
        }

        public static JsonSerializerSettings CreateDataTransferSerializerSettings(IEnumerable<Predicate<Type>> typeFilters = null)
        {
            var settings = new JsonSerializerSettings();
            ConfigureDataTransferSerializerSettings(settings, typeFilters);
            return settings;
        }

        public static readonly JsonSerializer DataPersistenceSerializer = new JsonSerializer
        {
            SerializationBinder = new CustomSerializationBinder(),
            ContractResolver = new CustomContractResolver(new Predicate<JsonProperty>[]
            {
                CustomContractResolver.ExcludeDelegateProperty,
                CustomContractResolver.ExcludeReadOnlyProperty
            }),
            Converters =
            {
                new PolymorphConverter(),
                new VersionConverter(),
                new CurrencyConverter(),
                new MoneyConverter(),
                new ConversionConverter()
            }
        };

        public static string SerializeArray<T>(T[] value, Func<T, string> elementSerializer)
        {
            return SerializeArray<T>(value, elementSerializer, ',', space: true);
        }

        public static string SerializeArray<T>(T[] value, Func<T, string> elementSerializer, char separator, bool space = false)
        {
            if (ArrayUtils.IsNullOrEmpty(value))
                return string.Empty;

            try { return string.Join(separator + (space ? " " : string.Empty), value.Select(it => elementSerializer(it).Escape(separator, separator))); }
            catch { return string.Empty; }
        }

        public static T[] DeserializeArray<T>(string value, Func<string, T> elementDeserializer)
        {
            return DeserializeArray<T>(value, elementDeserializer, ',');         
        }

        public static T[] DeserializeArray<T>(string value, Func<string, T> elementDeserializer, char separator)
        {
            if (string.IsNullOrEmpty(value))
                return ArrayUtils.Empty<T>();

            try { return value.SplitEscaped(separator, separator).Select(elementDeserializer).ToArray(); }
            catch { return null; }
        }

        public static string SerializeObject(object obj, JsonSerializer serializer = null)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                (serializer ?? DataPersistenceSerializer).Serialize(writer, obj);
                writer.Flush();

                return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public static void PopulateObject(object obj, string value, JsonSerializer serializer = null)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            using (var reader = new StreamReader(stream))
            {
                (serializer ?? DataPersistenceSerializer).Populate(reader, obj);
            }
        }

        public static T DeserializeObject<T>(string value, JsonSerializer serializer = null)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            using (var reader = new StreamReader(stream))
            {
                return (T)(serializer ?? DataPersistenceSerializer).Deserialize(reader, typeof(T));
            }
        }

        public static byte[] HashObject(object obj, JsonSerializer serializer = null, Func<HashAlgorithm> hasherFactory = null)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                (serializer ?? DataPersistenceSerializer).Serialize(writer, obj);
                writer.Flush();

                stream.Position = 0;
                using (var hasher = (hasherFactory ?? SHA256.Create)())
                    return hasher.ComputeHash(stream);
            }
        }
    }
}
