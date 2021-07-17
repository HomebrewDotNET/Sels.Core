using Sels.Core.Components.Conversion;
using Sels.Core.Components.Serialization.KeyValue.Converters;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Contracts.Serialization.KeyValue;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Serialization.KeyValue.Attributes
{
    /// <summary>
    /// Used to configure the serialization between the property this is defined on and the string value. This attribute adds extra options for collection properties.
    /// </summary>
    public class KeyValueCollectionAttribute : BaseKeyValueAttribute, IKeyValueSerializationAttribute
    {
        // Statics
        protected static Type DefaultFactory = typeof(CollectionFactory);
        protected static IGenericTypeConverter DefaultTypeConverter = GenericConverter.DefaultConverter;

        // Properties
        /// <summary>
        /// Optional factory for handling items from collections.
        /// </summary>
        public Type FactoryType { get; }
        /// <summary>
        /// Optional argument that can be passed down to an <see cref="IKeyValueCollectionFactory"/>.
        /// </summary>
        public object FactoryArgument { get; }
        /// <summary>
        /// Optional type of a <see cref="IGenericTypeConverter"/> to use for converting to/from collection elements.
        /// </summary>
        public Type TypeConverterType { get; }

        public KeyValueCollectionAttribute(string key = null, Type factoryType = null, object factoryArgument = null, Type typeConverterType = null, bool ignore = false) : base(key, ignore)
        {
            FactoryType = factoryType != null ? factoryType.ValidateArgumentAssignableTo(nameof(factoryType), typeof(IKeyValueCollectionFactory)).ValidateArgumentCanBeContructedWith(nameof(factoryType)) : null;
            TypeConverterType = typeConverterType?.ValidateArgumentAssignableTo(nameof(typeConverterType), typeof(IGenericTypeConverter)).ValidateArgumentCanBeContructedWith(nameof(typeConverterType));
            FactoryArgument = factoryArgument;
        }

        public override string[] Serialize(Type propertyType, object propertyValue)
        {            
            if (propertyType.IsItemContainer())
            {
                var propertyCollectionItemType = propertyType.GetItemTypeFromContainer();
                var typeConverter = TypeConverterType != null ? TypeConverterType.Construct<IGenericTypeConverter>() : DefaultTypeConverter;
                var factoryType = FactoryType ?? DefaultFactory;
                var factory = factoryType.Construct<IKeyValueCollectionFactory>();

                var serializedItems = new List<string>();

                // Serialize all items in collection first
                foreach(var item in (IEnumerable)propertyValue)
                {
                    var serializedItem = typeConverter.ConvertTo(propertyCollectionItemType, typeof(string), item).As<string>();
                    serializedItems.Add(serializedItem);
                }

                // Let factory handle the final serialization
                return factory.ParseItems(serializedItems, FactoryArgument);
            }

            return null;
        }

        public override object Deserialize(Type propertyType, IEnumerable<string> values, object propertyValue = null)
        {
            if (propertyType.IsItemContainer())
            {
                var propertyCollectionItemType = propertyType.GetItemTypeFromContainer();
                var typeConverter = TypeConverterType != null ? TypeConverterType.Construct<IGenericTypeConverter>() : DefaultTypeConverter;
                var factoryType = FactoryType ?? DefaultFactory;
                var factory = factoryType.Construct<IKeyValueCollectionFactory>();

                var deserializedItems = new List<object>();

                // Deserialize all items in collection first
                foreach (var item in values)
                {
                    var deserializedItem = typeConverter.ConvertTo(typeof(string), propertyCollectionItemType, item);
                    deserializedItems.Add(deserializedItem);
                }

                // Let factory handle the adding or updating the deserializedItems to the property collection
                return factory.CreateCollection(propertyType, deserializedItems, FactoryArgument);
            }

            return propertyType.GetDefaultValue();
        }
    }
}
