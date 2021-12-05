using Sels.Core.Components.Conversion;
using Sels.Core.Components.Serialization.Table.Attributes;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Contracts.Serialization;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sels.Core.Components.Serialization.Table
{
    /// <summary>
    /// Serializer that splits up a string into rows and columns and then serializes/deserializes each row.
    /// </summary>
    public class TableSerializer
    {
        // Fields
        private readonly IGenericTypeConverter _defaultConverter;

        // Properties
        /// <summary>
        /// Splitter used to split a string into rows.
        /// </summary>
        public string RowSplitter { get; }
        /// <summary>
        /// Splitter used to split a row into multiple columns. If null or empty columns are split on white space characters.
        /// </summary>
        public string ColumnSplitter { get; }
        /// <summary>
        /// How many rows are skipped before serializing/deserializing. Can be used to skip the header row.
        /// </summary>
        public int SkipDataRow { get; }
        /// <summary>
        /// How many of the last rows are skipped when serializing/deserializing.
        /// </summary>
        public int SkipLastDataRow { get; }

        public TableSerializer(string rowSplitter = null, string columnSplitter = null, int skipDataRow = 0, int skipLastDataRow = 0) : this(GenericConverter.DefaultConverter, rowSplitter, columnSplitter, skipDataRow, skipLastDataRow)
        {

        }

        public TableSerializer(IGenericTypeConverter defaultConverter, string rowSplitter = null, string columnSplitter = null, int skipDataRow = 0, int skipLastDataRow = 0)
        {
            _defaultConverter = defaultConverter.ValidateArgument(nameof(defaultConverter));
            RowSplitter = rowSplitter.HasValue() ? rowSplitter : DefaultRowSplitter;
            ColumnSplitter = columnSplitter.HasValue() ? columnSplitter : DefaultColumnSplitter;
            SkipDataRow = skipDataRow.ValidateArgumentLargerOrEqual(nameof(skipDataRow), 0);
            SkipLastDataRow = skipLastDataRow.ValidateArgumentLargerOrEqual(nameof(skipLastDataRow), 0);
        }

        #region Serialize
        /// <summary>
        /// Serializes all <paramref name="values"/> to a string.
        /// </summary>
        /// <param name="values">Objects to serialize</param>
        /// <param name="type">Type of object to serialize</param>
        /// <returns>Serialized <paramref name="values"/></returns>
        public string Serialize(IEnumerable<object> values, Type type)
        {
            values.ValidateArgument(nameof(values));
            type.ValidateArgument(nameof(type));

            var profile = BuildSerializerProfile(type);

            if (profile.HasValue())
            {
                var builder = new StringBuilder();
                var objects = values.ToArray();

                for (int i = 0; i < objects.Length; i++)
                {
                    var value = objects[i];
                    var columns = new string[profile.Max(x => x.ColumnIndex)];

                    // Build up columns for object
                    foreach(var propertyProfile in profile)
                    {
                        var converter = propertyProfile.Converter;
                        var property = propertyProfile.Property;
                        var propertyValue = property.GetValue(value);

                        if (converter.CanConvert(property.PropertyType, typeof(string), propertyValue))
                        {
                            columns[propertyProfile.ColumnIndex] = converter.ConvertTo(property.PropertyType, typeof(string), propertyValue).ToString();
                        }
                    }

                    // Add columns to builder
                    builder.Append(columns.JoinString(ColumnSplitter));

                    // Append row splitter
                    if(i != objects.Length - 1)
                    {
                        var joinValue = RowSplitter.HasValue() ? RowSplitter : Constants.Strings.Space;
                        builder.Append(joinValue);
                    }
                }

                return builder.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Serializes all <paramref name="values"/> to a string.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="values">Objects to serialize</param>
        /// <returns>Deserialized objects</returns>
        public string Serialize<T>(IEnumerable<T> values)
        {
            return Serialize(values.Select(x => x.As<object>()), typeof(T));
        }
        #endregion

        #region Deserialize
        /// <summary>
        /// Deserializes all rows in <paramref name="value"/> to objects of Type <paramref name="type"/>.
        /// </summary>
        /// <param name="value">String to deserialize</param>
        /// <param name="type">Type of object to deserialize to</param>
        /// <returns>Deserialized objects</returns>
        public IEnumerable<object> Deserialize(string value, Type type)
        {
            value.ValidateArgumentNotNullOrWhitespace(nameof(value));
            type.ValidateArgument(nameof(type));
            type.ValidateArgumentCanBeContructedWith(nameof(type));

            var profile = BuildSerializerProfile(type);

            if (profile.HasValue())
            {
                var rows = GetRows(value);

                if (rows.HasValue())
                {
                    foreach(var row in rows)
                    {
                        var newObject = type.Construct();

                        var columns = GetColumns(row);

                        // Loop over each property profile and try assigning the properties

                        foreach(var propertyProfile in profile)
                        {
                            if(columns.Length > propertyProfile.ColumnIndex)
                            {
                                var columnValue = columns[propertyProfile.ColumnIndex];
                                var converter = propertyProfile.Converter;
                                var property = propertyProfile.Property;

                                if (converter.CanConvert(columnValue.GetType(), property.PropertyType, columnValue))
                                {
                                    propertyProfile.Property.SetValue(newObject, converter.ConvertTo(columnValue.GetType(), property.PropertyType, columnValue));
                                }
                            }
                        }

                        yield return newObject;
                    }
                }
            }
        }

        /// <summary>
        /// Deserializes all rows in <paramref name="value"/> to objects of Type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="value">String to deserialize</param>
        /// <returns>Deserialized objects</returns>
        public IEnumerable<T> Deserialize<T>(string value) where T : new()
        {
            foreach(var item in Deserialize(value, typeof(T)))
            {
                yield return item.As<T>();
            }
        }
        #endregion

        private (int ColumnIndex, IGenericTypeConverter Converter, PropertyInfo Property)[] BuildSerializerProfile(Type type)
        {
            var propertyProfiles = new List<(int ColumnIndex, IGenericTypeConverter Converter, PropertyInfo property)>();

            foreach(var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var attribute = property.GetCustomAttribute<TableColumnAttribute>();

                if (attribute.HasValue())
                {
                    propertyProfiles.Add((attribute.ColumnIndex, attribute.Converter ?? _defaultConverter, property));
                }
            }

            return propertyProfiles.ToArray();
        }
        
        private string[] GetRows(string table)
        {
            var rows = table.Split(RowSplitter, StringSplitOptions.RemoveEmptyEntries).ToArray();
            return rows.Skip(SkipDataRow).Take(rows.Length - SkipDataRow - SkipLastDataRow).ToArray();
        }

        private string[] GetColumns(string row)
        {
            return ColumnSplitter.HasValue() ? row.Split(ColumnSplitter, StringSplitOptions.RemoveEmptyEntries) : row.Split().Where(x => x.HasValue()).ToArray();
        }

        // Statics
        /// <summary>
        /// Default splitter used to split a string into rows.
        /// </summary>
        public readonly static string DefaultRowSplitter = Environment.NewLine;
        /// <summary>
        /// Default splitter used to split a row into multiple columns. If null or empty columns are split on white space characters.
        /// </summary>
        public readonly static string DefaultColumnSplitter = string.Empty;
    }
}
