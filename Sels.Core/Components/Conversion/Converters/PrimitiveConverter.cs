using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Conversion.Converters
{
    /// <summary>
    /// Converts primitive types and strings to other primitive types.
    /// </summary>
    public class PrimitiveConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return (convertableType.IsAssignableTo<string>() || convertableType.IsPrimitive) && convertType.IsPrimitive;
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return Convert.ChangeType(value, convertType);
        }
    }
}
