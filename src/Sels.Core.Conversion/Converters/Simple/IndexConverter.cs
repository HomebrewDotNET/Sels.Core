using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
#if NETSTANDARD2_1_OR_GREATER
namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts between <see cref="string"/> and <see cref="System.Index"/>.
    /// </summary>
    public class IndexConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            return AreTypePair<string, Index>(value.GetType(), convertType);
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            if (value is Index index && convertType.Is<string>())
            {
                return index.ToString();
            }
            else if (value is string indexString && convertType.GetActualType().Is<Index>())
            {
                if (indexString.HasValue())
                {
                    var isEnd = indexString.StartsWith('^');
                    var orderString = isEnd ? indexString.Substring(1) : indexString;
                    if(int.TryParse(orderString, out var indexInt))
                    {
                        return new Index(indexInt, isEnd);
                    }
                }
                return default(Index);
            }

            throw new NotSupportedException($"Can not convert <{value}> to type <{convertType}>");
        }
    }
}
#endif