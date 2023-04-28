using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Attributes.Table;
using Sels.Core.Conversion.Components.Serialization.Profile;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Templates;
using Sels.Core.Conversion.Templates.Table;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sels.Core.Conversion.Serializers.Table
{
    /// <summary>
    /// Serializer that divides a string into columns and rows where the header row text/index determines what property is used.
    /// </summary>
    public class TableSerializer
    {
        // Fields
        private readonly IEnumerable<ILogger> _loggers;
        private readonly static Type[] _excludedCollectionTypes = new Type[] { typeof(string) };

        // Constants
        /// <summary>
        /// The default substring that is used to separate columns.
        /// </summary>
        public const string ColumnSeparator = ";";
        /// <summary>
        /// The default substring that will be used to join/split rows.
        /// </summary>
        public static string RowSeparator => Environment.NewLine;
        /// <summary>
        /// The default substring that will be used to join/split elements.
        /// </summary>
        public const string ElementSeparator = ",";
        private const string HasHeaderRowArg = "HasHeaderRow";

        // Fields
        private readonly TableSerializerConfiguration _configuration;

        /// <inheritdoc cref="TableSerializer"/>
        /// <param name="settings">Optional settings for this serializer</param>
        public TableSerializer(TableSerializerSettings settings = TableSerializerSettings.UseDefaultConverter) : this(x => { }, settings)
        {

        }
        /// <inheritdoc cref="TableSerializer"/>
        /// <param name="configurator">Delegate for configuring this serializer</param>
        /// <param name="settings">Optional settings for this serializer</param>
        public TableSerializer(Action<ITableSerializerConfigurator> configurator, TableSerializerSettings settings = TableSerializerSettings.UseDefaultConverter)
        {
            configurator.ValidateArgument(nameof(configurator));

            _configuration = new TableSerializerConfiguration(settings);
            configurator(_configuration);
            if (_configuration.UseDefaultConverter) _configuration.UseConverters(GenericConverter.DefaultConverter);

            _loggers = _configuration.Loggers;
        }

        /// <summary>
        /// Deserializes the rows and columns in <paramref name="value"/> to an instance of type <paramref name="type"/>.
        /// </summary>
        /// <param name="value">The string containing the rows and columns</param>
        /// <param name="type">The type to deserialize to</param>
        /// <param name="hasHeaderRow">If <paramref name="value"/> has a header row to take into account</param>
        /// <returns>The instance deserialized from <paramref name="value"/></returns>
        public object Deserialize(string value, Type type, bool hasHeaderRow = true)
        {
            using (_loggers.TraceMethod(this))
            {
                value.ValidateArgumentNotNullOrWhitespace(nameof(value));
                type.ValidateArgument(nameof(type));

                using (_loggers.TraceAction($"Deserializing rows and columns to an instance of type <{type}>"))
                {
                    var grid = GetTable(value);
                    if(grid.GetLength(0) <=1)
                    {
                        _loggers.Warning($"No rows to deserialize from. Skipping");
                        return type.GetDefaultValue();
                    }
                    return DeserializeTo(grid, type, hasHeaderRow);
                }
            }
        }
        /// <summary>
        /// Deserializes the rows and columns in <paramref name="value"/> to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="value">The string containing the rows and columns</param>
        /// <param name="hasHeaderRow">If <paramref name="value"/> has a header row to take into account</param>
        /// <returns>The instance deserialized from <paramref name="value"/></returns>
        public T Deserialize<T>(string value, bool hasHeaderRow = true)
        {
            using (_loggers.TraceMethod(this))
            {
                value.ValidateArgument(nameof(value));

                return Deserialize(value, typeof(T), hasHeaderRow).CastToOrDefault<T>();
            }
        }

        /// <summary>
        /// Serializes <paramref name="instance"/> to a string containing key/value pairs.
        /// </summary>
        /// <param name="instance">The instance to serialize</param>
        /// <param name="includeHeaderRow">If the header row should be included in the serialized string</param>
        /// <returns>String containing key/value pairs representing <paramref name="instance"/></returns>
        public string Serialize(object instance, bool includeHeaderRow = true)
        {
            using (_loggers.TraceMethod(this))
            {
                instance.ValidateArgument(nameof(instance));
                var type = instance.GetType();

                using (_loggers.TraceAction($"Serializing instance of type <{type}> to rows and columns"))
                {
                    var grid = SerializeFrom(instance);
                    return GetString(grid, includeHeaderRow);
                }
            }
        }

        private string[,] GetTable(string source)
        {
            using (_loggers.TraceMethod(this))
            {
                source.ValidateArgumentNotNullOrWhitespace(nameof(source));

                return source.ToGrid(_configuration.RowSplitter, _configuration.ColumnSplitter, true, true);
            }
        }

        private string GetString(string[,] grid, bool includeHeaderRow)
        {
            using (_loggers.TraceMethod(this))
            {
                grid.ValidateArgument(nameof(grid));
                var rows = new List<string>();

                for(int i = 0; i < grid.GetLength(0); i++)
                {
                    if (i == 0 && !includeHeaderRow) continue;

                    rows.Add(_configuration.ColumnJoiner(grid.GetRow(i).Select(x => x.HasValue() ? x : string.Empty)) ?? throw new InvalidOperationException($"Columns joiner returned null"));
                }

                return _configuration.RowJoiner(rows) ?? throw new InvalidOperationException($"Row joiner returned null");
            }
        }

        private IEnumerable<object> Deserialize(Type type, string[,] grid, bool hasHeaderRow)
        {
            using (_loggers.TraceMethod(this))
            {
                type.ValidateArgument(nameof(type));
                type.ValidateArgumentCanBeContructedWithArguments(nameof(type));
                grid.ValidateArgument(nameof(grid));

                _loggers.Debug($"Starting to deserialize {grid.Length} rows to instances of type <{type}>");

                // Build property profiles
                var headerRow = grid.GetRow(0);
                var deserializationProperties = GetPropertiesToDeserialize(type, headerRow);
                
                var startIndex = hasHeaderRow ? 1 : 0;
                // Deserialize to new instances
                for (int i = startIndex; i < grid.GetLength(0); i++)
                {
                    var instance = type.Construct();
                    _loggers.Debug($"Deserializing row {i} to a new instance of type <{type}>");
                    foreach (var propertyProfile in deserializationProperties)
                    {
                        var property = propertyProfile.Property;
                        var column = propertyProfile.ColumnIndex;
                        var profile = propertyProfile.Profile;
                        var value = grid[i, column];

                        if(value == null)
                        {
                            _loggers.Debug($"Value from column {column} in row {i} is null. Ignoring");
                            continue;
                        }

                        value = profile.Filter(value, false);

                        using (_loggers.TraceAction(LogLevel.Debug, $"Deserializing property <{property.Name}> to a new instance of type <{type}>"))
                        {
                            try
                            {
                                if (profile.IsCollection && !_excludedCollectionTypes.Contains(property.PropertyType))
                                {
                                    _loggers.Debug($"Property <{property.Name}> on <{type}> is a collection. Deserializing elements");
                                    IEnumerable<string> elementsToDeserialize = profile.Filter(profile.ElementSeparator?.Split(value) ?? value.Split(ElementSeparator), false);

                                    var deserializedElements = new List<object>();
                                    var elementType = property.PropertyType.GetElementTypeFromCollection();

                                    elementsToDeserialize.Execute((e, element) =>
                                    {
                                        if (element == null) return;
                                        _loggers.Trace($"Deserializing element <{e}> from column {column} in row {i}");

                                        if (profile.Converters.TryConvertTo(element, elementType, out var converted, profile.ConverterArguments))
                                        {
                                            _loggers.Trace($"Deserialized element <{e}> from column {column} in row {i} to <{converted}>");
                                            deserializedElements.Add(converted);
                                        }
                                        else
                                        {
                                            if (!_configuration.IgnoreFailedConversion) throw new InvalidOperationException($"Could not convert element <{e}> from column {column} in row {i} to <{elementType}>");
                                            _loggers.Warning($"No converter could convert element <{e}> from column {column} in row {i} to <{elementType}>. Skipping");
                                            return;
                                        }
                                    });

                                    // Create typed list
                                    var elements = deserializedElements.CreateList(elementType);

                                    _loggers.Debug($"Deserialized <{elements.Count}> elements from column {column} in row {i}. Converting collection <{elements.GetType()}> to property type <{property.PropertyType}>");
                                    property.SetValue(profile.IsStatic ? null : instance, profile.Converters.ConvertTo(elements, property.PropertyType, profile.ConverterArguments));
                                    continue;
                                }
                                else
                                {
                                    if (profile.Converters.TryConvertTo(value, property.PropertyType, out var converted, profile.ConverterArguments))
                                    {
                                        _loggers.Trace($"Deserialized {value} from column {column} in row {i} to <{converted}>");
                                        property.SetValue(profile.IsStatic ? null : instance, converted);
                                    }
                                    else
                                    {
                                        if (!_configuration.IgnoreFailedConversion) throw new InvalidOperationException($"Could not convert <{value}> from column {column} in row {i} to <{property.PropertyType}>");
                                        _loggers.Warning($"No converter could convert <{value}> from column {column} in row {i} to <{property.PropertyType}>. Skipping");
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var message = $"Could not deserialize column {column} in row {i} to property <{property.Name}> on <{type}>";
                                if (_configuration.IgnoreFailedConversion)
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
                    yield return instance;
                }               
            }
        }

        private string[,] Serialize(Type type, IEnumerable<object> instances)
        {
            using (_loggers.TraceMethod(this))
            {
                type.ValidateArgument(nameof(type));
                instances.ValidateArgument(nameof(instances));

                var instanceArray = instances.ToArray();
                _loggers.Debug($"Starting to serialize {instanceArray.Length} instances of type <{type}>");

                // Build property profiles
                var serializationProperties = GetPropertiesToSerialize(type).OrderByDescending(x => x.ColumnIndex > 0).ThenBy(x => x.ColumnIndex).ToArray();
                var grid = new string[instanceArray.Length+1, serializationProperties.Length];

                // Create header row
                for (int i = 0; i < serializationProperties.Length; i++)
                {
                    grid[0, i] = serializationProperties[i].Header;
                }

                // Serialize instances to grid
                for (int i = 0; i < instanceArray.Length; i++)
                {
                    var instance = instanceArray[i];
                    _loggers.Debug($"Serializing instance {i}");
                    for(int y = 0; y < serializationProperties.Length; y++)
                    {
                        var property = serializationProperties[y].Property;
                        var column = serializationProperties[y].ColumnIndex;
                        var header = serializationProperties[y].Header;
                        var profile = serializationProperties[y].Profile;

                        string value = null;

                        try
                        {
                            using (_loggers.TraceAction(LogLevel.Debug, $"Serializing property <{property.Name}> on <{type}> to column {y} in row {i}"))
                            {
                                var propertyValue = property.GetValue(profile.IsStatic ? null : instance);
                                if (propertyValue == null)
                                {
                                    _loggers.Debug($"Property <{property.Name}> on instance {i} <{type}> is null. Skipping");
                                    continue;
                                }

                                if (profile.IsCollection && !_excludedCollectionTypes.Contains(property.PropertyType))
                                {
                                    _loggers.Debug($"Property <{property.Name}> on <{type}> is a collection. Serializing elements");
                                    var valueCollection = propertyValue.CastTo<IEnumerable>();
                                    List<string> serializedElements = new List<string>();

                                    valueCollection.Enumerate().Where(x => x != null).Execute((e, element) =>
                                    {
                                        if (element == null) return;
                                        _loggers.Trace($"Serializing element <{e}> from property <{property.Name}> on instance {i} <{type}>");
                                        if (profile.Converters.TryConvertTo<string>(element, out var converted, profile.ConverterArguments))
                                        {
                                            _loggers.Trace($"Serialized element <{e}> from property <{property.Name}> on instance {i} <{type}> to <{converted}>");
                                            serializedElements.Add(converted);
                                        }
                                        else
                                        {
                                            if (!_configuration.IgnoreFailedConversion) throw new InvalidOperationException($"Could not convert element <{e}> from property <{property.Name}> on instance {i} <{type}> to <{typeof(string)}>");
                                            _loggers.Warning($"No converter could convert element <{e}> from property <{property.Name}> on instance {i} <{type}> to <{typeof(string)}>. Skipping");
                                            return;
                                        }
                                    });

                                    // Filter elements
                                    var elements = profile.Filter(serializedElements, true);
                                    value = profile.ElementSeparator?.Join(elements) ?? elements.JoinString(ElementSeparator);
                                }
                                else
                                {
                                    if (profile.Converters.TryConvertTo<string>(propertyValue, out var converted, profile.ConverterArguments))
                                    {
                                        _loggers.Trace($"Serialized property <{property.Name}> on instance {i} <{type}> to <{converted}>");
                                        value = converted;
                                    }
                                    else
                                    {
                                        if (!_configuration.IgnoreFailedConversion) throw new InvalidOperationException($"Could not convert property <{property.Name}> on instance {i} <{type}> to <{typeof(string)}>");
                                        _loggers.Warning($"No converter could convert property <{property.Name}> on instance {i}  <{type}> to <{typeof(string)}>. Skipping");
                                        continue;
                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            var message = $"Could not serialize property <{property.Name}> on instance {i} <{type}>";
                            if (_configuration.IgnoreFailedConversion)
                            {
                                _loggers.LogException(LogLevel.Warning, message, ex);
                            }
                            else
                            {
                                _loggers.LogException(LogLevel.Error, message, ex);
                                throw;
                            }
                        }

                        grid[i+1, y] = profile.Filter(value, true);
                    }                   
                }

                return grid;
            }
        }

        private IEnumerable<(PropertyInfo Property, int ColumnIndex, PropertySerializationProfile Profile)> GetPropertiesToDeserialize(Type type, string[] headerRow)
        {
            using (_loggers.TraceMethod(this))
            {
                type.ValidateArgument(nameof(type));
                headerRow.ValidateArgument(nameof(headerRow));

                _loggers.Debug($"Determining the properties to deserialize to on type <{type}>");
                var deserializationProperties = new List<(PropertyInfo Property, int columnIndex, PropertySerializationProfile profile)>();
                var typeProfile = new SerializationProfile(type, _configuration.PropertyFlags, _configuration.Converters, _configuration.Filters, _configuration.ElementFilters, _configuration.Loggers);

                foreach(var propertyProfile in typeProfile.PropertyProfiles.Where(x => x.Key.CanWrite))
                {
                    var attribute = propertyProfile.Key.GetColumnAttribute();
                    var columnIndex = attribute != null ? attribute.GetColumnIndex(headerRow) : Array.IndexOf(headerRow, headerRow.FirstOrDefault(x => x.Equals(propertyProfile.Key.Name, StringComparison.OrdinalIgnoreCase)));

                    if(columnIndex < 0)
                    {
                        _loggers.Warning($"Property <{propertyProfile.Key.Name}> on type <{type}> could not find a column to deserialize from using header row <{headerRow.JoinString('|')}>. Skipping");
                    }

                    deserializationProperties.Add((propertyProfile.Key, columnIndex, propertyProfile.Value));
                }

                return deserializationProperties;
            }
        }

        private IEnumerable<(PropertyInfo Property, int ColumnIndex, string Header, PropertySerializationProfile Profile)> GetPropertiesToSerialize(Type type)
        {
            using (_loggers.TraceMethod(this))
            {
                type.ValidateArgument(nameof(type));

                _loggers.Debug($"Determining the properties to serialize to on type <{type}>");
                var deserializationProperties = new List<(PropertyInfo Property, int ColumnIndex, string Header, PropertySerializationProfile Profile)>();
                var typeProfile = new SerializationProfile(type, _configuration.PropertyFlags, _configuration.Converters, _configuration.Filters, _configuration.ElementFilters, _configuration.Loggers);

                foreach (var propertyProfile in typeProfile.PropertyProfiles.Where(x => x.Key.CanRead))
                {
                    var header = propertyProfile.Key.GetCustomAttribute<ColumnHeaderAttribute>()?.Header ?? propertyProfile.Key.Name;
                    var columnIndex = propertyProfile.Key.GetCustomAttribute<ColumnIndexAttribute>()?.Index ?? -1;

                    deserializationProperties.Add((propertyProfile.Key, columnIndex, header, propertyProfile.Value));
                }

                return deserializationProperties;
            }
        }

        private object DeserializeTo(string[,] grid, Type type, bool hasHeaderRow)
        {
            if (type.IsContainer())
            {
                var targetType = type;
                var elementType = targetType.GetElementTypeFromCollection();
                var objects = Deserialize(elementType, grid, hasHeaderRow).CreateList(elementType);
                return GenericConverter.DefaultCollectionConverter.ConvertTo(objects, targetType);
            }
            else
            {
                return Deserialize(type, grid, hasHeaderRow).First();
            }
        }

        private string[,] SerializeFrom(object instance)
        {
            var type = instance.GetType();

            if (type.IsContainer())
            {
                return Serialize(type.GetElementTypeFromCollection(), instance.CastTo<IEnumerable>().Enumerate());
            }
            else
            {
                return Serialize(type, instance.AsEnumerable());
            }
        }

        private class TableSerializerConfiguration : BaseSerializerConfigurator<ITableSerializerConfigurator>, ITableSerializerConfigurator
        {
            // Properties
            public bool UseDefaultConverter { get; }
            public bool IgnoreFailedConversion { get; }
            public override ITableSerializerConfigurator Instance => this;
            public Func<string, IEnumerable<string>> ColumnSplitter { get; private set; }
            public Func<IEnumerable<string>, string> ColumnJoiner { get; private set; }
            public Func<string, IEnumerable<string>> RowSplitter { get; private set; }
            public Func<IEnumerable<string>, string> RowJoiner { get; private set; }

            public TableSerializerConfiguration(TableSerializerSettings settings)
            {
                IgnoreFailedConversion = settings.HasFlag(TableSerializerSettings.IgnoreUnconvertable);
                UseDefaultConverter = settings.HasFlag(TableSerializerSettings.UseDefaultConverter);

                // Default config
                SplitAndJoinColumnsUsing(ColumnSeparator);
                SplitAndJoinRowsUsing(RowSeparator);
            }

            public ITableSerializerConfigurator SplitAndJoinColumnsUsing(object splitAndJoinValue)
            {
                using (_loggers.TraceMethod(this))
                {
                    splitAndJoinValue.ValidateArgument(nameof(splitAndJoinValue));
                    var value = splitAndJoinValue.ToString();
                    value.ValidateArgumentNotNullOrEmpty(nameof(value));

                    ColumnSplitter = x => x.Split(value, StringSplitOptions.RemoveEmptyEntries);
                    ColumnJoiner = x => x.JoinString(value);

                    return this;
                }
            }

            public ITableSerializerConfigurator SplitAndJoinColummsUsing(Func<string, IEnumerable<string>> splitter, Func<IEnumerable<string>, string> joiner)
            {
                using (_loggers.TraceMethod(this))
                {
                    splitter.ValidateArgument(nameof(splitter));
                    joiner.ValidateArgument(nameof(joiner));

                    ColumnSplitter = splitter;
                    ColumnJoiner = joiner;

                    return this;
                }
            }

            public ITableSerializerConfigurator SplitAndJoinRowsUsing(Func<string, IEnumerable<string>> splitter, Func<IEnumerable<string>, string> joiner)
            {
                using (_loggers.TraceMethod(this))
                {
                    splitter.ValidateArgument(nameof(splitter));
                    joiner.ValidateArgument(nameof(joiner));

                    RowSplitter = splitter;
                    RowJoiner = joiner;

                    return this;
                }
            }

            public ITableSerializerConfigurator SplitAndJoinRowsUsing(object splitAndJoinValue)
            {
                using (_loggers.TraceMethod(this))
                {
                    splitAndJoinValue.ValidateArgument(nameof(splitAndJoinValue));
                    var value = splitAndJoinValue.ToString();
                    value.ValidateArgumentNotNullOrEmpty(nameof(value));

                    RowSplitter = x => x.Split(value, StringSplitOptions.RemoveEmptyEntries);
                    RowJoiner = x => x.JoinString(value);

                    return this;
                }
            }
        }
    }

    /// <summary>
    /// Exposes extra configuration for <see cref="TableSerializer"/>.
    /// </summary>
    public interface ITableSerializerConfigurator : ISerializerConfigurator<ITableSerializerConfigurator>
    {
        /// <summary>
        /// Defines delegates for splitting the string into multiple columns and joining multiple columns.
        /// </summary>
        /// <param name="splitter">The delegate that will split the string into multiple columns</param>
        /// <param name="joiner">The delegate that will join the columns into a string</param>
        /// <returns>Current configurator for method chaining</returns>
        ITableSerializerConfigurator SplitAndJoinColummsUsing(Func<string, IEnumerable<string>> splitter, Func<IEnumerable<string>, string> joiner);
        /// <summary>
        /// Columns will be split/joined using the <see cref="object.ToString()"/> on <paramref name="splitAndJoinValue"/>.
        /// </summary>
        /// <param name="splitAndJoinValue">The object to get the string to join/split from</param>
        /// <returns>Current configurator for method chaining</returns>
        ITableSerializerConfigurator SplitAndJoinColumnsUsing(object splitAndJoinValue);
        /// <summary>
        /// Defines delegates for splitting the string to deserialize into multiple rows and joining the serialized rows.
        /// </summary>
        /// <param name="splitter">The delegate that will split the string into multiple rows</param>
        /// <param name="joiner">The delegate that will join the rows into a string</param>
        /// <returns>Current configurator for method chaining</returns>
        ITableSerializerConfigurator SplitAndJoinRowsUsing(Func<string, IEnumerable<string>> splitter, Func<IEnumerable<string>, string> joiner);
        /// <summary>
        /// Rows will be split/joined using the <see cref="object.ToString()"/> on <paramref name="splitAndJoinValue"/>.
        /// </summary>
        /// <param name="splitAndJoinValue">The object to get the string to join/split from</param>
        /// <returns>Current configurator for method chaining</returns>
        ITableSerializerConfigurator SplitAndJoinRowsUsing(object splitAndJoinValue);
    }

    /// <summary>
    /// Exposes extra settings for <see cref="TableSerializer"/>.
    /// </summary>
    [Flags]
    public enum TableSerializerSettings
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
