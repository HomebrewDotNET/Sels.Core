using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converter that can convert between a filename and a file info.
    /// </summary>
    public class FileInfoConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();

            return AreTypePair<string, FileInfo>(convertableType, convertType);
        }
        /// <inheritdoc
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();

            if (convertableType.Is<FileInfo>())
            {
                return value.Cast<FileInfo>().FullName;
            }
            else
            {
                return new FileInfo(value.ToString());
            }
        }
    }
}
