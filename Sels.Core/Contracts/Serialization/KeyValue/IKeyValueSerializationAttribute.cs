using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Serialization.KeyValue
{
    internal interface IKeyValueSerializationAttribute
    {
        /// <summary>
        /// Specifies the key name.
        /// </summary>
        string Key { get; }
        /// <summary>
        /// Set to true to ignore property during serialization.
        /// </summary>
        bool Ignore { get; }

        /// <summary>
        /// Serializes the property value.
        /// </summary>
        /// <param name="propertyType">Property type</param>
        /// <param name="propertyValue">Property value</param>
        /// <returns>Serialized values</returns>
        string[] Serialize(Type propertyType, object propertyValue);

        /// <summary>
        /// Deserializes the string to the property value.
        /// </summary>
        /// <param name="values">String values to deserialize</param>
        /// <returns>Property value</returns>
        object Deserialize(Type propertyType, IEnumerable<string> values, object propertyValue = null);
    }
}
