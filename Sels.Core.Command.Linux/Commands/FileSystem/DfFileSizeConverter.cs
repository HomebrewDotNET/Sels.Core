using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Components.FileSizes.Byte.Binary;
using Sels.Core.Conversion.Contracts;
using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Conversion.Extensions;

namespace Sels.Core.Command.Linux.Commands.FileSystem
{
    internal class DfFileSizeConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string>? arguments = null)
        {
            var convertableType = value.GetType();

            return convertableType.Is<string>() && convertType.IsAssignableTo<FileSize>();
        }
        /// <inheritdoc
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string>? arguments = null)
        {
            var kiloBytes = value.ToString().TrimEnd('K').ConvertTo<decimal>();
            var kiloByteSize = FileSize.CreateFromSize<KibiByte>(kiloBytes);

            // If convert type is the abstract base class we revert to SingleByte
            convertType = convertType.Is<FileSize>() ? typeof(SingleByte) : convertType;

            return kiloByteSize.ToSize(convertType);
        }
    }
}
