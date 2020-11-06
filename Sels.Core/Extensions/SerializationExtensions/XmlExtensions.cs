using Sels.Core.Extensions.Object;
using Sels.Core.Extensions.Object.Byte;
using Sels.Core.Extensions.Object.String;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Sels.Core.Extensions.Serialization.Xml
{
    public static class XmlExtensions
    {
        public static string SerializeAsXml<T>(this T value)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, value);

                    return stream.ToArray().ToBase64String();
                }
            }
        }

        public static T DeserializeFromXml<T>(this string value)
        {
            using (var stream = new MemoryStream(value.To64ByteArray()))
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
