using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Conversion.Converters
{
    /// <summary>
    /// Converts json strings to objects and objects to json strings.
    /// </summary>
    public class JsonConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            // Convert from json string to object.
            if(convertableType.Equals(typeof(string)) && !convertType.IsPrimitive && value != null && value.ToString().IsJson()){
                return true;
            }
            // Convert object to json string
            else if(!convertableType.IsPrimitive && convertType.Equals(typeof(string)) && value != null)
            {
                return true;
            }

            return false;
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            // Convert from json string to object.
            if (convertableType.Equals(typeof(string)) && !convertType.IsPrimitive && value != null)
            {
                return value.ToString().DeserializeFromJson(convertableType);
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
