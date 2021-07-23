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

            if(value != null)
            {
                if (convertableType.IsAssignableTo<Enum>())
                {
                    return convertType.IsAssignableTo<string>() || convertType.IsAssignableTo<int>();
                }
                else if (convertType.IsAssignableTo<Enum>())
                {
                    return convertableType.IsAssignableTo<string>() || convertableType.IsAssignableTo<int>();
                }
            }

            return false;
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            if (convertableType.IsAssignableTo<Enum>())
            {
                return Convert.ChangeType(value, convertType);
            }
            else
            {
                return Enum.Parse(convertType, value.ToString(), true);
            }           
        }
    }
}
