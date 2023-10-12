using Newtonsoft.Json;
using Sels.Core.Conversion.Extensions;
using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts json strings to objects and objects to json strings.
    /// </summary>
    public class JsonConverter : BaseTypeConverter
    {
        // Constants
        /// <summary>
        /// Argument for providing the json settings when converting.
        /// </summary>
        public const string SettingsArgument = "Json.Settings";

        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
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
        protected override object ConvertObjectTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            var convertableType = value.GetType();
            JsonSerializerSettings settings = arguments.HasValue() && arguments.TryGetValue<JsonSerializerSettings>(SettingsArgument, out var jsonSettings) ? jsonSettings : null;

            // Convert from json string to object.
            if (convertableType.Equals(typeof(string)) && !convertType.IsPrimitive && value != null)
            {
                return value.ToString().DeserializeFromJson(convertType, settings);
            }
            // Convert object to json string
            else if (!convertableType.IsPrimitive && convertType.Equals(typeof(string)) && value != null)
            {
                return value.SerializeAsJson(settings);
            }

            return convertType.GetDefaultValue();
        }
    }
}
