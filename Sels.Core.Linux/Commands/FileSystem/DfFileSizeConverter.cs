using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Components.FileSizes.Byte.Binary;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Commands.FileSystem
{
    internal class DfFileSizeConverter : IGenericTypeConverter
    {
        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            convertableType.ValidateArgument(nameof(convertableType));
            convertType.ValidateArgument(nameof(convertType));

            if (value.HasValue())
            {
                // From string to file size
                if (convertableType.Is<string>() && convertType.IsAssignableTo<FileSize>())
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

            // From string to file size
            if (convertableType.Is<string>() && convertType.IsAssignableTo<FileSize>())
            {
                var kiloBytes = value.ToString().TrimEnd('K').ConvertTo<decimal>();
                var kiloByteSize = FileSize.CreateFromSize<KibiByte>(kiloBytes);

                // If convert type is the abstract base class we revert to SingleByte
                convertType = convertType.Is<FileSize>() ? typeof(SingleByte) : convertType;

                return kiloByteSize.ToSize(convertType);
            }

            return new SingleByte(0);
        }
    }
}
