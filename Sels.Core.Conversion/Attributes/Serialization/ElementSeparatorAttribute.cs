using Sels.Core.Extensions;
using Sels.Core.Extensions.Text;
using System;
using System.Collections.Generic;

namespace Sels.Core.Conversion.Attributes.Serialization
{
    /// <summary>
    /// Splits the string into multiple substrings during deseialization that will be converted to a collection. During serialization the elements will be converted to strings and joined instead.
    /// </summary>
    public class ElementSeparatorAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// The string that will be used to split/join the sub strings.
        /// </summary>
        public string Separator { get; }

        /// <summary>
        /// Splits the string into multiple sub string during deseialization that will be converted to a collection. During serialization the elements will be converted to strings and joined instead.
        /// </summary>
        /// <param name="splitter">The string to split/join. If left to null, empty or whitespace the string will be split on whitespace characters</param>
        public ElementSeparatorAttribute(string splitter = null)
        {
            Separator = splitter;
        }

        /// <summary>
        /// Splits up <paramref name="source"/> that needs to be deserialized into multiple sub strings.
        /// </summary>
        /// <param name="source">The string to split</param>
        /// <returns>The split up string</returns>
        public IEnumerable<string> Split(string source)
        {
            source.ValidateArgument(nameof(source));

            return Separator.HasValue() ? source.Split(Separator) : source.Split();
        }

        /// <summary>
        /// Joins <paramref name="source"/> after being serialized.
        /// </summary>
        /// <param name="source">The strings to join</param>
        /// <returns>The joined string</returns>
        public string Join(IEnumerable<string> source)
        {
            source.ValidateArgument(nameof(source));

            return Separator.HasValue() ? source.JoinString(Separator) : source.JoinString(Constants.Strings.Space);
        }
    }
}
