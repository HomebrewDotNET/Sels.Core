using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Conversion.Extensions;
using Sels.Core.Components.FileSizes.Byte.Binary;
using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.FileSystem.Templates.FileSizes;

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
        /// <inheritdoc/>
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
