using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Conversion.Contracts
{
    /// <summary>
    /// Converter that can converts objects into other types.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Checks if this converter can convert <paramref name="value"/> to type <paramref name="convertType"/>.
        /// </summary>
        /// <param name="convertType">Type to convert to</param>
        /// <param name="value">Value to convert</param>
        /// <param name="arguments">Arguments to modify the behaviour of this converter</param>
        /// <returns>Boolean indicating if this converter can convert from <paramref name="value"/> to <paramref name="convertType"/></returns>
        bool CanConvert(object value, Type convertType, IDictionary<string, string> arguments = null);

        /// <summary>
        /// Converts <paramref name="value"/> to <paramref name="convertType"/>.
        /// </summary>
        /// <param name="convertType">Type to convert to</param>
        /// <param name="value">Object to convert</param>
        /// <param name="arguments">Arguments to modify the behaviour of this converter</param>
        /// <returns>Converted value</returns>
        object ConvertTo(object value, Type convertType, IDictionary<string, string> arguments = null);
    }

    /// <summary>
    /// Contains extension methods for <see cref="ITypeConverter"/>.
    /// </summary>
    public static class TypeConverterExtensions
    {
        /// <summary>
        /// Converts <paramref name="value"/> to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="converter">The converter to use</param>
        /// <param name="value">The value to convert</param>
        /// <param name="arguments">Optional arguments for the converter</param>
        /// <returns><paramref name="value"/> converted to <typeparamref name="T"/></returns>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="converter"/> cannot convert <paramref name="value"/> to <typeparamref name="T"/></exception>
        public static T ConvertTo<T>(this ITypeConverter converter, object value, IDictionary<string, string> arguments = null)
        {
            converter.ValidateArgument(nameof(converter));
            value.ValidateArgument(nameof(value));
            var type = typeof(T);

            if (!converter.CanConvert(value, type, arguments)) throw new NotSupportedException($"Converter cannot convert type <{value.GetType()}> to <{type}>");
            return converter.ConvertTo(value, type, arguments).Cast<T>();
        }
        /// <summary>
        /// Tries to convert <paramref name="value"/> to an instance of type <paramref name="convertType"/>.
        /// </summary>
        /// <param name="converter">The converter to use</param>
        /// <param name="value">The value to convert</param>
        /// <param name="convertType">The type to convert to</param>
        /// <param name="converted"><paramref name="value"/> converted to <paramref name="convertType"/> if conversion was successful</param>
        /// <param name="arguments">Optional arguments for the converter</param>
        /// <returns>Whether or not <paramref name="value"/> could be converted</returns>
        public static bool TryConvertTo(this ITypeConverter converter, object value, Type convertType, out object converted, IDictionary<string, string> arguments = null)
        {
            converter.ValidateArgument(nameof(converter));
            value.ValidateArgument(nameof(value));
            convertType.ValidateArgument(nameof(convertType));
            converted = convertType.GetDefaultValue();

            try
            {
                if(converter.CanConvert(value, convertType, arguments))
                {
                    converted = converter.ConvertTo(value, convertType, arguments);
                    return true;
                }
            }
            catch { }
            return false;
        }
        /// <summary>
        /// Tries to convert <paramref name="value"/> to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="converter">The converter to use</param>
        /// <param name="value">The value to convert</param>
        /// <param name="converted"><paramref name="value"/> converted to <typeparamref name="T"/> if conversion was successful</param>
        /// <param name="arguments">Optional arguments for the converter</param>
        /// <returns>Whether or not <paramref name="value"/> could be converted</returns>
        public static bool TryConvertTo<T>(this ITypeConverter converter, object value, out T converted, IDictionary<string, string> arguments = null)
        {
            converter.ValidateArgument(nameof(converter));
            value.ValidateArgument(nameof(value));
            converted = default;

            var success = TryConvertTo(converter, value, typeof(T), out var convertedObject, arguments);
            if(success) converted = convertedObject.Cast<T>();
            return success;
        }


        /// <summary>
        /// Converts <paramref name="value"/> to an instance of type <paramref name="convertType"/>.
        /// </summary>
        /// <param name="converters">The converters to use</param>
        /// <param name="value">The value to convert</param>
        /// <param name="convertType">The type to convert to</param>
        /// <param name="arguments">Optional arguments for the converter</param>
        /// <returns><paramref name="value"/> converted to <paramref name="convertType"/></returns>
        /// <exception cref="NotSupportedException">Thrown when no converter can convert <paramref name="value"/> to <paramref name="convertType"/></exception>
        public static object ConvertTo(this IEnumerable<ITypeConverter> converters, object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            converters.ValidateArgument(nameof(converters));
            value.ValidateArgument(nameof(value));
            convertType.ValidateArgument(nameof(convertType));

            var converter = converters.FirstOrDefault(x => x.CanConvert(value, convertType, arguments));

            return converter != null ? converter.ConvertTo(value, convertType, arguments) : throw new NotSupportedException($"No converter can convert type <{value.GetType()}> to <{convertType}>");
        }
        /// <summary>
        /// Converts <paramref name="value"/> to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="converters">The converters to use</param>
        /// <param name="value">The value to convert</param>
        /// <param name="arguments">Optional arguments for the converter</param>
        /// <returns><paramref name="value"/> converted to <typeparamref name="T"/></returns>
        /// <exception cref="NotSupportedException">Thrown when no converter can convert <paramref name="value"/> to <typeparamref name="T"/></exception>
        public static T ConvertTo<T>(this IEnumerable<ITypeConverter> converters, object value, IDictionary<string, string> arguments = null)
        {
            converters.ValidateArgument(nameof(converters));
            value.ValidateArgument(nameof(value));
            var type = typeof(T);

            return converters.ConvertTo(value, type, arguments).Cast<T>();
        }
        /// <summary>
        /// Tries to convert <paramref name="value"/> to an instance of type <paramref name="convertType"/>.
        /// </summary>
        /// <param name="converters">The converters to use</param>
        /// <param name="value">The value to convert</param>
        /// <param name="convertType">The type to convert to</param>
        /// <param name="converted"><paramref name="value"/> converted to <paramref name="convertType"/> if conversion was successful</param>
        /// <param name="arguments">Optional arguments for the converters</param>
        /// <returns>Whether or not <paramref name="value"/> could be converted</returns>
        public static bool TryConvertTo(this IEnumerable<ITypeConverter> converters, object value, Type convertType, out object converted, IDictionary<string, string> arguments = null)
        {
            converters.ValidateArgument(nameof(converters));
            value.ValidateArgument(nameof(value));
            convertType.ValidateArgument(nameof(convertType));
            converted = convertType.GetDefaultValue();

            foreach (var converter in converters)
            {
                try
                {

                    if (converter.CanConvert(value, convertType, arguments))
                    {
                        converted = converter.ConvertTo(value, convertType, arguments);
                        return true;
                    }
                }
                catch { }
            }
            
            return false;
        }
        /// <summary>
        /// Tries to convert <paramref name="value"/> to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="converters">The converters to use</param>
        /// <param name="value">The value to convert</param>
        /// <param name="converted"><paramref name="value"/> converted to <typeparamref name="T"/> if conversion was successful</param>
        /// <param name="arguments">Optional arguments for the converter</param>
        /// <returns>Whether or not <paramref name="value"/> could be converted</returns>
        public static bool TryConvertTo<T>(this IEnumerable<ITypeConverter> converters, object value, out T converted, IDictionary<string, string> arguments = null)
        {
            converters.ValidateArgument(nameof(converters));
            value.ValidateArgument(nameof(value));
            converted = default;

            var success = TryConvertTo(converters, value, typeof(T), out var convertedObject, arguments);
            if (success) converted = convertedObject.Cast<T>();
            return success;
        }
    }
}
