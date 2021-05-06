using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Sels.Core.Components.Serialization;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;

namespace Sels.Core.Extensions.Conversion
{
    public static class SerializationExtensions
    {
        public static string Serialize<T>(this T value, SerializationProvider provider)
        {
            switch (provider)
            {
                case SerializationProvider.Json:
                    return value.SerializeAsJson();
                case SerializationProvider.Bson:
                    return value.SerializeAsBson();
                case SerializationProvider.Xml:
                    return value.SerializeAsXml();
            }

            throw new NotSupportedException($"Serialization provider {provider} is not supported");
        }

        public static T Deserialize<T>(this string value, SerializationProvider provider)
        {
            switch (provider)
            {
                case SerializationProvider.Json:
                    return value.DeserializeFromJson<T>();
                case SerializationProvider.Bson:
                    return value.DeserializeFromBson<T>();
                case SerializationProvider.Xml:
                    return value.DeserializeFromXml<T>();
            }

            throw new NotSupportedException($"Serialization provider {provider} is not supported");
        }

        public static IEnumerable<string> SerializeObjects<T>(this IEnumerable<T> values, SerializationProvider provider)
        {
            switch (provider)
            {
                case SerializationProvider.Json:
                    return values.SerializeObjectsAsJson<T>();
                case SerializationProvider.Bson:
                    return values.SerializeObjectsAsBson<T>();
                case SerializationProvider.Xml:
                    return values.SerializeObjectsAsXml<T>();
            }

            throw new NotSupportedException($"Serialization provider {provider} is not supported");
        }

        public static IEnumerable<T> DeserializeObjects<T>(this IEnumerable<string> values, SerializationProvider provider)
        {
            switch (provider)
            {
                case SerializationProvider.Json:
                    return values.DeserializeObjectsFromJson<T>();
                case SerializationProvider.Bson:
                    return values.DeserializeObjectsFromBson<T>();
                case SerializationProvider.Xml:
                    return values.DeserializeObjectsFromXml<T>();
            }

            throw new NotSupportedException($"Serialization provider {provider} is not supported");
        }

        #region Xml
        public static string SerializeAsXml<T>(this T value)
        {
            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, value);

                return writer.ToString();
            }
        }

        public static T DeserializeFromXml<T>(this string value)
        {
            using (var stream = new MemoryStream(value.GetBytes<UTF8Encoding>()))
            {
                using (var reader = new StreamReader(stream))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(reader);
                }
            }
        }

        public static IEnumerable<string> SerializeObjectsAsXml<T>(this IEnumerable<T> values)
        {
            return values.Select(x => x.SerializeAsXml());
        }

        public static IEnumerable<T> DeserializeObjectsFromXml<T>(this IEnumerable<string> values)
        {
            return values.Select(x => x.DeserializeFromXml<T>());
        }
        #endregion

        #region Json
        public static T DeepCloneWithJson<T>(this T value)
        {
            if (value != null)
            {
                var serialized = value.SerializeAsJson();

                return serialized.DeserializeFromJson<T>();
            }
            else
            {
                return value;
            }
        }

        public static string SerializeAsJson<T>(this T value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public static T DeserializeFromJson<T>(this string value)
        {
            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            return JsonConvert.DeserializeObject<T>(value, settings);
        }

        public static IEnumerable<string> SerializeObjectsAsJson<T>(this IEnumerable<T> values)
        {
            return values.Select(x => x.SerializeAsJson());
        }

        public static IEnumerable<T> DeserializeObjectsFromJson<T>(this IEnumerable<string> values)
        {
            return values.Select(x => x.DeserializeFromJson<T>());
        }
        #endregion

        #region Bson
        public static string SerializeAsBson<T>(this T value)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BsonDataWriter(stream))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, value);

                    return stream.ToArray().ToBase64String();
                }
            }
        }

        public static T DeserializeFromBson<T>(this string value)
        {
            using (var stream = new MemoryStream(value.GetBytesFromBase64()))
            {
                using (var reader = new BsonDataReader(stream))
                {
                    reader.ReadRootValueAsArray = typeof(T).IsItemContainer();

                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<T>(reader);
                }
            }
        }

        public static IEnumerable<string> SerializeObjectsAsBson<T>(this IEnumerable<T> values)
        {
            return values.Select(x => x.SerializeAsBson());
        }

        public static IEnumerable<T> DeserializeObjectsFromBson<T>(this IEnumerable<string> values)
        {
            return values.Select(x => x.DeserializeFromBson<T>());
        }
        #endregion
    }
}
