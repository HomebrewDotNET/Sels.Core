using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Sels.Core.Extensions.Object
{
    public static class ObjectExtensions
    {
        public static string GetTypeName(this object value)
        {
            return value.GetType().ToString();
        }

        public static T Cast<T>(object value) where T : class
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static T[] ItemToArray<T>(this T value)
        {
            value.ValidateVariable(nameof(value));

            return new T[] { value };
        }

        public static T[] ItemToArrayOrDefault<T>(this T value)
        {
            if (value.HasValue())
            {
                return new T[] { value };
            }

            return new T[0];
        }

    }
}
