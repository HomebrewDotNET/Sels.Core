using Sels.Core.Command.Linux.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Command.Linux.Templates.Commands.Awk
{
    /// <summary>
    /// Command for manipulating data with the awk scripting language.
    /// </summary>
    public abstract class AwkCommand : BaseLinuxCommand
    {
        // Properties
        [TextArgument("-F", "{Name}{Value}", allowEmpty:false, order:0, required:false)]
        public string FieldSeparator { get; set; }

        public AwkCommand() : base(LinuxCommandConstants.Commands.Awk)
        {

        }
    }
}
