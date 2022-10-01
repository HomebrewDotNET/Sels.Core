using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts between ints/strings and enums.
    /// </summary>
    public class EnumConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();
            return AreTypePair(convertableType, convertType, x => x.IsAssignableTo<Enum>(), x => x.IsAssignableTo<string>() || x.IsAssignableTo<int>());
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            convertType = Nullable.GetUnderlyingType(convertType) ?? convertType;

            if (convertType.IsAssignableTo<Enum>())
            {
                return Enum.Parse(convertType, value.ToString(), true);
            }
            else
            {
                return Convert.ChangeType(value, convertType);
            }
        }
    }
}
