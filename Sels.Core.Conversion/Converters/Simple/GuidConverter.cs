using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts objects to Guids.
    /// </summary>
    public class GuidConverter : BaseTypeConverter
    {
        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            return convertType.IsAssignableTo<Guid>();
        }
        /// <inheritdoc
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            return new Guid(value.ToString());
        }
    }
}
