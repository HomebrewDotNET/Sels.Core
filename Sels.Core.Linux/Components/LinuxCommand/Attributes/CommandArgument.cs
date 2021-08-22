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
        /// <summary>
        /// Defines an argument on a <see cref="ICommand"/> that will use <see cref="ICommand.BuildCommand"/> to build the argument.
        /// </summary>
        /// <param name="order">Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.</param>
        /// <param name="required">Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.</param>
        public CommandArgument(int order = LinuxConstants.DefaultLinuxArgumentOrder, bool required = false) : base(Selector.Method, nameof(ICommand.BuildCommand), order: order, required: required)
        {

        }
    }
}
