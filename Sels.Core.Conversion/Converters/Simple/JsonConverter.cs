using Sels.Core.Conversion.Extensions;
using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts json strings to objects and objects to json strings.
    /// </summary>
    public class JsonConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();

            // Convert from json string to object.
            if (convertableType.Equals(typeof(string)) && !convertType.IsPrimitive && value.ToString().IsJson())
            {
                return true;
            }
            // Convert object to json string
            else if (!convertableType.IsPrimitive && convertType.Equals(typeof(string)))
            {
                return true;
            }

            return false;
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();

            // Convert from json string to object.
            if (convertableType.Equals(typeof(string)) && !convertType.IsPrimitive && value != null)
            {
                return value.ToString().DeserializeFromJson(convertType);
            }
            // Convert object to json string
            else if (!convertableType.IsPrimitive && convertType.Equals(typeof(string)) && value != null)
            {
                return value.SerializeAsJson();
            }

            return convertType.GetDefaultValue();
        }
    }
}
