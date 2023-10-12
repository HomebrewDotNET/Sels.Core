using Sels.Core.Conversion.Converters;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using System.Collections.Generic;
using System.Linq;

namespace Sels.Core.Conversion.Extensions
{
    /// <summary>
    /// Contains extension methods for converting objects to other types.
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Attempts to convert <paramref name="value"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="value">Object to convert</param>
        /// <param name="arguments">Arguments for conversion</param>
        /// <returns>Converted object</returns>
        public static T ConvertTo<T>(this object value, IReadOnlyDictionary<string, object> arguments)
        {
            if (value.HasValue())
            {
                return GenericConverter.DefaultConverter.ConvertTo(value, typeof(T), arguments).CastToOrDefault<T>();
            }

            return default;
        }

        /// <summary>
        /// Attempts to convert <paramref name="value"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="value">Object to convert</param>
        /// <param name="arguments">Optional arguments for conversion</param>
        /// <returns>Converted object</returns>
        public static T ConvertTo<T>(this object value, params (string Argument, object Value)[] arguments)
        {
            return value.ConvertTo<T>(arguments.HasValue() ? arguments.ToDictionary(x => x.Argument, x => x.Value) : null);
        }
        /// <summary>
        /// Attempts to convert <paramref name="value"/> to <typeparamref name="T"/>. Will return the default value of <typeparamref name="T"/> when conversion fails.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="value">Object to convert</param>
        /// <param name="arguments">Optional arguments for conversion</param>
        /// <returns>Converted object or default of <typeparamref name="T"/> if the conversion fails</returns>
        public static T ConvertToOrDefault<T>(this object value, IReadOnlyDictionary<string, object> arguments)
        {
            try
            {
                return value.ConvertTo<T>(arguments);
            }
            catch 
            {
                return default;
            }
        }

        /// <summary>
        /// Attempts to convert <paramref name="value"/> to <typeparamref name="T"/>. Will return the default value of <typeparamref name="T"/> when conversion fails.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="value">Object to convert</param>
        /// <param name="arguments">Optional arguments for conversion</param>
        /// <returns>Converted object or default of <typeparamref name="T"/> if the conversion fails</returns>
        public static T ConvertToOrDefault<T>(this object value, params (string Argument, object Value)[] arguments)
        {
            try
            {
                return value.ConvertTo<T>(arguments);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Tries to convert <paramref name="value"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="value">The object to convert</param>
        /// <param name="converted">The converted value</param>
        /// <param name="arguments">Optional argument for the converters</param>
        /// <returns>True if <paramref name="value"/> was succesfully converted, otherwise false</returns>
        public static bool TryConvertTo<T>(this object value, out T converted, IReadOnlyDictionary<string, object> arguments)
        {
            return TypeConverterExtensions.TryConvertTo<T>(GenericConverter.DefaultConverter, value, out converted, arguments);
        }

        /// <summary>
        /// Tries to convert <paramref name="value"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="value">The object to convert</param>
        /// <param name="converted">The converted value</param>
        /// <param name="arguments">Optional argument for the converters</param>
        /// <returns>True if <paramref name="value"/> was succesfully converted, otherwise false</returns>
        public static bool TryConvertTo<T>(this object value, out T converted, params (string Argument, object Value)[] arguments)
        {
            return TryConvertTo<T>(value, out converted, arguments.HasValue() ? arguments.ToDictionary(x => x.Argument, x => x.Value) : null);
        }
    }
}
