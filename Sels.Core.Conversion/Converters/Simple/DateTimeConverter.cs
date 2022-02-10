using Sels.Core.Conversion.Contracts;
using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts objects to DateTime.
    /// </summary>
    public class DateTimeConverter : BaseTypeConverter
    {
        //Constants
        /// <summary>
        /// The argument for providing a custom date format.
        /// </summary>
        public const string FormatArgument = "Format";

        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();

            return AreTypePair<string, DateTime>(convertableType, convertType);
        }
        /// <inheritdoc
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            string format = arguments.HasValue() && arguments.TryGetValue(FormatArgument, out var formatValue) ? formatValue : null;

            // Convert string to date
            if (convertType.IsAssignableTo<DateTime>())
            {
                var dateString = value.ToString();
                return format.HasValue() ? DateTime.ParseExact(dateString, format, null) : DateTime.Parse(dateString);
            }
            // Convert date to string
            else
            {
                var date = value.Cast<DateTime>();
                return format.HasValue() ? date.ToString(format) : date.ToString();
            }
        }
    }
}
