using Sels.Core.Conversion;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Conversion.Converters
{
    /// <summary>
    /// Converter that can be configured with other converters. Converter will use first sub converter that can convert between the supplied types. 
    /// </summary>
    public class GenericConverter : BaseTypeConverter
    {
        // Fields
        private readonly List<ITypeConverter> _converters = new List<ITypeConverter>();

        // Properties
        /// <summary>
        /// Current converters used by this converter.
        /// </summary>
        public ITypeConverter[] Converters => _converters.ToArray();
        /// <summary>
        /// Current settings for this converter.
        /// </summary>
        public GenericConverterSettings Settings { get; }

        /// <summary>
        /// Converter that can be configured with other converters. Converter will use first sub converter that can convert between the supplied types. 
        /// </summary>
        public GenericConverter() : this(GenericConverterSettings.None)
        {

        }
        /// <summary>
        /// Converter that can be configured with other converters. Converter will use first sub converter that can convert between the supplied types. 
        /// </summary>
        /// <param name="settings">Settings to modifiy the behaviour of this converter</param>
        public GenericConverter(GenericConverterSettings settings)
        {
            Settings = settings.ValidateArgument(nameof(settings));
        }

        #region Conversion
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();
            return convertableType.Equals(convertType) || (_converters.HasValue() && _converters.Any(x => x.CanConvert(value, convertType, arguments)));
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();

            try
            {
                if (!Settings.HasFlag(GenericConverterSettings.AlwaysAttemptConversion) && convertableType.IsAssignableTo(convertType))
                {
                    return value;
                }

                var converter = _converters.FirstOrDefault(x => x.CanConvert(value, convertType, arguments));

                if (converter.HasValue())
                {
                    return converter.ConvertTo(value, convertType, arguments);
                }
            }
            catch
            {
                if (!Settings.HasFlag(GenericConverterSettings.IgnoreUnconvertable))
                {
                    throw;
                }
            }

            return convertType.GetDefaultValue();
        }
        #endregion

        #region Config
        /// <summary>
        /// Replaces all current converts with <paramref name="converters"/>. Setting null will clear the converters.
        /// </summary>
        /// <param name="converters">Converters to set</param>
        /// <returns>Self</returns>
        public GenericConverter Set(IEnumerable<ITypeConverter> converters = null)
        {
            _converters.Clear();

            if (converters.HasValue())
            {
                _converters.AddRange(converters);
            }

            return this;
        }

        #region Add
        /// <summary>
        /// Adds a sub converter that the <see cref="GenericConverter"/> can use.
        /// </summary>
        /// <param name="converter">Converter to add</param>
        /// <returns>Self</returns>
        public GenericConverter AddConverter(ITypeConverter converter)
        {
            converter.ValidateArgument(nameof(converter));

            _converters.Add(converter);

            return this;
        }

        /// <summary>
        /// Adds a sub converter with the supplied delegates that the <see cref="GenericConverter"/> can use.
        /// </summary>
        /// <param name="convertFunc">Func that matches method signiture of <see cref="ITypeConverter.ConvertTo(object, Type, IDictionary{string, string})"/></param>
        /// <param name="canConvertFunc">Func that matches method signiture of <see cref="ITypeConverter.CanConvert(object, Type, IDictionary{string, string})"/></param>
        /// <returns>Self</returns>
        public GenericConverter AddConverter(Func<object, Type, IDictionary<string, string>, object> convertFunc, Func<object, Type, IDictionary<string, string>, bool> canConvertFunc = null)
        {
            convertFunc.ValidateArgument(nameof(convertFunc));

            return AddConverter(new DynamicConverter(convertFunc, canConvertFunc));
        }

        /// <summary>
        /// Adds a new sub converter of type <typeparamref name="TConverter"/>.
        /// </summary>
        /// <typeparam name="TConverter">Type of converter</typeparam>
        /// <returns>Self</returns>
        public GenericConverter AddConverter<TConverter>() where TConverter : ITypeConverter, new()
        {
            return AddConverter(new TConverter());
        }
        #endregion

        #region Insert
        /// <summary>
        /// Adds a sub converter that the <see cref="GenericConverter"/> can use and adds it before the first converter with type <paramref name="type"/>.
        /// </summary>
        /// <param name="converter">Converter to add</param>
        /// <param name="type">Type of converter to insert before</param>
        /// <returns>Self</returns>
        public GenericConverter InsertConverter(Type type, ITypeConverter converter)
        {
            type.ValidateArgument(nameof(type));
            converter.ValidateArgument(nameof(converter));         

            _converters.InsertBefore(x => x.HasValue() && x.GetType().Equals(type) ,converter);

            return this;
        }

        /// <summary>
        /// Adds a sub converter with the supplied delegates that the <see cref="GenericConverter"/> can use and adds it before the first converter with type <paramref name="type"/>.
        /// </summary>
        /// <param name="convertFunc">Func that matches method signiture of <see cref="ITypeConverter.ConvertTo(object, Type, IDictionary{string, string})"/></param>
        /// <param name="canConvertFunc">Func that matches method signiture of <see cref="ITypeConverter.CanConvert(object, Type, IDictionary{string, string})"/></param>
        /// <param name="type">Type of converter to insert before</param>
        /// <returns>Self</returns>
        public GenericConverter AddConverter(Type type, Func<object, Type, IDictionary<string, string>, object> convertFunc, Func<object, Type, IDictionary<string, string>, bool> canConvertFunc = null)
        {
            type.ValidateArgument(nameof(type));
            convertFunc.ValidateArgument(nameof(convertFunc));

            return InsertConverter(type, new DynamicConverter(convertFunc, canConvertFunc));
        }

        /// <summary>
        /// Adds a new sub converter of type <typeparamref name="TConverter"/> and adds it before the first converter with type <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="TConverter">Type of converter</typeparam>
        /// <returns>Self</returns>
        public GenericConverter InsertConverter<TConverter>(Type type) where TConverter : ITypeConverter, new()
        {
            type.ValidateArgument(nameof(type));

            return InsertConverter(type, new TConverter());
        }
        #endregion
        #endregion

        // Statics
        #region Globals
        /// <summary>
        /// Default <see cref="GenericConverter"/> that contains sub converters that cover most simple base types.
        /// </summary>
        public static GenericConverter DefaultConverter => new GenericConverter()
                                                                        .AddConverter<DirectoryInfoConverter>()
                                                                        .AddConverter<FileInfoConverter>()
                                                                        .AddConverter<DateTimeConverter>()
                                                                        .AddConverter<EnumConverter>()
                                                                        .AddConverter<GuidConverter>()
                                                                        .AddConverter<GeneralConverter>()
                                                                        .AddConverter<StringConverter>()
                                                                        .AddConverter<CollectionConverter>()
                                                                        .AddConverter<ArrayConverter>();
        /// <summary>
        /// Default <see cref="GenericConverter"/> that can convert between most collection types.
        /// </summary>
        public static GenericConverter DefaultCollectionConverter => new GenericConverter()
                                                                        .AddConverter<CollectionConverter>()
                                                                        .AddConverter<ArrayConverter>();
        /// <summary>
        /// Default <see cref="GenericConverter"/> that contains sub converters that cover most simple base types with support for converting between objects and json strings.
        /// </summary>
        public static GenericConverter DefaultJsonConverter => DefaultConverter.InsertConverter<JsonConverter>(typeof(StringConverter));

        /// <summary>
        /// Default <see cref="GenericConverter"/> that contains sub converters that cover most simple base types with support for converting between objects and xml strings.
        /// </summary>
        public static GenericConverter DefaultXmlConverter => DefaultConverter.InsertConverter<XmlConverter>(typeof(StringConverter));
        #endregion
    }

    /// <summary>
    /// Exposes extra settings for <see cref="GenericConverter"/>.
    /// </summary>
    [Flags]
    public enum GenericConverterSettings
    {
        /// <summary>
        /// No settings selected.
        /// </summary>
        None = 0,
        /// <summary>
        /// Ignore any exceptions thrown by sub converters and will return the default value of the requested type instead.
        /// </summary>
        IgnoreUnconvertable = 1,
        /// <summary>
        /// Always trigger sub converters even when converting to the same type.
        /// </summary>
        AlwaysAttemptConversion = 2
    }
}
