using Sels.Core.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Sels.Core.Extensions.Serialization
{
    public static class XmlExtensions
    {
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
                    return (T) serializer.Deserialize(reader);
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
    }
}
