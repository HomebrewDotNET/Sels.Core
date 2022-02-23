using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Contracts;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Components.Serialization.Profile;
using Sels.Core.Conversion.Attributes.KeyValue;
using System.Collections;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Conversion.Templates;

namespace Sels.Core.Conversion.Serializers.KeyValue
{
    /// <summary>
    /// Serializer that divides a string into key/value pairs where the key dictates what property will be used for the value.
    /// </summary>
    public class KeyValueSerializer : BaseSerializer<IEnumerable<KeyValuePair<string, string>>>
    {
        // Fields
        private readonly static Type[] _excludedCollectionTypes = new Type[] { typeof(string) };

        // Constants
        /// <summary>
        /// The default substring that is used to separate the key and the value.
        /// </summary>
        public const string KeyValueSeparator = ":";
        /// <summary>
        /// The default substring that will be used to join/split elements.
        /// </summary>
        public const string ElementSeparator = ",";

        // Fields
        private readonly KeyValueSerializerConfiguration _configuration;

        /// <inheritdoc cref="KeyValueSerializer"/>
        /// <param name="settings">Optional settings for this serializer</param>
        public KeyValueSerializer(KeyValueSerializerSettings settings = KeyValueSerializerSettings.UseDefaultConverter) : this(x => { }, settings)
        {

        }
        /// <inheritdoc cref="KeyValueSerializer"/>
        /// <param name="configurator">Delegate for configuring this serializer</param>
        /// <param name="settings">Optional settings for this serializer</param>
        public KeyValueSerializer(Action<IKeyValueSerializerConfigurator> configurator, KeyValueSerializerSettings settings = KeyValueSerializerSettings.UseDefaultConverter)
        {
            configurator.ValidateArgument(nameof(configurator));

            _configuration = new KeyValueSerializerConfiguration(settings);
            configurator(_configuration);
            if (_configuration.UseDefaultConverter) _configuration.UseConverters(GenericConverter.DefaultConverter);

            _loggers = _configuration.Loggers;
            CreateTypeHandlers();
        }

        /// <summary>
        /// Deserializes the key/value pairs in <paramref name="value"/> to <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="T">Type of the object to deserialize to</typeparam>
        /// <param name="value">The string containing the key/value pairs</param>
        /// <param name="instance">The instance to deserialize to</param>
        /// <returns>The instance deserialized from <paramref name="value"/></returns>
        public T Deserialize<T>(string value, T instance)
        {
            using (_loggers.TraceMethod(this))
            {
                value.ValidateArgument(nameof(value));
                instance.ValidateArgument(nameof(instance));
                var type = instance.GetType();

                var pairs = _configuration.GetPairs(value);
                using (_loggers.TraceAction($"Deserializing <{pairs.GetCount()}> key/value pairs to an instance of type <{type}>"))
                {
                   return DeserializeTo(pairs, instance).Cast<T>();
                }
            }
        }
        /// <summary>
        /// Deserializes the key/value pairs in <paramref name="value"/> to an instance of type <paramref name="type"/>.
        /// </summary>
        /// <param name="value">The string containing the key/value pairs</param>
        /// <param name="type">The type to deserialize to</param>
        /// <returns>The instance deserialized from <paramref name="value"/></returns>
        public object Deserialize(string value, Type type)
        {
            using (_loggers.TraceMethod(this))
            {
                value.ValidateArgument(nameof(value));
                type.ValidateArgument(nameof(type));
                type.ValidateArgumentCanBeContructedWithArguments(nameof(type));

                return Deserialize(value, type.Construct());
            }
        }   
        /// <summary>
        /// Deserializes the key/value pairs in <paramref name="value"/> to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="value">The string containing the key/value pairs</param>
        /// <returns>The instance deserialized from <paramref name="value"/></returns>
        public T Deserialize<T>(string value) where T : new()
        {
            using (_loggers.TraceMethod(this))
            {
                value.ValidateArgument(nameof(value));

                return Deserialize(value, typeof(T)).CastOrDefault<T>();
            }
        }

        /// <summary>
        /// Serializes <paramref name="instance"/> to a string containing key/value pairs.
        /// </summary>
        /// <param name="instance">The instance to serialize</param>
        /// <returns>String containing key/value pairs representing <paramref name="instance"/></returns>
        public string Serialize(object instance)
        {
            using (_loggers.TraceMethod(this))
            {
                instance.ValidateArgument(nameof(instance));
                var type = instance.GetType();

                using (_loggers.TraceAction($"Serializing instance of type <{type}> to key/value pairs"))
                {
                    var pairs = SerializeFrom(instance);
                    return _configuration.GetString(pairs);
                }
            }
        }

        private void CreateTypeHandlers()
        {
            // Default handler
            AddTypeHandler(x => x.Handles(t => true).SerializeUsing((type, instance) =>
            {
                var profile = new KeyValueSerializerProfile(_configuration, type);
                return profile.Serialize(instance);
            }).DeserializeUsing((pairs, instance) => {
                var profile = new KeyValueSerializerProfile(_configuration, instance.GetType());
                return profile.Deserialize(pairs, instance);
            }));
        }

        private class KeyValueSerializerProfile
        {
            // Fields
            private readonly Type _type;
            private readonly IEnumerable<ILogger> _loggers;
            private readonly bool _ignoreFailedConversion;
            private readonly Dictionary<PropertyInfo, (string Key, bool IsMerge, PropertySerializationProfile Profile)> _typeProfiles;

            public KeyValueSerializerProfile(KeyValueSerializerConfiguration config, Type type)
            {
                config.ValidateArgument(nameof(config));
                _type = type.ValidateArgument(nameof(type));

                _ignoreFailedConversion = config.IgnoreFailedConversion;
                _loggers = config.Loggers;

                var typeProfile = new SerializationProfile(type, config.PropertyFlags, config.Converters, config.Filters, config.ElementFilters, config.Loggers);
                _typeProfiles = typeProfile.PropertyProfiles.ToDictionary(x => x.Key, x => (x.Value.Property.GetKey(), x.Value.Property.MergePairs(), x.Value));
            }

            public IEnumerable<KeyValuePair<string, string>> Serialize(object instance)
            {
                using (_loggers.TraceMethod(this))
                {
                    instance.ValidateArgument(nameof(instance));
                    var type = instance.GetType();
                    var pairs = new List<KeyValuePair<string, string>>();

                    _loggers.TraceObject($"Serializing to key/value pairs", instance);
                    foreach(var propertyProfile in _typeProfiles)
                    {
                        var property = propertyProfile.Key;
                        var key = propertyProfile.Value.Key;
                        var isMerge = propertyProfile.Value.IsMerge;
                        var profile = propertyProfile.Value.Profile;

                        using(_loggers.TraceAction(LogLevel.Debug, $"Serializing property <{property.Name}> on <{type}>"))
                        {
                            var value = property.GetValue(profile.IsStatic ? null : instance);
                            if (value == null)
                            {
                                _loggers.Debug($"Property <{property.Name}> on <{type}> is null. Skipping");
                                continue;
                            }

                            try
                            {                                
                                if (profile.IsCollection && !_excludedCollectionTypes.Contains(property.PropertyType))
                                {
                                    _loggers.Debug($"Property <{property.Name}> on <{type}> is a collection. Serializing elements");
                                    var valueCollection = value.Cast<IEnumerable>();
                                    List<string> serializedElements = new List<string>();

                                    valueCollection.Enumerate().Where(x => x != null).Execute((i, element) =>
                                    {
                                        if (element == null) return;
                                        _loggers.Trace($"Serializing element <{i}> from property <{property.Name}> on <{type}>");
                                        if(profile.Converters.TryConvertTo<string>(element, out var converted, profile.ConverterArguments))
                                        {
                                            _loggers.Trace($"Serialized element <{i}> from property <{property.Name}> on <{type}> to <{converted}>");
                                            serializedElements.Add(converted);
                                        }
                                        else
                                        {
                                            if (!_ignoreFailedConversion) throw new InvalidOperationException($"Could not convert element <{i}> from property <{property.Name}> on <{type}> to <{typeof(string)}>");
                                            _loggers.Warning($"No converter could convert element <{i}> from property <{property.Name}> on <{type}> to <{typeof(string)}>. Skipping");
                                            return;
                                        }
                                    });

                                    // Filter elements
                                    var elements = profile.Filter(serializedElements, true);

                                    if (isMerge)
                                    {
                                        _loggers.Trace($"Merging <{serializedElements.Count}> serialized elements from property <{property.Name}> on <{type}>");
                                        var merged = profile.ElementSeparator?.Join(elements) ?? elements.JoinString(ElementSeparator);
                                        pairs.Add(CreatePair(profile, key, merged));
                                        continue;
                                    }
                                    else
                                    {
                                        pairs.AddRange(elements.Select(x => CreatePair(profile, key, x)));
                                    }
                                }
                                else
                                {
                                    if (profile.Converters.TryConvertTo<string>(value, out var converted, profile.ConverterArguments))
                                    {
                                        _loggers.Trace($"Serialized property <{property.Name}> on <{type}> to <{converted}>");
                                        pairs.Add(CreatePair(profile, key, converted));
                                    }
                                    else
                                    {
                                        if (!_ignoreFailedConversion) throw new InvalidOperationException($"Could not convert property <{property.Name}> on <{type}> to <{typeof(string)}>");
                                        _loggers.Warning($"No converter could convert property <{property.Name}> on <{type}> to <{typeof(string)}>. Skipping");
                                        continue;
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                var message = $"Could not serialize property <{property.Name}> on <{type}>";
                                if (_ignoreFailedConversion)
                                {
                                    _loggers.LogException(LogLevel.Warning, message, ex);
                                }
                                else
                                {
                                    _loggers.LogException(LogLevel.Error, message, ex);
                                    throw;
                                }
                            }                           
                        }
                    }

                    return pairs;
                }
            }

            public object Deserialize(IEnumerable<KeyValuePair<string, string>> pairs, object instance)
            {
                using (_loggers.TraceMethod(this))
                {
                    pairs.ValidateArgument(nameof(pairs));
                    instance.ValidateArgument(nameof(instance));

                    var groupedPairs = pairs.GroupAsDictionary(x => x.Key, x => x.Value);

                    foreach(var propertyProfile in _typeProfiles)
                    {
                        var property = propertyProfile.Key;
                        var key = propertyProfile.Value.Key;
                        var isMerge = propertyProfile.Value.IsMerge;
                        var profile = propertyProfile.Value.Profile;

                        var pairGroup = groupedPairs.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                        
                        if(!pairGroup.Value.HasValue())
                        {
                            _loggers.Debug($"No values to deserialize for property <{property.Name}> on <{_type}>. Skipping");
                            continue;
                        }

                        using(_loggers.TraceAction(LogLevel.Debug, $"Deserializing <{pairGroup.Value.Count}> key/value pairs to property <{property.Name}> on <{_type}>"))
                        {
                            try
                            {
                                if (profile.IsCollection && !_excludedCollectionTypes.Contains(property.PropertyType))
                                {
                                    _loggers.Debug($"Property <{property.Name}> on <{_type}> is a collection. Deserializing elements");
                                    IEnumerable<string> elementsToDeserialize = null;
                                    if (isMerge)
                                    {
                                        _loggers.Debug($"Merge for property <{property.Name}> on <{_type}> is enabled. Getting elements from <{pairGroup.Value.Count}> key/value pairs");
                                        // Filter every value from each pair, split the values and finally filter all the split values
                                        elementsToDeserialize = profile.Filter(pairGroup.Value.Select(x => GetValue(profile, x)).SelectMany(x => profile.ElementSeparator?.Split(x) ?? x.Split(ElementSeparator)), false);
                                    }
                                    else
                                    {
                                        elementsToDeserialize = pairGroup.Value.Select(x => GetValue(profile, x));
                                    }

                                    var deserializedElements = new List<object>();
                                    var elementType = property.PropertyType.GetElementTypeFromCollection();

                                    elementsToDeserialize.Execute((i, element) =>
                                    {
                                        if (element == null) return;
                                        _loggers.Trace($"Deserializing element <{i}> from key/value pairs {key}");

                                        if (profile.Converters.TryConvertTo(element, elementType, out var converted, profile.ConverterArguments))
                                        {
                                            _loggers.Trace($"Deserialized element <{i}> from key/value pairs {key} to <{converted}>");
                                            deserializedElements.Add(converted);
                                        }
                                        else
                                        {
                                            if (!_ignoreFailedConversion) throw new InvalidOperationException($"Could not convert element <{i}> from key/value pairs {key} to <{elementType}>");
                                            _loggers.Warning($"No converter could convert element <{i}> from key/value pairs {key} to <{elementType}>. Skipping");
                                            return;
                                        }
                                    });

                                    // Create typed list
                                    var elements = deserializedElements.CreateList(elementType);

                                    _loggers.Debug($"Deserialized <{elements.Count}> elements from key/value pairs {key}. Converting collection <{elements.GetType()}> to property type <{property.PropertyType}>");
                                    property.SetValue(profile.IsStatic ? null : instance, profile.Converters.ConvertTo(elements, property.PropertyType, profile.ConverterArguments));
                                    continue;
                                }
                                else
                                {
                                    if (groupedPairs.Values.Count > 1) _loggers.Warning($"Property <{property.Name}> on <{_type}> is not a collection but multiple key/value pairs for <{key}> exists. Selecting first to be deserialized");
                                    var value = GetValue(profile, pairGroup.Value.First());

                                    if (profile.Converters.TryConvertTo(value, property.PropertyType, out var converted, profile.ConverterArguments))
                                    {
                                        _loggers.Trace($"Deserialized {value} from key/value pair {key} to <{converted}>");
                                        property.SetValue(profile.IsStatic ? null : instance, converted);
                                    }
                                    else
                                    {
                                        if (!_ignoreFailedConversion) throw new InvalidOperationException($"Could not convert <{value}> from key/value pair {key} to <{property.PropertyType}>");
                                        _loggers.Warning($"No converter could convert <{value}> from key/value pair {key} to <{property.PropertyType}>. Skipping");
                                        continue;
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                var message = $"Could not deserialize <{pairGroup.Value.Count}> key/value pairs to property <{property.Name}> on <{_type}>";
                                if (_ignoreFailedConversion)
                                {
                                    _loggers.LogException(LogLevel.Warning, message, ex);
                                }
                                else
                                {
                                    _loggers.LogException(LogLevel.Error, message, ex);
                                    throw;
                                }
                            }
                        }                       
                    }

                    return instance;
                }
            }

            private KeyValuePair<string, string> CreatePair(PropertySerializationProfile profile, string key, string value)
            {
                return new KeyValuePair<string, string>(key, profile.Filter(value, true));
            } 

            private string GetValue(PropertySerializationProfile profile, string value)
            {
                return profile.Filter(value, false);
            }
        }

        private class KeyValueSerializerConfiguration : BaseSerializerConfigurator<IKeyValueSerializerConfigurator>, IKeyValueSerializerConfigurator
        {
            // Fields
            private Func<string, IEnumerable<string>> _rowSplitter = RowSplitter;
            private Func<IEnumerable<string>, string> _rowJoiner = RowJoiner;
            private Func<string, KeyValuePair<string, string>> _toPairFunc = ToPair;
            private Func<KeyValuePair<string, string>, string> _toStringFunc = ToPairString;
            private Predicate<string> _keyValuePairChecker = IsValidPair;

            // Properties
            public bool UseDefaultConverter { get;}
            public bool IgnoreFailedConversion { get; }
            public override IKeyValueSerializerConfigurator Instance => this;

            public KeyValueSerializerConfiguration(KeyValueSerializerSettings settings)
            {
                IgnoreFailedConversion = settings.HasFlag(KeyValueSerializerSettings.IgnoreUnconvertable);
                UseDefaultConverter = settings.HasFlag(KeyValueSerializerSettings.UseDefaultConverter);
            }

            #region Configuration
            public IKeyValueSerializerConfigurator SplitAndJoinRowsUsing(Func<string, IEnumerable<string>> splitter, Func<IEnumerable<string>, string> joiner)
            {
                using (_loggers.TraceMethod(this))
                {
                    splitter.ValidateArgument(nameof(splitter));
                    joiner.ValidateArgument(nameof(joiner));

                    _rowSplitter = splitter;
                    _rowJoiner = joiner;

                    return this;
                }
            }

            public IKeyValueSerializerConfigurator SplitAndJoinRowsUsing(object splitAndJoinValue)
            {
                using (_loggers.TraceMethod(this))
                {
                    splitAndJoinValue.ValidateArgument(nameof(splitAndJoinValue));
                    var value = splitAndJoinValue.ToString();
                    value.ValidateArgumentNotNullOrEmpty(nameof(value));

                    _rowSplitter = x => x.Split(value, StringSplitOptions.RemoveEmptyEntries);
                    _rowJoiner = x => x.JoinString(value);

                    return this;
                }
            }

            public IKeyValueSerializerConfigurator ConvertKeyValuePairsUsing(Func<string, KeyValuePair<string, string>> toPairFunc, Func<KeyValuePair<string, string>, string> toStringFunc, Predicate<string> isValidPairPredicate)
            {
                using (_loggers.TraceMethod(this))
                {
                    toPairFunc.ValidateArgument(nameof(toPairFunc));
                    toStringFunc.ValidateArgument(nameof(toStringFunc));
                    isValidPairPredicate.ValidateArgument(nameof(isValidPairPredicate));

                    _toPairFunc = toPairFunc;
                    _toStringFunc = toStringFunc;
                    _keyValuePairChecker = isValidPairPredicate;

                    return this;
                }
            }

            public IKeyValueSerializerConfigurator ConvertKeyValuePairUsing(object splitAndJoinValue)
            {
                using (_loggers.TraceMethod(this))
                {
                    splitAndJoinValue.ValidateArgument(nameof(splitAndJoinValue));
                    var value = splitAndJoinValue.ToString();
                    value.ValidateArgumentNotNullOrEmpty(nameof(value));

                    _toPairFunc = x => { var key = x.SplitOnFirst(value, out var other); return new KeyValuePair<string, string>(key, other); };
                    _toStringFunc = x => Helper.Strings.JoinStrings(value, x.Key, x.Value);
                    _keyValuePairChecker = x => x.Contains(value);

                    return this;
                }
            }           
            #endregion

            public IEnumerable<KeyValuePair<string, string>> GetPairs(string source)
            {
                using (_loggers.TraceMethod(this))
                {
                    _loggers.Debug($"Splitting source string of length <{source.Length}> into key/value pairs");
                    _loggers.Trace($"Splitting source string <{source}> into key/value pairs");
                    if (source.HasValue())
                    {
                        string lastValidRow = null;
                        List<string> nonPairRows = new List<string>(); 

                        foreach(var row in _rowSplitter(source))
                        {
                            if (_keyValuePairChecker(row))
                            {
                                if(lastValidRow != null)
                                {
                                    yield return ConvertRowsToPair(lastValidRow, nonPairRows);                                    
                                }
                                lastValidRow = row;
                            }
                            else
                            {
                                nonPairRows.Add(row);
                            }
                        }

                        if(lastValidRow != null)
                        {
                            yield return ConvertRowsToPair(lastValidRow, nonPairRows);
                        }
                    }
                }
            }

            public string GetString(IEnumerable<KeyValuePair<string, string>> pairs)
            {
                using (_loggers.TraceMethod(this))
                {
                    _loggers.Debug($"Joining <{pairs.GetCount()}> key/value pairs");

                    return _rowJoiner(pairs.Select(x => _toStringFunc(x)));
                }
            }

            private KeyValuePair<string, string> ConvertRowsToPair(string row, List<string> nonPairRows)
            {
                using (_loggers.TraceMethod(this))
                {
                    nonPairRows.Insert(0, row);
                    var pair = _toPairFunc(_rowJoiner(nonPairRows));
                    nonPairRows.Clear();
                    _loggers.Trace($"Created pair <{pair.Key}/{pair.Value}>");
                    return pair;
                }                
            }


            private static IEnumerable<string> RowSplitter(string source)
            {
                return source.SplitOnNewLine();
            }

            private static string RowJoiner(IEnumerable<string> rows)
            {
                return rows.JoinStringNewLine();
            }

            private static KeyValuePair<string, string> ToPair(string pair)
            {
                var key = pair.SplitOnFirst(KeyValueSerializer.KeyValueSeparator, out var value);
                return new KeyValuePair<string, string>(key.Trim(), value?.Trim() ?? string.Empty);
            }

            private static string ToPairString(KeyValuePair<string, string> pair)
            {
                return $"{pair.Key.Trim()}{KeyValueSerializer.KeyValueSeparator}{pair.Value?.Trim()}";
            }

            private static bool IsValidPair(string pair)
            {
                return pair.Contains(KeyValueSerializer.KeyValueSeparator);
            }
        }
    }

    /// <summary>
    /// Exposes extra configuration for <see cref="KeyValueSerializer"/>.
    /// </summary>
    public interface IKeyValueSerializerConfigurator : ISerializerConfigurator<IKeyValueSerializerConfigurator>
    {
        /// <summary>
        /// Defines delegates for splitting the string to deserialize into multiple key/value pairs and joining the serialized key/value pairs.
        /// </summary>
        /// <param name="splitter">The delegate that will split the string into multiple key value pairs</param>
        /// <param name="joiner">The delegate that will join the key/value pairs into a string</param>
        /// <returns>Current configurator for method chaining</returns>
        IKeyValueSerializerConfigurator SplitAndJoinRowsUsing(Func<string, IEnumerable<string>> splitter, Func<IEnumerable<string>, string> joiner);
        /// <summary>
        /// Key/value pairs will be split/joined using the <see cref="object.ToString()"/> on <paramref name="splitAndJoinValue"/>.
        /// </summary>
        /// <param name="splitAndJoinValue">The object to get the string to join/split from</param>
        /// <returns>Current configurator for method chaining</returns>
        IKeyValueSerializerConfigurator SplitAndJoinRowsUsing(object splitAndJoinValue);
        /// <summary>
        /// Defines delegates for converting between <see cref="KeyValuePair{TKey, TValue}"/> and their string representation.
        /// </summary>
        /// <param name="toPairFunc">The delegate that will create a <see cref="KeyValuePair{TKey, TValue}"/> from the string representation</param>
        /// <param name="toStringFunc">The delegate that will create the string representation for a <see cref="KeyValuePair{TKey, TValue}"/></param>
        /// <param name="isValidPairPredicate">The delegate that checks if the string is a valid key/value pair</param>
        /// <returns>Current configurator for method chaining</returns>
        IKeyValueSerializerConfigurator ConvertKeyValuePairsUsing(Func<string, KeyValuePair<string, string>> toPairFunc, Func<KeyValuePair<string, string>, string> toStringFunc, Predicate<string> isValidPairPredicate);
        /// <summary>
        /// Key/value strings will be split/joined using the <see cref="object.ToString()"/> on <paramref name="splitAndJoinValue"/>.
        /// </summary>
        /// <param name="splitAndJoinValue">The object to get the string to join/split from</param>
        /// <returns>Current configurator for method chaining</returns>
        IKeyValueSerializerConfigurator ConvertKeyValuePairUsing(object splitAndJoinValue);

    }

    /// <summary>
    /// Exposes extra settings for <see cref="KeyValueSerializer"/>.
    /// </summary>
    [Flags]
    public enum KeyValueSerializerSettings
    {
        /// <summary>
        /// No selected settings.
        /// </summary>
        None = 0,
        /// <summary>
        /// If no exceptions should be thrown when either the conversion for a property fails or when no converters are available to do the conversion. 
        /// </summary>
        IgnoreUnconvertable = 1,
        /// <summary>
        /// If <see cref="GenericConverter.DefaultConverter"/> should be added as a converter.
        /// </summary>
        UseDefaultConverter = 2
    }
}
