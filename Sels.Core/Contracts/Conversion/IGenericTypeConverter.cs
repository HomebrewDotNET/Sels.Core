using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Conversion
{
    /// <summary>
    /// Converter that can converts objects into other types.
    /// </summary>
    public interface IGenericTypeConverter
    {
        /// <summary>
        /// Checks this converter can convert <paramref name="value"/> of type <paramref name="convertableType"/> to type <paramref name="convertType"/>.
        /// </summary>
        /// <param name="convertableType">Type to convert</param>
        /// <param name="convertType">Type to convert to</param>
        /// <param name="value">Value to convert</param>
        /// <returns>Boolean indicating if this converter can convert from <paramref name="convertableType"/> to <paramref name="convertType"/></returns>
        bool CanConvert(Type convertableType, Type convertType, object value);

        /// <summary>
        /// Converts <paramref name="value"/> of type <paramref name="convertableType"/> to <paramref name="convertType"/>.
        /// </summary>
        /// <param name="convertableType">Type to convert</param>
        /// <param name="convertType">Type to convert to</param>
        /// <param name="value">Object to convert</param>
        /// <returns>Converted value</returns>
        object ConvertTo(Type convertableType, Type convertType, object value);
    }
}
