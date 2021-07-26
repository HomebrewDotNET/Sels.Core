using Sels.Core.Components.Conversion.Converters;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Conversion
{
    /// <summary>
    /// Converter that can be configured with other converters. Converter will use first sub converter that can convert between the supplied types. 
    /// </summary>
    public class GenericConverter : IGenericTypeConverter
    {
        // Fields
        private readonly List<IGenericTypeConverter> _converters = new List<IGenericTypeConverter>();

        // Properties
        /// <summary>
        /// Current converters used by this converter.
        /// </summary>
        public IGenericTypeConverter[] Converters => _converters.ToArray();
        /// <summary>
        /// Current settings for this converter.
        /// </summary>
        public GenericConverterSettings Settings { get; }

        public GenericConverter() : this(new GenericConverterSettings())
        {

        }

        public GenericConverter(GenericConverterSettings settings)
        {
            Settings = settings.ValidateArgument(nameof(settings));
        }

        #region Conversion
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return _converters.HasValue() && _converters.Any(x => x.CanConvert(convertableType, convertType, value));
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            try
            {
                var converter = _converters.FirstOrDefault(x => x.CanConvert(convertableType, convertType, value));

                if (converter.HasValue())
                {
                    return converter.ConvertTo(convertableType, convertType, value);
                }
            }
            catch
            {
                if (Settings.ThrowOnFailedConversion)
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
        public GenericConverter Set(IEnumerable<IGenericTypeConverter> converters = null)
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
        public GenericConverter AddConverter(IGenericTypeConverter converter)
        {
            converter.ValidateArgument(nameof(converter));

            _converters.Add(converter);

            return this;
        }

        /// <summary>
        /// Adds a sub converter with the supplied delegates that the <see cref="GenericConverter"/> can use.
        /// </summary>
        /// <param name="convertFunc">Func that matches method signiture of <see cref="CanConvert(Type, Type, object)"/></param>
        /// <param name="canConvertFunc">Func that matches method signiture of <see cref="ConvertTo(Type, Type, object)"/></param>
        /// <returns>Self</returns>
        public GenericConverter AddConverter(Func<Type, Type, object, object> convertFunc, Func<Type, Type, object, bool> canConvertFunc = null)
        {
            convertFunc.ValidateArgument(nameof(convertFunc));

            return AddConverter(new DynamicConverter(convertFunc, canConvertFunc));
        }

        /// <summary>
        /// Adds a new sub converter of type <typeparamref name="TConverter"/>.
        /// </summary>
        /// <typeparam name="TConverter">Type of converter</typeparam>
        /// <returns>Self</returns>
        public GenericConverter AddConverter<TConverter>() where TConverter : IGenericTypeConverter, new()
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
        public GenericConverter InsertConverter(Type type, IGenericTypeConverter converter)
        {
            type.ValidateArgument(nameof(type));
            converter.ValidateArgument(nameof(converter));         

            _converters.InsertBefore(x => x.HasValue() && x.GetType().Equals(type) ,converter);

            return this;
        }

        /// <summary>
        /// Adds a sub converter with the supplied delegates that the <see cref="GenericConverter"/> can use and adds it before the first converter with type <paramref name="type"/>.
        /// </summary>
        /// <param name="convertFunc">Func that matches method signiture of <see cref="CanConvert(Type, Type, object)"/></param>
        /// <param name="canConvertFunc">Func that matches method signiture of <see cref="ConvertTo(Type, Type, object)"/></param>
        /// <param name="type">Type of converter to insert before</param>
        /// <returns>Self</returns>
        public GenericConverter AddConverter(Type type, Func<Type, Type, object, object> convertFunc, Func<Type, Type, object, bool> canConvertFunc = null)
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
        public GenericConverter InsertConverter<TConverter>(Type type) where TConverter : IGenericTypeConverter, new()
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
                                                                        .AddConverter<FileSizeConverter>()
                                                                        .AddConverter<DateTimeConverter>()
                                                                        .AddConverter<EnumConverter>()
                                                                        .AddConverter<GuidConverter>()
                                                                        .AddConverter<GeneralConverter>()
                                                                        .AddConverter<StringConverter>();
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
    /// Contains settings that modifies the behaviour of <see cref="GenericConverter"/>.
    /// </summary>
    public class GenericConverterSettings
    {
        /// <summary>
        /// Rethrows any exception that gets thrown during conversion, set to false to return the default value of the convertType.
        /// </summary>
        public bool ThrowOnFailedConversion { get; set; } = true;
    }
}
