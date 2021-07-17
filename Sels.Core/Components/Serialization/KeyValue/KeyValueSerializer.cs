using Microsoft.Extensions.Logging;
using Sels.Core.Components.Conversion;
using Sels.Core.Components.Serialization.KeyValue.Converters;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Contracts.Serialization.KeyValue;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sels.Core.Components.Serialization.KeyValue
{
    public class KeyValueSerializer : ISerializationProvider
    {
        // Constants
        public const string DefaultValueSplitter = ":";

        // Statics
        private static TypeConverter _defaultTypeConverter = new TypeConverter();
        private static CollectionFactory _defaultCollectionFactory = new CollectionFactory();
        private static IGenericTypeConverter _defaultElementConverter = GenericConverter.DefaultConverter;
        private static Type[] _excludedCollectionTypes = new Type[] { typeof(string) };

        // Fields
        private bool _trimLines;
        private bool _trimKeys;
        private bool _trimValues;

        // Properties
        /// <summary>
        /// Used to split/join key value pairs from an item
        /// </summary>
        public string KeyValueSplitter { get; }
        /// <summary>
        /// Used to split/join values from/to the source string
        /// </summary>
        public string ItemSplitter { get; set; }

        public KeyValueSerializer(string keyValueSplitter = DefaultValueSplitter, string itemSplitter = null, KeyValueSerializerSettings settings = null)
        {
            keyValueSplitter.ValidateVariable(nameof(keyValueSplitter));

            KeyValueSplitter = keyValueSplitter;
            ItemSplitter = itemSplitter ?? Environment.NewLine;

            if (settings.HasValue() && settings.ParsingOptions.HasValue())
            {
                if (settings.ParsingOptions.Contains(ParsingOption.TrimKey)) _trimKeys = true;
                if (settings.ParsingOptions.Contains(ParsingOption.TrimItem)) _trimLines = true;
                if (settings.ParsingOptions.Contains(ParsingOption.TrimValue)) _trimValues = true;
            }
        }

        public T Deserialize<T>(string value) where T : new()
        {
            // Build profile for type T.
            var profile = BuildSerializerProfile(typeof(T));

            // Create object
            var newObject = new T();

            // Divide value into multiple lines and get key value pairs from those lines
            var keyValuePairs = GetItems(value).Select(x => GetKeyValuePair(x));

            // Group the pairs by key
            var groupedPairs = keyValuePairs.GroupAsDictionary(x => x.Key, x => x.Value);

            // Deserialize each grouped pair and add it to deserializedObject
            foreach(var itemSerializer in profile)
            {
                if(groupedPairs.TryGetValue(itemSerializer.Key, out var values))
                {
                    itemSerializer.DeserializeInto(newObject, values);
                }
            }

            return newObject;
        }

        public string Serialize<T>(T value)
        {
            // Build profile for type T.
            var profile = BuildSerializerProfile(typeof(T));

            // Serialize all properties and create key value pairs
            var keyValuePairs = new List<string>();

            foreach(var itemSerializer in profile)
            {
                keyValuePairs.AddRange(itemSerializer.Serialize(value).Select(x => BuildKeyValuePair(itemSerializer.Key, x)));
            }

            // Join all key value pairs into final string.
            return BuildItems(keyValuePairs);
        }

        private string[] GetItems(string value)
        {
            return value.Split(ItemSplitter, StringSplitOptions.RemoveEmptyEntries).Where(x => x.HasValue() && x.Contains(KeyValueSplitter)).ModifyItemIf(x => _trimLines, x => x.Trim()).ToArray();            
        }

        private string BuildItems(IEnumerable<string> values)
        {
            return values.JoinString(ItemSplitter);
        }

        private (string Key, string Value) GetKeyValuePair(string item)
        {
            var key = item.TrySplitFirstOrDefault(KeyValueSplitter, out string value);

            return ( key.ModifyIf(x => _trimKeys, x => x.Trim()), value.ModifyIf(x => _trimValues, x => x.Trim()));
        }

        private string BuildKeyValuePair(string key, string value)
        {
            return Helper.Strings.JoinStrings(KeyValueSplitter, key, value);
        }

        private List<ItemSerializer> BuildSerializerProfile(Type type)
        {
            var profile = new List<ItemSerializer>();

            foreach(var property in type.GetProperties())
            {
                var serializerAttribute = property.GetCustomAttributes().FirstOrDefault(x => x.IsAssignableTo<IKeyValueSerializationAttribute>()).AsOrDefault<IKeyValueSerializationAttribute>();

                // Let attribute handle the serialization/deserialization.
                if (serializerAttribute.HasValue())
                {
                    if (serializerAttribute.Ignore) continue;

                    var key = serializerAttribute.Key ?? property.Name;

                    profile.Add(new ItemSerializer(key, property, (type, propertyValue) => serializerAttribute.Serialize(type, propertyValue), (type, propertyValue, values) => serializerAttribute.Deserialize(type, values, propertyValue)));
                }
                // Serialize/deserialize each item with converter and use collection factory to create or update property collection.
                else if (!_excludedCollectionTypes.Any(x => property.PropertyType.IsAssignableTo(x)) && property.PropertyType.IsItemContainer())
                {
                    profile.Add(new ItemSerializer(property.Name, property, (type, propertyValue) => 
                    {
                        var collectionElementType = property.PropertyType.GetItemTypeFromContainer();

                        return _defaultCollectionFactory.ParseItems(propertyValue.AsOrDefault<IEnumerable>().Enumerate().Select(x => _defaultElementConverter.ConvertTo(collectionElementType, typeof(string), x).As<string>()));
                    }, 
                    (type, propertyValue, values) => 
                    {
                        return _defaultCollectionFactory.CreateCollection(type, values.Select(x => _defaultElementConverter.ConvertTo(typeof(string) ,type, x)), propertyValue);
                    }));
                }
                // Default type converter which can handle most base types.
                else
                {
                    profile.Add(new ItemSerializer(property.Name, property, (type, propertyValue) =>
                    {
                        return _defaultTypeConverter.ConvertTo(type, propertyValue);
                    },
                    (type, propertyValue, values) =>
                    {
                        return _defaultTypeConverter.ConvertTo(type, values);
                    }));
                }
            }

            return profile;
        }
    }

    public static class KeyValueConverter
    {
        public static KeyValueSerializer DefaultSerializer => new KeyValueSerializer(settings: new KeyValueSerializerSettings(ParsingOption.TrimItem, ParsingOption.TrimKey, ParsingOption.TrimValue));

        public static string Serialize<T>(T value)
        {
            return DefaultSerializer.Serialize<T>(value);
        }

        public static T Deserialize<T>(string value) where T : new()
        {
            return DefaultSerializer.Deserialize<T>(value);
        }
    }

    internal class ItemSerializer
    {
        // Fields
        private readonly Func<Type, object, string[]> _serializerFunc;
        private readonly Func<Type, object, IEnumerable<string>, object> _deserializerFunc;

        // Properties
        public string Key { get; }
        public PropertyInfo Property { get; }

        public ItemSerializer(string key, PropertyInfo property, Func<Type, object, string[]> serializerFunc, Func<Type, object, IEnumerable<string>, object> deserializerFunc)
        {
            Key = key.ValidateArgumentNotNullOrWhitespace(nameof(key));
            Property = property.ValidateArgument(nameof(property));
            _serializerFunc = serializerFunc.ValidateArgument(nameof(key));
            _deserializerFunc = deserializerFunc.ValidateArgument(nameof(deserializerFunc));
        }

        internal string[] Serialize(object objectToSerialize)
        {
            return _serializerFunc(Property.PropertyType, Property.GetValue(objectToSerialize));
        }

        internal void DeserializeInto(object objectToPopulate, IEnumerable<string> values)
        {
            var propertyValue = _deserializerFunc(Property.PropertyType, Property.GetValue(objectToPopulate), values);

            if(propertyValue != null && Property.PropertyType.IsAssignableFrom(propertyValue.GetType()))
            {
                Property.SetValue(objectToPopulate, propertyValue);
            }
        }
    }

    public class KeyValueSerializerSettings
    {
        // Properties
        public ParsingOption[] ParsingOptions { get; set; }

        public KeyValueSerializerSettings()
        {

        }

        public KeyValueSerializerSettings(params ParsingOption[] parsingOptions)
        {
            ParsingOptions = parsingOptions;
        }
    }

    public enum ParsingOption
    {
        /// <summary>
        /// Trim key when splitting each item.
        /// </summary>
        TrimKey,
        /// <summary>
        /// Trim value when splitting each item.
        /// </summary>
        TrimValue,
        /// <summary>
        /// Trim each item when splitting the source string into multiple items.
        /// </summary>
        TrimItem
    }
}
