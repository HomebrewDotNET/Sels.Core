using Sels.Core.Conversion.Serialization.Filters;
using Sels.Core.Extensions;

namespace Sels.Core.Conversion.Templates.Filters
{
    /// <summary>
    /// Filter that will trim a char on read and adds them on write.
    /// </summary>
    public abstract class BaseTrimFilter : ISerializationFilter
    {
        // Fields
        private readonly char _valueToTrim;

        /// <summary>
        /// Filter that will trim a char on read and adds them on write.
        /// </summary>
        /// <param name="valueToTrim">The char to trim</param>
        public BaseTrimFilter(char valueToTrim)
        {
            _valueToTrim = valueToTrim.ValidateArgument(nameof(valueToTrim));
        }

        /// <inheritdoc/>
        public string ModifyOnRead(string input)
        {
            if (input.HasValue())
            {
                return input.Trim(_valueToTrim);
            }
            return input;
        }
        /// <inheritdoc/>
        public string ModifyOnWrite(string input)
        {
            if (input.HasValue())
            {
                if (!input.StartsWith(_valueToTrim.ToString())) input = _valueToTrim + input;
                if (!input.EndsWith(_valueToTrim.ToString())) input = input + _valueToTrim;
            }

            return input;
        }
    }
}
