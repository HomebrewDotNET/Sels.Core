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
    /// Converter that can convert between a filename and a file info.
    /// </summary>
    public class FileInfoConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            return value.HasValue() && (convertableType.Is<FileInfo>() && convertType.Is<string>()) || (convertableType.Is<string>() && convertType.Is<FileInfo>());
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            if (convertableType.Is<FileInfo>())
            {
                return value.As<FileInfo>().FullName;
            }
            else
            {
                return new FileInfo(value.ToString());
            }
        }
    }
}
