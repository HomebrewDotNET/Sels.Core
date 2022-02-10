using Sels.Core.Conversion.Contracts;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Templates.Filters
{
    /// <summary>
    /// Filter that will trim a char from the end of a string and adds them on write.
    /// </summary>
    public class BaseEndTrimFilter : ISerializationFilter
    {
        // Fields
        private readonly char _valueToTrim;

        /// <summary>
        /// Filter that will trim a char from the end of a string and adds them on write.
        /// </summary>
        /// <param name="valueToTrim">The char to trim</param>
        public BaseEndTrimFilter(char valueToTrim)
        {
            _valueToTrim = valueToTrim.ValidateArgument(nameof(valueToTrim));
        }

        /// <inheritdoc/>
        public string ModifyOnRead(string input)
        {
            if (input.HasValue())
            {
                return input.TrimEnd(_valueToTrim);
            }
            return input;
        }
        /// <inheritdoc/>
        public string ModifyOnWrite(string input)
        {
            if (input.HasValue())
            {
                if (!input.EndsWith(_valueToTrim.ToString())) input = input + _valueToTrim;
            }

            return input;
        }
    }
}
