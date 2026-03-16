using Sels.Core.Extensions;
using System;

namespace Sels.Core.Conversion.Serialization.Filters.Conversion
{
    /// <summary>
    /// Filter that will convert 1 and 0 to true and false on read and the opposite on write.
    /// </summary>
    public class BoolToBitFilter : ISerializationFilter
    {
        // Fields
        private readonly string _defaultReadValue = "false";
        private readonly string _defaultWriteValue = "1";

        /// <summary>
        /// Filter that will convert 0 and 1 to true and false on read and the opposite on write.
        /// </summary>
        public BoolToBitFilter()
        {

        }

        /// <summary>
        /// Filter that will convert 0 and 1 to true and false on read and the opposite on write.
        /// </summary>
        /// <param name="defaultReadValue">The value to return when input string isn't 0 or 1 on read</param>
        /// <param name="defaultWriteValue">The value to return when input string isn't true or false on write</param>
        public BoolToBitFilter(string defaultReadValue, string defaultWriteValue)
        {
            _defaultReadValue = defaultReadValue.ValidateArgument(nameof(defaultReadValue));
            _defaultWriteValue = defaultWriteValue.ValidateArgument(nameof(defaultWriteValue));
        }
        /// <inheritdoc/>
        public string ModifyOnRead(string input)
        {
            if(input == "1")
            {
                return "true";
            }
            else if (input == "0")
            {
                return "false";
            }

            return _defaultReadValue;
        }
        /// <inheritdoc/>
        public string ModifyOnWrite(string input)
        {
            if(input != null)
            {
                if (input.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    return "1";
                }
                else if (input.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    return "0";
                }
            }
           
            return _defaultWriteValue;
        }
    }
}
