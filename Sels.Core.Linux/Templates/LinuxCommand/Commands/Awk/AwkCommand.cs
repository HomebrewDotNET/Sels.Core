using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Templates.LinuxCommand.Commands.Awk
{
    /// <summary>
    /// Command for manipulating data with the awk scripting language.
    /// </summary>
    public abstract class AwkCommand : BaseLinuxCommand
    {
        // Properties
        [TextArgument("-F", "{Name}{Value}", allowEmpty:false, order:0, required:false)]
        public string FieldSeparator { get; set; }

        public AwkCommand() : base(LinuxConstants.Commands.Awk)
        {

        }
    }
}
