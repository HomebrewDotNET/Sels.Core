using Sels.Core.Contracts.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Attributes
{
    /// <summary>
    /// Creates argument by calling the <see cref="ICommand.BuildCommand"/> method.
    /// </summary>
    public class CommandArgument : ObjectArgument
    {
        public CommandArgument(int order = LinuxConstants.DefaultLinuxArgumentOrder, bool required = false) : base(Selector.Method, nameof(ICommand.BuildCommand), order: order, required: required)
        {

        }
    }
}
