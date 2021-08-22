using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.Core
{
    /// <summary>
    /// Command that displays text.
    /// </summary>
    public class EchoCommand : BaseLinuxCommand
    {
        // Properties
        /// <summary>
        /// Text to display.
        /// </summary>
        [TextArgument(order: 2, required: true)]
        public string Message { get; set; }
        /// <summary>
        /// Enables the interpretation of backslash escapes in <see cref="Message"/>.
        /// </summary>
        [FlagArgument("-e", order: 1)]
        public bool DoInterpreteBackslash { get; set; }

        public EchoCommand() : base(LinuxConstants.Commands.Echo)
        {

        }
    }
}
