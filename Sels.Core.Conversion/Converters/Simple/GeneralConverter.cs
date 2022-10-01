using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converter that can convert between most common simple types.
    /// </summary>
    public class GeneralConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();
            convertableType = Nullable.GetUnderlyingType(convertableType) ?? convertableType;
            convertType = Nullable.GetUnderlyingType(convertType) ?? convertType;

            return AreTypePair(convertableType, convertType, x => IsConvertableType(x), x => IsConvertableType(x));
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            convertType = Nullable.GetUnderlyingType(convertType) ?? convertType;

            return Convert.ChangeType(value, convertType);
        }

        // Statics
        /// <summary>
        /// Checks if this converter can convert <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean indicating if <paramref name="type"/> is supported</returns>
        public static bool IsConvertableType(Type type)
        {
            return type.HasValue() && (ConvertableTypes.Contains(type) || type.IsAssignableTo<IConvertible>());
        }

        /// <summary>
        /// Supported types that this converted can convert between.
        /// </summary>
        public static Type[] ConvertableTypes { get; } = new Type[] { typeof(object), typeof(DBNull), typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(object), typeof(string)
        };
    }
}
