using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Conversion.Converters
{
    /// <summary>
    /// Converter that can convert 
    /// </summary>
    public class FileSizeConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            if (value.HasValue())
            {
                // Converting between file sizes
                if (convertableType.IsAssignableTo<FileSize>() && convertType.IsAssignableTo<FileSize>())
                {
                    return true;
                }
                // Converting from file size to byte size/size
                else if(convertableType.IsAssignableTo<FileSize>() && (convertType.Is<long>() || convertType.Is<decimal>()))
                {
                    return true;
                }
                // Converting from byte size/size to file size
                else if ((convertableType.Is<long>() || convertableType.Is<decimal>()) && convertType.IsAssignableTo<FileSize>())
                {
                    return true;
                }
                // From string to file size
                else if (convertableType.Is<string>() && convertType.IsAssignableTo<FileSize>())
                {
                    return true;
                }
                // From file size to string
                else if (convertableType.IsAssignableTo<FileSize>() && convertType.Is<string>())
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            // Converting between file sizes
            if (convertableType.IsAssignableTo<FileSize>() && convertType.IsAssignableTo<FileSize>())
            {
                return value.As<FileSize>().ToSize(convertType);
            }
            // Converting from file size to byte size/size
            else if (convertableType.IsAssignableTo<FileSize>() && (convertType.Is<long>() || convertType.Is<decimal>()))
            {
                var size = value.As<FileSize>();

                return convertType.Is<long>() ? size.ByteSize : size.Size;
            }
            // Converting from byte size/size to file size
            else if (convertType.IsAssignableTo<FileSize>() && (convertableType.Is<long>() || convertableType.Is<decimal>()))
            {
                // If convert type is the abstract base class we revert to SingleByte
                convertType = convertType.Is<FileSize>() ? typeof(SingleByte) : convertType;

                return convertableType.Is<long>() ? FileSize.CreateFromBytes(value.As<long>(), convertType) : FileSize.CreateFromSize(value.As<decimal>(), convertType);
            }
            // From string to file size
            else if (convertableType.Is<string>() && convertType.IsAssignableTo<FileSize>())
            {
                // If convert type is the abstract base class we revert to SingleByte
                convertType = convertType.Is<FileSize>() ? typeof(SingleByte) : convertType;

                var bytes = value.ConvertTo<long>();

                return FileSize.CreateFromBytes(bytes, convertType);
            }
            // From file size to string
            else if (convertableType.IsAssignableTo<FileSize>() && convertType.Is<string>())
            {
                return value.As<FileSize>().ByteSize.ToString();
            }

            return new SingleByte(0);
        }
    }
}
