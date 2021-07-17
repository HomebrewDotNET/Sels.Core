using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Components.Serialization.KeyValue.Attributes;

namespace Sels.Core.Contracts.Serialization.KeyValue
{
    /// <summary>
    /// Converter for converting between the string value and other types.
    /// </summary>
    public interface IKeyValueConverter
    {
        /// <summary>
        /// Converts the <paramref name="values"/> to <paramref name="conversionType"/>. Used for converting the values for the same key to a property value.
        /// </summary>
        /// <param name="conversionType">Type to convert to</param>
        /// <param name="values">String values to convert</param>
        /// <param name="converterArgument">Optional argument from <see cref="IKeyValueSerializationAttribute"/></param>
        /// <returns>Converted object</returns>
        object ConvertTo(Type conversionType, IEnumerable<string> values, object converterArgument = null);

        /// <summary>
        /// Converts <paramref name="value"/> of Type <paramref name="conversionType"/> to string. Used for converting a property value to values for the same key.
        /// </summary>
        /// <param name="conversionType">Type to convert from</param>
        /// <param name="value">Property value to convert</param>
        /// <param name="converterArgument">Optional argument from <see cref="IKeyValueSerializationAttribute"/></param>
        /// <returns>Converted object</returns>
        string[] ConvertTo(Type conversionType, object value, object converterArgument = null);
    }
}
