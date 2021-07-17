using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Attributes
{
    /// <summary>
    /// Can be used to give a value to an object or enum when using it as an LinuxArgument
    /// </summary>
    public class LinuxValueAttribute : Attribute
    {
        public object Value { get; private set; }

        public LinuxValueAttribute(object value)
        {
            Value = value;
        }
    }
}
