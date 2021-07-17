using Sels.Core.Extensions;
using Sels.Core.Linux.Extensions;
using Sels.Core.Linux.Extensions.Argument;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Attributes
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

        public FlagArgument(string flag , int order = LinuxConstants.DefaultLinuxArgumentOrder, bool required = false) : base(true, order, required)
        {
            flag.ValidateArgumentNotNullOrWhitespace(nameof(flag));

            Flag = flag;
        }

        public override string CreateArgument(object value = null)
        {
            if(value != null && bool.TryParse(value.GetArgumentValue(), out bool result) && result)
            {
                return Flag;
            }

            return null;
        }
    }
}
