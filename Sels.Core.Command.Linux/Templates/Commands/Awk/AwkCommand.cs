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
        /// <summary>
        /// What string awk can use to split a string into multiple values. Default splitter is whitespace.
        /// </summary>
        [TextArgument("-F", "{Name}{Value}", allowEmpty:false, order:0, required:false)]
        public string FieldSeparator { get; set; }
        /// <inheritdoc cref="AwkCommand"/>
        public AwkCommand() : base(LinuxCommandConstants.Commands.Awk)
        {

        }
    }
}
