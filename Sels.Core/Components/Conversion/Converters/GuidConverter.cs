using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Conversion.Converters
{
    /// <summary>
    /// Converts objects to Guids.
    /// </summary>
    public class GuidConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return convertType.Equals(typeof(Guid)) && value != null;
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return new Guid(value.ToString());
        }
    }
}
