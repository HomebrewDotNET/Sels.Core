using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Sels.Core.Extensions.Execution.Linq;
using Sels.Core.Extensions.Object;
using Sels.Core.Extensions.Object.Byte;
using Sels.Core.Extensions.Object.String;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Reflection.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sels.Core.Extensions.Serialization.Bson
{
    public static class BsonExtensions
    {
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
            using (var stream = new MemoryStream(value.To64ByteArray()))
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
    }
}
