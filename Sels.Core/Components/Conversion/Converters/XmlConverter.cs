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
    /// Converts xml strings to objects and objects to xml strings.
    /// </summary>
    public class XmlConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            // Convert from xml string to object.
            if (convertableType.Equals(typeof(string)) && !convertType.IsPrimitive && value != null && value.ToString().IsXml())
            {
                return true;
            }
            // Convert object to xml string
            else if (!convertableType.IsPrimitive && convertType.Equals(typeof(string)) && value != null)
            {
                return true;
            }

            return false;
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            // Convert from xml string to object.
            if (convertableType.Equals(typeof(string)) && !convertType.IsPrimitive && value != null)
            {
                return value.ToString().DeserializeFromXml(convertableType);
            }
            // Convert object to xml string
            else if (!convertableType.IsPrimitive && convertType.Equals(typeof(string)) && value != null)
            {
                return value.SerializeAsXml();
            }

            return convertType.GetDefaultValue();
        }
    }
}
