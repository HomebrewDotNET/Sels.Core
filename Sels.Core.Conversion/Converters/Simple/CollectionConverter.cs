using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converter that can convert an <see cref="IEnumerable"/> to other collection types if they can be created using an <see cref="IEnumerable{T}"/> in the constructor.
    /// </summary>
    public class CollectionConverter : ITypeConverter
    {
        /// <inheritdoc/>
        public bool CanConvert(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            convertType.ValidateArgument(nameof(convertType));
            if (value == null) return false;
            var convertableType = value.GetType();

            if (convertableType.IsContainer() && convertType.IsContainer())
            {
                return convertableType.GetElementTypeFromCollection().IsAssignableTo(convertType.GetElementTypeFromCollection()) && convertType.CanConstructWithArguments(value) || convertType.CanConstructWith(typeof(List<>).MakeGenericType(convertableType.GetElementTypeFromCollection()));
            }

            return false;
        }
        /// <inheritdoc/>
        public object ConvertTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            value.ValidateArgument(x => CanConvert(x, convertType, arguments), $"Converter <{this}> cannot convert using the provided value. Call <{nameof(CanConvert)}> first");

            // Convert using source collection
            if (convertType.CanConstructWithArguments(value))
            {
                return convertType.Construct(value);
            }
            // Convert using list
            else
            {
                var list = value.CastTo<IEnumerable>().CreateList(value.GetElementTypeFromCollection());
                return convertType.Construct(list);
            }

           
        }
    }
}
