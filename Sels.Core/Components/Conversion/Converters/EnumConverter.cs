using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Conversion.Converters
{
    /// <summary>
    /// Converts objects to Enums.
    /// </summary>
    public class EnumConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return convertType.IsAssignableFrom(typeof(Enum)) && value != null;
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return Enum.Parse(convertType, value.ToString());
        }
    }
}
