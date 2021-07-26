using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Components.Conversion.Converters
{
    /// <summary>
    /// Converter that can convert between a path and a directory info.
    /// </summary>
    public class DirectoryInfoConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return value.HasValue() && (convertableType.Is<DirectoryInfo>() && convertType.Is<string>()) || (convertableType.Is<string>() && convertType.Is<DirectoryInfo>());
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            if(convertableType.Is<DirectoryInfo>())
            {
                return value.As<DirectoryInfo>().FullName;
            }
            else
            {
                return new DirectoryInfo(value.ToString());
            }
        }
    }
}
