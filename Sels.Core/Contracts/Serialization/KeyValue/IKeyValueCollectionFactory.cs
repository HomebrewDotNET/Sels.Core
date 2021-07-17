using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Serialization.KeyValue
{
    /// <summary>
    /// Factory for creating or updating the property collection when deserializing or joining together the serialized string values.
    /// </summary>
    public interface IKeyValueCollectionFactory
    {
        /// <summary>
        /// Creates a collection containing the deserializedItems.
        /// </summary>
        /// <param name="collectionType">Type of collection</param>
        /// <param name="propertyCollection">Property value</param>
        /// <param name="deserializedItems">Deserialized items</param>
        /// <param name="factoryArgument">Optional argument from KeyValueCollectionAttribute</param>
        /// <returns>Collection</returns>
        public object CreateCollection(Type collectionType, IEnumerable<object> deserializedItems, object factoryArgument = null);

        /// <summary>
        /// Used to parse serialized items from source collection before adding the key before all items. Can be used to either merge/join some or all items.
        /// </summary>
        /// <param name="serializedItems">Serialized items from source collection</param>
        /// <param name="factoryArgument">Optional argument from KeyValueCollectionAttribute</param>
        /// <returns>Joined/Merged items</returns>
        public string[] ParseItems(IEnumerable<string> serializedItems, object factoryArgument = null);
    }
}
