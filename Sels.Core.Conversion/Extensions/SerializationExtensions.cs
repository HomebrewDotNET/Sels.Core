using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Sels.Core.Components.Serialization;
using Sels.Core.Contracts.Serialization;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;

namespace Sels.Core.Conversion.Extensions
{
    /// <summary>
    /// Contains extensions for serializing/deserializing objects to/from string using data formats like json, xml, ...
    /// </summary>
    public static class SerializationExtensions
    {
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
            return value.DeserializeFromXml(typeof(T)).Cast<T>();
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
        public static IEnumerable<string> SerializeAllsAsXml<T>(this IEnumerable<T> values)
        {
            return values.Select(x => x.SerializeAsXml());
        }

        /// <summary>
        /// Deserializes all Xml strings in <paramref name="values"/> to objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="values">List of Xml strings to deserialize</param>
        /// <returns>List containing all the deserialized Xml strings</returns>
        public static IEnumerable<T> DeserializeAllFromXml<T>(this IEnumerable<string> values)
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
        /// <param name="formatting">Formatting option</param>
        /// <returns>Json string</returns>
        public static string SerializeAsJson(this object value, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(value, formatting);
        }

        /// <summary>
        /// Deserializes the Json string <paramref name="value"/> to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="value">Json string to deserialize</param>
        /// <returns>Deserialized Json string</returns>
        public static T DeserializeFromJson<T>(this string value)
        {
            return value.DeserializeFromJson(typeof(T)).Cast<T>();
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
        public static List<string> SerializeAllAsJson<T>(this IEnumerable<T> values)
        {
            return values.Select(x => x.SerializeAsJson()).ToList();
        }

        /// <summary>
        /// Deserializes all Json strings in <paramref name="values"/> to objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="values">List of Json strings to deserialize</param>
        /// <returns>List containing all the deserialized Json strings</returns>
        public static List<T> DeserializeAllFromJson<T>(this IEnumerable<string> values)
        {
            return values.Select(x => x.DeserializeFromJson<T>()).ToList();
        }
        #endregion
    }
}
