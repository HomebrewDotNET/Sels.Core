using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Linux.Extensions.Argument;

namespace Sels.Core.Linux.Components.LinuxCommand.Attributes
{
    /// <summary>
    /// Can be used to give a value to an object or enum when using it as an LinuxArgument. Value will be fetched using <see cref="LinuxArgumentExtensions.GetArgumentValue(object)"/>.
    /// </summary>
    public class LinuxValueAttribute : Attribute
    {
        public object Value { get; private set; }

        /// <summary>
        /// Defines a custom value for an object.
        /// </summary>
        /// <param name="value">Value for the object</param>
        public LinuxValueAttribute(object value)
        {
            Value = value;
        }
    }
}
