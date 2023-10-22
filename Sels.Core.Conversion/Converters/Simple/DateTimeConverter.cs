using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts objects to DateTime.
    /// </summary>
    public class DateTimeConverter : BaseTypeConverter
    {
        // Constants
        /// <summary>
        /// The argument for providing a custom date format.
        /// </summary>
        public const string FormatArgument = "DateTime.Format";

        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            var convertableType = value.GetType();

            return AreTypePair<string, DateTime>(convertableType, convertType);
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            string format = arguments.HasValue() && arguments.TryGetValue<string>(FormatArgument, out var formatValue) ? formatValue : null;

            convertType = Nullable.GetUnderlyingType(convertType) ?? convertType;

            // Convert string to date
            if (convertType.IsAssignableTo<DateTime>())
            {
                var dateString = value.ToString();
                return format.HasValue() ? DateTime.ParseExact(dateString, format, null) : DateTime.Parse(dateString);
            }
            // Convert date to string
            else
            {
                var date = value.CastTo<DateTime>();
                return format.HasValue() ? date.ToString(format) : date.ToString();
            }
        }
    }
}
