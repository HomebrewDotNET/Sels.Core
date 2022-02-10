using Sels.Core.Conversion.Converters;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Calculation;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static T ConvertTo<T>(this object value, Dictionary<string, string> arguments)
        {
            if (value.HasValue())
            {
                return GenericConverter.DefaultConverter.ConvertTo(value, typeof(T), arguments).CastOrDefault<T>();
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
        public static T ConvertTo<T>(this object value, params (string Argument, string Value)[] arguments)
        {
            return value.ConvertTo<T>(arguments.HasValue() ? arguments.ToDictionary(x => x.Argument, x => x.Value) : null);
        }
    }
}
