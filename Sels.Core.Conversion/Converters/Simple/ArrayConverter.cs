using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converter that can convert collections to an array.
    /// </summary>
    public class ArrayConverter : ITypeConverter
    {
        /// <inheritdoc/>
        public bool CanConvert(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            convertType.ValidateArgument(nameof(convertType));
            if (value == null) return false;
            var convertableType = value.GetType();

            if(convertType.IsArray && convertableType.IsAssignableTo<IEnumerable>())
            {
                return convertableType.GetElementTypeFromCollection().IsAssignableTo(convertType.GetElementTypeFromCollection());
            }

            return false;
        }
        /// <inheritdoc/>
        public object ConvertTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            value.ValidateArgument(x => CanConvert(x, convertType, arguments), $"Converter <{this}> cannot convert using the provided value. Call <{nameof(CanConvert)}> first");

            var values = value.CastTo<IEnumerable>().Enumerate().ToArray();
            var array = convertType.Construct(values.Length).CastTo<Array>();
            for (int i = 0; i < values.Length; i++)
            {
                array.SetValue(values[i], i);
            }
            return array;
        }
    }
}
