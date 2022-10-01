using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Command.Linux.Commands
{
    /// <summary>
    /// How 2 commands should be chained together.
    /// </summary>
    public enum CommandChainer
    {
        /// <summary>
        /// Always chain regardless of exit code of previous command.
        /// </summary>
        [StringEnumValue(";")]
        Always,
        /// <summary>
        /// Only chain if previous command was executed succesfully.
        /// </summary>
        [StringEnumValue("&&")]
        OnSuccess,
        /// <summary>
        /// Only chain if previous command failed to execute properly.
        /// </summary>
        [StringEnumValue("||")]
        OnFail,
        /// <summary>
        /// Pipe output from previous command to current command.
        /// </summary>
        [StringEnumValue("|")]
        Pipe,
        /// <summary>
        /// Links commands with a space
        /// </summary>
        [StringEnumValue(" ")]
        None
    }
}
