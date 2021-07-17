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
        private static char[] _validXmlStartTokens = new char[] { '<' };

        /// <summary>
        /// Checks if <paramref name="value"/> start with a valid xml token.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>Boolean indicating if <paramref name="value"/> starts with a valid xml token</returns>
        public static bool IsXml(this string value)
        {
            return value != null && value.Length > 0 && _validXmlStartTokens.Any(x => x.Equals(value[0]));
        }

        /// <summary>
        /// Serializes <paramref name="value"/> to a Xml string.
        /// </summary>
        /// <param name="value">Object to serialize</param>
        /// <returns>Xml string</returns>
        public static string SerializeAsXml(this object value)
        {
            if(value != null)
            {
                using (var writer = new StringWriter())
                {
                    var serializer = new XmlSerializer(value.GetType());
                    serializer.Serialize(writer, value);

                    return writer.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Deserializes the Xml string <paramref name="value"/> to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="value">Xml string to deserialize</param>
        /// <returns>Deserialized Xml string</returns>
        public static T DeserializeFromXml<T>(this string value)
        {
            return value.DeserializeFromXml(typeof(T)).As<T>();
        }

        /// <summary>
        /// Deserializes the Xml string <paramref name="value"/> to an object of type <paramref name="type"/>.
        /// </summary>
        /// <param name="value">Xml string to deserialize</param>
        /// <param name="type">Type of object to deserialize to</param>
        /// <returns>Deserialized Xml string</returns>
        public static object DeserializeFromXml(this string value, Type type)
        {
            type.ValidateArgument(nameof(type));

            if (value.HasValue())
            {
                using (var stream = new MemoryStream(value.GetBytes<UTF8Encoding>()))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var serializer = new XmlSerializer(type);
                        return serializer.Deserialize(reader);
                    }
                }
            }

            return type.GetDefaultValue();
        }

        /// <summary>
        /// Serializes all objects in <paramref name="values"/> to Xml strings.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="values">List of objects to serialize</param>
        /// <returns>List containing the serialized Xml strings</returns>
        public static IEnumerable<string> SerializeObjectsAsXml<T>(this IEnumerable<T> values)
        {
            return values.Select(x => x.SerializeAsXml());
        }

        /// <summary>
        /// Deserializes all Xml strings in <paramref name="values"/> to objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="values">List of Xml strings to deserialize</param>
        /// <returns>List containing all the deserialized Xml strings</returns>
        public static IEnumerable<T> DeserializeObjectsFromXml<T>(this IEnumerable<string> values)
        {
            return values.Select(x => x.DeserializeFromXml<T>());
        }
        #endregion

        #region Json
        private static char[] _validJsonStartTokens = new char[] { '{', '[' };

        /// <summary>
        /// Checks if <paramref name="value"/> start with a valid json token.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>Boolean indicating if <paramref name="value"/> starts with a valid json token</returns>
        public static bool IsJson(this string value)
        {
            return value != null && value.Length > 0 && _validJsonStartTokens.Any(x => x.Equals(value[0]));
        }

        /// <summary>
        /// Clones <paramref name="value"/> using Json.
        /// </summary>
        /// <typeparam name="T">Object of type to clone</typeparam>
        /// <param name="value">Object to clone</param>
        /// <returns>Cloned object</returns>
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

        /// <summary>
        /// Serializes <paramref name="value"/> to a Json string.
        /// </summary>
        /// <param name="value">Object to serialize</param>
        /// <returns>Json string</returns>
        public static string SerializeAsJson(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Deserializes the Json string <paramref name="value"/> to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="value">Json string to deserialize</param>
        /// <returns>Deserialized Json string</returns>
        public static T DeserializeFromJson<T>(this string value)
        {
            return value.DeserializeFromJson(typeof(T)).As<T>();
        }

        /// <summary>
        /// Deserializes the Json string <paramref name="value"/> to an object of type <paramref name="type"/>.
        /// </summary>
        /// <param name="value">Json string to deserialize</param>
        /// <param name="type">Type of object to deserialize to</param>
        /// <returns>Deserialized Json string</returns>
        public static object DeserializeFromJson(this string value, Type type)
        {
            type.ValidateArgument(nameof(type));

            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            return JsonConvert.DeserializeObject(value, type, settings);
        }

        /// <summary>
        /// Serializes all objects in <paramref name="values"/> to Json strings.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="values">List of objects to serialize</param>
        /// <returns>List containing the serialized Json strings</returns>
        public static List<string> SerializeObjectsAsJson<T>(this IEnumerable<T> values)
        {
            return values.Select(x => x.SerializeAsJson()).ToList();
        }

        /// <summary>
        /// Deserializes all Json strings in <paramref name="values"/> to objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="values">List of Json strings to deserialize</param>
        /// <returns>List containing all the deserialized Json strings</returns>
        public static List<T> DeserializeObjectsFromJson<T>(this IEnumerable<string> values)
        {
            return values.Select(x => x.DeserializeFromJson<T>()).ToList();
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
