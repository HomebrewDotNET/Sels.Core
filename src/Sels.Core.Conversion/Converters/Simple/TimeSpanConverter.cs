using Microsoft.Extensions.Caching.Memory;
using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts between <see cref="string"/>/<see cref="double"/> and <see cref="TimeSpan"/>.
    /// When converting to/from double the TimeSpan will be converted using the TotalMilliseconds.
    /// </summary>
    public class TimeSpanConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            return AreTypePair<string, TimeSpan>(value.GetType(), convertType) || AreTypePair<double, TimeSpan>(value.GetType(), convertType);
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            if(value is TimeSpan timeSpan)
            {
                if (convertType.IsString())
                {
                    return timeSpan.ToString();
                }
                else if (convertType.Is<double>())
                {
                    return timeSpan.TotalMilliseconds;
                }
            }
            else if (value is string timeSpanString && convertType.Is<TimeSpan>())
            {
                return TimeSpan.Parse(timeSpanString);
            }
            else if (value is double totalMilliseconds && convertType.Is<TimeSpan>())
            {
                return TimeSpan.FromMilliseconds(totalMilliseconds);
            }

            throw new NotSupportedException($"Can not convert <{value}> to type <{convertType}>");
        }
    }
}
