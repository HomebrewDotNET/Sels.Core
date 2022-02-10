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
    /// Converts xml strings to objects and objects to xml strings.
    /// </summary>
    public class XmlConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();

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
        /// <inheritdoc
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {

            var convertableType = value.GetType();
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
