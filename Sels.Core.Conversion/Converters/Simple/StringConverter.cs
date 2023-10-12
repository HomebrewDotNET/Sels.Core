using Sels.Core.Conversion.Templates;
using System;
using System.Collections.Generic;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts objects to string.
    /// </summary>
    public class StringConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            return convertType.Equals(typeof(string));
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            return value.ToString();
        }
    }
}
