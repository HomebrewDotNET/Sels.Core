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
        public string Key { get; }
        /// <summary>
        /// Set to true to ignore property during serialization.
        /// </summary>
        public bool Ignore { get; }

        /// <summary>
        /// Serializes the property value.
        /// </summary>
        /// <param name="propertyType">Property type</param>
        /// <param name="propertyValue">Property value</param>
        /// <returns>Serialized values</returns>
        public string[] Serialize(Type propertyType, object propertyValue);

        /// <summary>
        /// Deserializes the string to the property value.
        /// </summary>
        /// <param name="values">String values to deserialize</param>
        /// <returns>Property value</returns>
        public object Deserialize(Type propertyType, IEnumerable<string> values, object propertyValue = null);
    }
}
