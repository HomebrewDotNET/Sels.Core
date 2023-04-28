using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converter that can convert between a string and a collection by splitting / joining the string into multiple elements.
    /// </summary>
    public class StringCollectionConverter : BaseTypeConverter
    {
        // Constants
        /// <summary>
        /// The name of the argument that contains the string to split on and to join the string with. When empty or not provided strings will be split on whitespace and joined with a space character.
        /// </summary>
        public const string SplitterArgument = nameof(StringCollectionConverter) + ".Splitter";

        // Statics
        /// <summary>
        /// The type converter that will be used to convert string elements to the target type.
        /// </summary>
        public static ITypeConverter ElementConverter { get; set; } = GenericConverter.DefaultConverter;
        /// <summary>
        /// The type converter that will be used to convert the list with the converted elements to the requested collection type.
        /// </summary>
        public static ITypeConverter CollectionConverter { get; set; } = GenericConverter.DefaultCollectionConverter;

        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var sourceType = value.GetType();

            return AreTypePair(sourceType, convertType, x => x.IsString(), x => x.IsContainer());
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IDictionary<string, string> arguments = null)
        {
            var sourceType = value.GetType();
            var splitValue = arguments?.GetOrDefault(SplitterArgument);

            // Source is string so we split, convert the elements to the target type and create a instance of the target collection with the elements.
            if (sourceType.IsString())
            {
                var strings = splitValue.IsNullOrEmpty() ? value.ToString().Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries) : value.ToString().Split(splitValue, StringSplitOptions.RemoveEmptyEntries);

                var tempList = new List<object>();
                var elementType = convertType.GetElementTypeFromCollection();
                // Convert elements to target type
                foreach(var toConvert in strings)
                {
                    tempList.Add(ElementConverter.ConvertTo(toConvert, elementType, arguments));
                }

                // Convert to typed list and then convert to target collection
                return CollectionConverter.ConvertTo(tempList.CreateList(elementType), convertType, arguments);

            }
            // Source is collection so we convert the elements to string and join them into a single string.
            else
            {
                var strings = value.CastTo<IEnumerable>().Enumerate().Select(x => ElementConverter.ConvertTo<string>(x, arguments));
                return strings.JoinString(splitValue.IsNullOrEmpty() ? " " : splitValue); 
            }
        }
    }
}
