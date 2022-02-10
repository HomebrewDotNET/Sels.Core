using Sels.Core.Conversion.Contracts;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Templates.Filters
{
    /// <summary>
    /// Filter that will trim a char from the start of a string and adds them on write.
    /// </summary>
    public class BaseStartTrimFilter : ISerializationFilter
    {
        // Fields
        private readonly char _valueToTrim;

        /// <summary>
        /// Filter that will trim a char from the start of a string and adds them on write.
        /// </summary>
        /// <param name="valueToTrim">The char to trim</param>
        public BaseStartTrimFilter(char valueToTrim)
        {
            _valueToTrim = valueToTrim.ValidateArgument(nameof(valueToTrim));
        }

        /// <inheritdoc/>
        public string ModifyOnRead(string input)
        {
            if (input.HasValue())
            {
                return input.TrimStart(_valueToTrim);
            }
            return input;
        }
        /// <inheritdoc/>
        public string ModifyOnWrite(string input)
        {
            if (input.HasValue())
            {
                if (!input.StartsWith(_valueToTrim.ToString())) input = _valueToTrim + input;
            }

            return input;
        }
    }
}
