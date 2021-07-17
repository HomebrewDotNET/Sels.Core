using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Conversion
{
    /// <summary>
    /// Converter that uses delegates to convert objects. 
    /// </summary>
    public class DynamicConverter : IGenericTypeConverter
    {
        // Fields
        private readonly Func<Type, Type, object, object> _convertFunc;
        private readonly Func<Type, Type, object, bool> _canConvertFunc;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="convertFunc">Func that matches method signiture of <see cref="CanConvert(Type, Type, object)"/></param>
        /// <param name="canConvertFunc">Func that matches method signiture of <see cref="ConvertTo(Type, Type, object)"/></param>
        public DynamicConverter(Func<Type, Type, object, object> convertFunc, Func<Type, Type, object, bool> canConvertFunc = null)
        {
            _convertFunc = convertFunc.ValidateArgument(nameof(convertFunc));
            _canConvertFunc = canConvertFunc;
        }

        public bool CanConvert(Type convertableType, Type convertType, object value)
        {
            return _canConvertFunc == null || _canConvertFunc(convertableType, convertType, value);
        }

        public object ConvertTo(Type convertableType, Type convertType, object value)
        {
            return _convertFunc(convertableType, convertType, value);
        }
    }
}
