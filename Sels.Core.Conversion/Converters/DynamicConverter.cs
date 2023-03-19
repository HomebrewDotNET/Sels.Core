using Sels.Core.Extensions;
using System;
using System.Collections.Generic;

namespace Sels.Core.Conversion.Converters
{
    /// <summary>
    /// Converter that uses delegates to convert objects. 
    /// </summary>
    public class DynamicConverter : ITypeConverter
    {
        // Fields
        private readonly Func<object, Type, IDictionary<string, string>, object> _convertFunc;
        private readonly Func<object, Type, IDictionary<string, string>, bool> _canConvertFunc;

        /// <summary>
        /// Converter that uses delegates to convert objects. 
        /// </summary>
        /// <param name="convertFunc">Func that matches method signiture of <see cref="CanConvert(object, Type, IDictionary{string, string})"/></param>
        /// <param name="canConvertFunc">Func that matches method signiture of <see cref="ConvertTo(object, Type, IDictionary{string, string})"/></param>
        public DynamicConverter(Func<object, Type, IDictionary<string, string>, object> convertFunc, Func<object, Type, IDictionary<string, string>, bool> canConvertFunc = null)
        {
            _convertFunc = convertFunc.ValidateArgument(nameof(convertFunc));
            _canConvertFunc = canConvertFunc;
        }

        /// <inheritdoc/>
        public bool CanConvert(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            value.ValidateArgument(nameof(value));
            convertType.ValidateArgument(nameof(convertType));

            return _canConvertFunc == null || _canConvertFunc(value, convertType, arguments);
        }
        /// <inheritdoc/>
        public object ConvertTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            value.ValidateArgument(nameof(value));
            convertType.ValidateArgument(nameof(convertType));

            return _convertFunc(value, convertType, arguments);
        }
    }
}
