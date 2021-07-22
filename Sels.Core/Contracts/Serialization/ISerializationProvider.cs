using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Serialization
{
    /// <summary>
    /// Service that can serialize and deserialize objects
    /// </summary>
    public interface ISerializationProvider
    {
        /// <summary>
        /// Serializes <paramref name="value"/> to a string.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="value">Object to serialize</param>
        /// <returns>String representing <paramref name="value"/></returns>
        string Serialize<T>(T value);

        /// <summary>
        /// Deserializes <paramref name="value"/> into an object to Type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="value">String containing the serialized object</param>
        /// <returns>Deserialized object from <paramref name="value"/></returns>
        T Deserialize<T>(string value) where T : new();
    }
}
