using Sels.Core.Command.Linux.Templates.Attributes;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Command.Linux.Attributes
{
    /// <summary>
    /// Creates argument with flag when source property can be converted to bool and returns true
    /// </summary>
    public class FlagArgument : LinuxArgument
    {
        // Properties
        /// <summary>
        /// Argument value that is used when the source property value is true
        /// </summary>
        public string Flag { get; }

        /// <summary>
        /// Defines an argument that will create a flag when the property value can be converted to true.
        /// </summary>
        /// <param name="flag">String value to generate</param>
        /// <param name="order">Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.</param>
        /// <param name="required">Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.</param>
        public FlagArgument(string flag , int order = LinuxCommandConstants.DefaultLinuxArgumentOrder, bool required = false) : base(true, order, required)
        {
            flag.ValidateArgumentNotNullOrWhitespace(nameof(flag));

            Flag = flag;
        }

        /// <inheritdoc/>
        public override string? CreateArgument(object? value = null)
        {
            if(value != null && bool.TryParse(value.GetArgumentValue(), out bool result) && result)
            {
                return Flag;
            }

            return null;
        }
    }
}
