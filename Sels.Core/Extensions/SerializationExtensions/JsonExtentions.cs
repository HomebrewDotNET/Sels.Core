using Newtonsoft.Json;
using Sels.Core.Extensions.Serialization.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Extensions.Serialization.Json
{
    public static class JsonExtentions
    {
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
            return JsonConvert.DeserializeObject<T>(value);
        }

        public static IEnumerable<string> SerializeObjectsAsJson<T>(this IEnumerable<T> values)
        {
            return values.Select(x => x.SerializeAsJson());
        }

        public static IEnumerable<T> DeserializeObjectsFromJson<T>(this IEnumerable<string> values)
        {
            return values.Select(x => x.DeserializeFromJson<T>());
        }

    }
}
