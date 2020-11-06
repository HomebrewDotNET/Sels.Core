using Sels.Core.Components.Serialization;
using Sels.Core.Extensions.Serialization.Bson;
using Sels.Core.Extensions.Serialization.Json;
using Sels.Core.Extensions.Serialization.Xml;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Sels.Core.Extensions.Serialization
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
    }
}
