using Sels.Core.Command.Linux.Templates;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Command.Linux.Attributes;

namespace Sels.Core.Command.Linux.Commands.Core
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

        /// <inheritdoc cref="EchoCommand"/>
        public EchoCommand() : base(LinuxCommandConstants.Commands.Echo)
        {

        }
    }
}
