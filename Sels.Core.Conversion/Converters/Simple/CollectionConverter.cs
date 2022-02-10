using Sels.Core.Conversion.Contracts;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converter that can convert an <see cref="IEnumerable"/> to other collection types if they can be created using an <see cref="IEnumerable{T}"/> in the constructor.
    /// </summary>
    public class CollectionConverter : ITypeConverter
    {
        /// <inheritdoc/>
        public bool CanConvert(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            convertType.ValidateArgument(nameof(convertType));
            if (value == null) return false;
            var convertableType = value.GetType();

            if (convertableType.IsContainer() && convertType.IsContainer())
            {
                return convertableType.GetElementTypeFromCollection().IsAssignableTo(convertType.GetElementTypeFromCollection()) && convertType.CanConstructWith(typeof(List<>).MakeGenericType(convertableType));
            }

            return false;
        }
        /// <inheritdoc/>
        public object ConvertTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            value.ValidateArgument(x => CanConvert(x, convertType, arguments), $"Converter <{this}> cannot convert using the provided value. Call <{nameof(CanConvert)}> first");

            var list = value.Cast<IEnumerable>().CreateList(convertType.GetElementTypeFromCollection());
            return convertType.Construct(list);
        }
    }
}
