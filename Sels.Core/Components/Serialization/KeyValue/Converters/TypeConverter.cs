using Sels.Core.Components.Conversion;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Contracts.Serialization.KeyValue;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Serialization.KeyValue.Converters
{
    public class TypeConverter : IKeyValueConverter
    {
        // Fields
        private readonly IGenericTypeConverter _defaultConverter;

        public TypeConverter()
        {
            var converter = GenericConverter.DefaultConverter;

            // Don't throw exceptions by default
            converter.Settings.ThrowOnFailedConversion = false;

            _defaultConverter = converter;
        }

        public virtual object ConvertTo(Type conversionType, IEnumerable<string> values, object converterArgument = null)
        {
            // Only use last item as default
            var item = values.LastOrDefault();

            if (item.HasValue())
            {
                return GetConverter(converterArgument).ConvertTo(typeof(string), conversionType, item);
            }

            return conversionType.GetDefaultValue();
        }

        public virtual string[] ConvertTo(Type conversionType, object value, object converterArgument = null)
        {
            return GetConverter(converterArgument).ConvertTo(conversionType, typeof(string), value).AsOrDefault<string>().AsArray();
        }

        private IGenericTypeConverter GetConverter(object converterArgument)
        {
            if(converterArgument != null && converterArgument.IsAssignableFrom<IGenericTypeConverter>() && converterArgument.GetType().CanConstructWith())
            {
                return converterArgument.GetType().Construct<IGenericTypeConverter>();
            }

            return _defaultConverter;
        }
    }
}
