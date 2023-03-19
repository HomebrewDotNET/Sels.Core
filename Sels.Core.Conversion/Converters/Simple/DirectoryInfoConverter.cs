using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converter that can convert between a path and a directory info.
    /// </summary>
    public class DirectoryInfoConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var convertableType = value.GetType();

            return AreTypePair<string, DirectoryInfo>(convertableType, convertType);
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            if (value.GetType().Is<DirectoryInfo>())
            {
                return value.Cast<DirectoryInfo>().FullName;
            }
            else
            {
                return new DirectoryInfo(value.ToString());
            }
        }
    }
}
