using Sels.Core.Components.Serialization.KeyValue.Converters;
using Sels.Core.Contracts.Serialization.KeyValue;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Serialization.KeyValue.Attributes
{
    /// <summary>
    /// Used to configure the serialization between the property this is defined on and the string value.
    /// </summary>
    public class KeyValuePropertyAttribute : BaseKeyValueAttribute
    {
        // Statics
        protected static Type DefaultConverter = typeof(TypeConverter);

        // Properties
        /// <summary>
        /// Optional value converter used to convert between the property type and string.
        /// </summary>
        public Type ConverterType { get; }
        /// <summary>
        /// Optional argument that can be passed down to an IKeyValueConverter.
        /// </summary>
        public object ConverterArgument { get; }

        public KeyValuePropertyAttribute(string key = null, Type converterType = null, object converterArgument = null, bool ignore = false) : base(key, ignore)
        {
            ConverterType = converterType?.ValidateArgumentAssignableTo(nameof(converterType), typeof(IKeyValueConverter)).ValidateArgumentCanBeContructedWith(nameof(converterType));
            ConverterArgument = converterArgument;
        }

        public override string[] Serialize(Type propertyType, object propertyValue)
        {
            var converterType = ConverterType ?? DefaultConverter;
            var converter = converterType.Construct<IKeyValueConverter>();

            return converter.ConvertTo(propertyType, propertyValue, ConverterArgument);
        }

        public override object Deserialize(Type propertyType, IEnumerable<string> values, object propertyValue = null)
        {
            var converterType = ConverterType ?? DefaultConverter;
            var converter = converterType.Construct<IKeyValueConverter>();

            return converter.ConvertTo(propertyType, values, ConverterArgument);
        }
    }
}
