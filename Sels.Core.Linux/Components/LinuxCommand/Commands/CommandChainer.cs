using Sels.Core.Components.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands
{
    /// <summary>
    /// How 2 commands should be chained together.
    /// </summary>
    public enum CommandChainer
    {
        /// <summary>
        /// Always chain regardless of exit code of previous command.
        /// </summary>
        [EnumValue(";")]
        Always,
        /// <summary>
        /// Only chain if previous command was executed succesfully.
        /// </summary>
        [EnumValue("&&")]
        OnSuccess,
        /// <summary>
        /// Only chain if previous command failed to execute properly.
        /// </summary>
        [EnumValue("||")]
        OnFail,
        /// <summary>
        /// Pipe output from previous command to current command.
        /// </summary>
        [EnumValue("|")]
        Pipe,
        /// <summary>
        /// Links commands with a space
        /// </summary>
        [EnumValue(" ")]
        None
    }
}
